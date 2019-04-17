using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Autofac;
using AutoMapper;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using Newtonsoft.Json;
using TeamsTalentMgmtAppV3.Constants;
using TeamsTalentMgmtAppV3.Extensions;
using TeamsTalentMgmtAppV3.Models.Bot;
using TeamsTalentMgmtAppV3.Models.Commands;
using TeamsTalentMgmtAppV3.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Services
{
    public sealed class BotService : IBotService
    {
        private readonly IPositionService _positionService;
        private readonly ICandidateService _candidateService;
        private readonly IRecruiterService _recruiterService;
        private readonly IInterviewService _interviewService;
        private readonly ITemplateService _templateService;
        private readonly IMapper _mapper;

        public BotService(IPositionService positionService,
            ICandidateService candidateService,
            IRecruiterService recruiterService,
            IInterviewService interviewService,
            ITemplateService templateService,
            IMapper mapper)
        {
            _positionService = positionService;
            _candidateService = candidateService;
            _interviewService = interviewService;
            _templateService = templateService;
            _recruiterService = recruiterService;
            _mapper = mapper;
        }
        public async Task<bool> HandleAdaptiveCardAction(Activity activity, CancellationToken cancellationToken)
        {
            var command = JsonConvert.DeserializeObject<ActionCommandBase>(activity.Value?.ToString());
            if (string.IsNullOrEmpty(command?.CommandId))
            {
                return false;
            }

            AdaptiveCard result = null;
            switch (command.CommandId)
            {
                case AppCommands.OpenNewPosition:
                    result = await OpenNewPosition(activity, cancellationToken);
                    break;
                
                case AppCommands.LeaveInternalComment:
                    result = await LeaveInternalComment(activity, cancellationToken);
                    break;
                
                case AppCommands.ScheduleInterview:
                    result = await ScheduleInterview(activity, cancellationToken);
                    break;
            }

            if (result is null)
            {
                return false;
            }
            
            // update activity
            var message = Activity.CreateMessageActivity();
            var client = new ConnectorClient(new Uri(activity.ServiceUrl));
            message.Attachments.Add(result.ToAttachment());  
            await client.Conversations.UpdateActivityAsync(activity.Conversation.Id, activity.ReplyToId, (Activity) message, cancellationToken);

            return true;
        }

        public async Task HandleConversationUpdate(Activity activity, CancellationToken cancellationToken)
        {
            // We're only interested in member added events
            if (activity.MembersAdded?.Count > 0)
            {
                var client = new ConnectorClient(new Uri(activity.ServiceUrl));
                var conversationMembers = await client.Conversations.GetConversationMembersAsync(activity.Conversation.Id, cancellationToken);
                var channelAccounts = conversationMembers.AsTeamsChannelAccounts();
               
                var membersAddedIdsList = activity.MembersAdded.Select(x => x.Id).ToList();
                
                var botId = activity.Recipient.Id;
                var botWasAdded = membersAddedIdsList.Contains(botId);
                if (!botWasAdded) // save information about new users only
                {
                    channelAccounts = channelAccounts.Where(x => membersAddedIdsList.Contains(x.Id));
                }

                await _recruiterService.SaveTeamsChannelData(activity.ServiceUrl, activity.GetTenantId(), channelAccounts.ToList(), cancellationToken);
            }
        }

        public async Task HandleMessage(Activity activity, CancellationToken cancellationToken)
        {
            if (activity.HasFileAttachments())
            {
                var description = await TryToExtractDescriptionFromFile(activity.Attachments);
                if (description.HasValue())
                {
                    var reply = activity.CreateReply();
                    var card = _templateService.GetAdaptiveCardForNewJobPosting(description);
                    
                    reply.Attachments = new List<Attachment>
                    {
                        card.ToAttachment()
                    };
                    
                    var client = new ConnectorClient(new Uri(activity.ServiceUrl));
                    await client.Conversations.ReplyToActivityAsync(reply, cancellationToken);
                }
            }
            else  // continue process for text messages
            {
                activity.SendTypingActivity();
                activity.Text = activity.GetTextWithoutMentions();
                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                {
                    await Conversation.SendAsync(activity, () => scope.Resolve<IDialog<object>>(), cancellationToken);
                }
            }
        }

        public async Task HandleFileConsentResponse(Activity activity, CancellationToken cancellationToken)
        {
            var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl));
            
            var fileConsentCardResponse = JsonConvert.DeserializeObject<FileConsentCardResponse>(activity.Value?.ToString());
            if (fileConsentCardResponse is null)
            {
                return;
            }

            switch (fileConsentCardResponse.Action)
            {
                case FileConsentCardResponse.DeclineAction:
                    if (activity.ReplyToId.HasValue())
                    {
                        // Delete the file consent card
                        await connectorClient.Conversations.DeleteActivityAsync(activity.Conversation.Id, activity.ReplyToId, cancellationToken);
                    }
                    break;
                case FileConsentCardResponse.AcceptAction:
                    activity.SendTypingActivity();
                    await SendFileToUser(fileConsentCardResponse, connectorClient, activity, cancellationToken);
                    break;
            }
        }

        private async Task SendFileToUser(FileConsentCardResponse fileConsentCardResponse, ConnectorClient connectorClient, Activity activity, CancellationToken cancellationToken)
        {
            var responseContext = JsonConvert.DeserializeObject<FileConsentContext>(fileConsentCardResponse.Context?.ToString());
            if (responseContext is null)
            {
                return;
            }

            var candidate = await _candidateService.GetById(responseContext.CandidateId);
            if (candidate is null)
            {
                return;
            }

            var reply = activity.CreateReply();
                 
            try
            {
                // Upload the file contents to the upload session we got from the invoke value
                // See https://docs.microsoft.com/en-us/onedrive/developer/rest-api/api/driveitem_createuploadsession#upload-bytes-to-the-upload-session
                var bytes = Encoding.UTF8.GetBytes(candidate.Summary);
                var uploadInfoUploadUrl = fileConsentCardResponse.UploadInfo.UploadUrl;
                using (var stream = new MemoryStream(bytes))
                {
                    using (var content = new StreamContent(stream))
                    {
                        content.Headers.ContentRange = new ContentRangeHeaderValue(0, bytes.LongLength - 1, bytes.LongLength);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        var response = await HttpClientFactory.Create().PutAsync(uploadInfoUploadUrl, content, cancellationToken);
                        response.EnsureSuccessStatusCode();
                    }
                }
                
                if (activity.ReplyToId.HasValue())
                {
                    await connectorClient.Conversations.DeleteActivityAsync(activity.Conversation.Id, activity.ReplyToId, cancellationToken);
                }
                
                // Send the user a link to the uploaded file
                var fileInfoCard = FileInfoCard.FromFileUploadInfo(fileConsentCardResponse.UploadInfo);
                reply.Attachments = new List<Attachment>
                {
                    fileInfoCard.ToAttachment()
                };
                
                await connectorClient.Conversations.ReplyToActivityAsync(reply, cancellationToken);
            }
            catch (Exception ex)
            {
                reply.Text = $"There was an error uploading the file: {ex.Message}";
                await connectorClient.Conversations.ReplyToActivityAsync(reply, cancellationToken);
            }
        }

        private static async Task<string> TryToExtractDescriptionFromFile(IEnumerable<Attachment> attachments)
        {
            var attachment = attachments.First(); // assuming that user sends only one file 
            if (attachment.ContentType == FileDownloadInfo.ContentType)
            {
                var downloadInfo = JsonConvert.DeserializeObject<FileDownloadInfo>(attachment.Content.ToString());
                if (downloadInfo != null)
                {
                    // downloadUrl is an unauthenticated URL to the file contents, valid for only a few minutes
                    return await HttpClientFactory.Create().GetStringAsync(downloadInfo.DownloadUrl);
                }
            }

            return string.Empty;
        }

        private async Task<AdaptiveCard> OpenNewPosition(Activity activity, CancellationToken cancellationToken)
        {
            var commandData = JsonConvert.DeserializeObject<PositionCreateCommand>(activity.Value?.ToString());
            if (commandData is null)
            {
                return null;
            }

            var position = await _positionService.AddNewPosition(commandData, cancellationToken);
            return _mapper.Map<AdaptiveCard>(position);
        }

        private async Task<AdaptiveCard> LeaveInternalComment(Activity activity, CancellationToken cancellationToken)
        {
            var commandData = JsonConvert.DeserializeObject<LeaveCommentCommand>(activity.Value?.ToString());
            if (commandData is null)
            {
                return null;
            }
            await _candidateService.AddComment(commandData, activity.From.Name, cancellationToken);
            var candidate = await _candidateService.GetById(commandData.CandidateId);
            return _mapper.Map<AdaptiveCard>(candidate);
        }
        
        private async Task<AdaptiveCard> ScheduleInterview(Activity activity, CancellationToken cancellationToken)
        {
            var commandData = JsonConvert.DeserializeObject<ScheduleInterviewCommand>(activity.Value?.ToString());
            if (commandData is null)
            {
                return null;
            }
            await _interviewService.ScheduleInterview(commandData, cancellationToken);
            var candidate = await _candidateService.GetById(commandData.CandidateId);
            return _mapper.Map<AdaptiveCard>(candidate);
        }
    }
}