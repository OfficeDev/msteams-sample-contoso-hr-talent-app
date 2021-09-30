using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AdaptiveCards;
using AutoMapper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeamsTalentMgmtAppV4.Models;
using TeamsTalentMgmtAppV4.Models.TemplateModels;
using TeamsTalentMgmtAppV4.Services.Interfaces;
using TeamsTalentMgmtAppV4.Services.Templates;
using TeamTalentMgmtApp.Shared.Constants;
using TeamTalentMgmtApp.Shared.Models.Bot;
using TeamTalentMgmtApp.Shared.Models.Commands;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;
using TeamTalentMgmtApp.Shared.Services.Interfaces;

namespace TeamsTalentMgmtAppV4.Services
{
    public class InvokeActivityHandler : IInvokeActivityHandler
    {
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly ICandidateService _candidateService;
        private readonly IInterviewService _interviewService;
        private readonly IPositionService _positionService;
        private readonly IRecruiterService _recruiterService;
        private readonly ILocationService _locationService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly PositionsTemplate _positionsTemplate;
        private readonly CandidatesTemplate _candidatesTemplate;
        private readonly NewJobPostingToAdaptiveCardTemplate _newJobPostingToAdaptiveCardTemplate;

        public InvokeActivityHandler(
            PositionsTemplate positionsTemplate,
            CandidatesTemplate candidatesTemplate,
            NewJobPostingToAdaptiveCardTemplate newJobPostingToAdaptiveCardTemplate,
            ICandidateService candidateService,
            IInterviewService interviewService,
            IPositionService positionService,
            IRecruiterService recruiterService,
            ILocationService locationService,
            IOptions<AppSettings> appSettings,
            IHttpClientFactory clientFactory,
            IMapper mapper)
        {
            _appSettings = appSettings.Value;
            _newJobPostingToAdaptiveCardTemplate = newJobPostingToAdaptiveCardTemplate;
            _positionsTemplate = positionsTemplate;
            _candidatesTemplate = candidatesTemplate;
            _candidateService = candidateService;
            _positionService = positionService;
            _interviewService = interviewService;
            _recruiterService = recruiterService;
            _locationService = locationService;
            _clientFactory = clientFactory;
            _mapper = mapper;
        }

        public async Task<InvokeResponse> HandleMessagingExtensionQueryAsync(
            ITurnContext turnContext,
            MessagingExtensionQuery query,
            CancellationToken cancellationToken)
        {
            var initialRunParameter = GetQueryParameterByName(query, "initialRun");

            // situation where the incoming payload was received from the config popup
            if (!string.IsNullOrEmpty(query.State))
            {
                initialRunParameter = bool.TrueString;
            }

            var isInitialRun = string.Equals(initialRunParameter, bool.TrueString, StringComparison.OrdinalIgnoreCase);
            var maxResults = isInitialRun ? 5 : query.QueryOptions.Count ?? 25;

            var attachments = new List<MessagingExtensionAttachment>();
            var searchText = GetQueryParameterByName(query, MessagingExtensionCommands.SearchTextParameterName);

            switch (query.CommandId)
            {
                case MessagingExtensionCommands.SearchPositions:
                    var positions = await _positionService.Search(searchText, maxResults, cancellationToken);
                    foreach (var position in positions)
                    {
                        var positionsTemplate = new PositionTemplateModel
                        {
                            Items = new List<Position> { position },
                        };

                        var mainCard = await _positionsTemplate.RenderTemplate(turnContext, null, TemplateConstants.PositionAsAdaptiveCardWithMultipleItems, positionsTemplate);
                        var previewCard = await _positionsTemplate.RenderTemplate(turnContext, null, TemplateConstants.PositionAsThumbnailCardWithMultipleItems, positionsTemplate);
                        attachments.Add(mainCard.Attachments.First().ToMessagingExtensionAttachment(previewCard.Attachments.First()));
                    }

                    break;

                case MessagingExtensionCommands.SearchCandidates:
                    var candidates = await _candidateService.Search(searchText, maxResults, cancellationToken);
                    var interviewers = await _recruiterService.GetAllInterviewers(cancellationToken);
                    foreach (var candidate in candidates)
                    {
                        var templateModel = new CandidateTemplateModel
                        {
                            Items = new List<Candidate> { candidate },
                            Interviewers = interviewers,
                            AppSettings = _appSettings,
                        };

                        var mainCard = await _candidatesTemplate.RenderTemplate(turnContext, null, TemplateConstants.CandidateAsAdaptiveCardWithMultipleItems, templateModel);
                        var previewCard = await _candidatesTemplate.RenderTemplate(turnContext, null, TemplateConstants.CandidateAsThumbnailCardWithMultipleItems, templateModel);
                        attachments.Add(mainCard.Attachments.First().ToMessagingExtensionAttachment(previewCard.Attachments.First()));
                    }

                    break;
            }

            return _mapper.Map<MessagingExtensionAttachment[], InvokeResponse>(attachments.ToArray());
        }

        public async Task<InvokeResponse> HandleMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var composeExtensionQuery = turnContext.TurnState.Get<ITeamsContext>().GetMessagingExtensionActionData();
            MessagingExtensionActionResponse response = null;
            if (string.Equals(composeExtensionQuery?.CommandId, MessagingExtensionCommands.OpenNewPosition, StringComparison.OrdinalIgnoreCase))
            {
                var locations = await _locationService.GetAllLocations(cancellationToken);
                var hiringManagers = await _recruiterService.GetAllHiringManagers(cancellationToken);

                var messageActivity = await _newJobPostingToAdaptiveCardTemplate.RenderTemplate(turnContext, null, nameof(NewJobPostingToAdaptiveCardTemplate), new
                {
                    Locations = locations,
                    HiringManagers = hiringManagers,
                    Description = string.Empty
                });

                response = new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Type = "continue",
                        Value = new TaskModuleTaskInfo
                        {
                            Card = messageActivity.Attachments.First(),
                            Title = "Create new job posting",
                            Width = "large",
                            Height = "large"
                        }
                    }
                };
            }

            return _mapper.Map<MessagingExtensionActionResponse, InvokeResponse>(response);
        }

        public async Task<InvokeResponse> HandleMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var submitActionData = turnContext.TurnState.Get<ITeamsContext>().GetMessagingExtensionActionData();
            var positionCreateCommand = JObject.FromObject(submitActionData?.Data).ToObject<PositionCreateCommand>();
            var response = new InvokeResponse { Status = (int)HttpStatusCode.OK };
            if (positionCreateCommand is null)
            {
                return response;
            }

            switch (positionCreateCommand.CommandId)
            {
                // Open task module confirming job posting
                case AppCommands.OpenNewPosition:
                    response = await CreateConfirmJobPostingTaskModuleResponse(turnContext, positionCreateCommand, cancellationToken);
                    break;

                // Insert card confirming the action
                case AppCommands.ConfirmCreationOfNewPosition:
                    response = await PositionMessagingExtensionResponse(turnContext, positionCreateCommand.PositionId);
                    break;
            }

            return response;
        }

        public async Task<InvokeResponse> HandleMessagingExtensionOnCardButtonClickedAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var data = turnContext.TurnState.Get<ITeamsContext>().GetMessagingExtensionQueryData();
            switch (data.CommandId)
            {
                case AppCommands.LeaveInternalComment:
                    var leaveCommentRequest = JsonConvert.DeserializeObject<LeaveCommentCommand>(turnContext.Activity?.Value?.ToString());
                    await _candidateService.AddComment(leaveCommentRequest, turnContext.Activity?.From.Name, cancellationToken);
                    break;
                case AppCommands.ScheduleInterview:
                    var scheduleInterviewRequest = JsonConvert.DeserializeObject<ScheduleInterviewCommand>(turnContext.Activity?.Value?.ToString());
                    await _interviewService.ScheduleInterview(scheduleInterviewRequest, cancellationToken);
                    break;
            }

            return new InvokeResponse { Status = (int)HttpStatusCode.OK };
        }

        public async Task<InvokeResponse> HandleSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, SigninStateVerificationQuery query, CancellationToken cancellationToken)
        {
            var connectionName = _appSettings.OAuthConnectionName;
            var token = await ((IUserTokenProvider)turnContext.Adapter).GetUserTokenAsync(
                turnContext,
                connectionName,
                query.State,
                cancellationToken);
            if (token != null && !string.IsNullOrEmpty(token.Token))
            {
                await turnContext.SendActivityAsync("You have signed in successfully. Please type command one more time.", cancellationToken: cancellationToken);
            }

            return new InvokeResponse { Status = (int)HttpStatusCode.OK };
        }

        public async Task<InvokeResponse> HandleAppBasedLinkQueryAsync(
            ITurnContext<IInvokeActivity> turnContext,
            AppBasedLinkQuery query,
            CancellationToken cancellationToken)
        {
            var url = query?.Url?.Trim() ?? string.Empty;

            var result = new InvokeResponse { Status = (int)HttpStatusCode.OK };
            if (url.Contains("OpenPositionsPersonalTab"))
            {
                var positionIdParam = HttpUtility.ParseQueryString(new Uri(url).Query).Get("positionId");
                if (int.TryParse(positionIdParam, out var positionId))
                {
                    result = await PositionMessagingExtensionResponse(turnContext, positionId);
                }
            }

            return result;
        }

        public async Task<InvokeResponse> HandleFileConsentDeclineResponse(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(turnContext.Activity.ReplyToId))
            {
                await turnContext.DeleteActivityAsync(turnContext.Activity.ReplyToId, cancellationToken);
            }

            return new InvokeResponse { Status = (int)HttpStatusCode.OK };
        }

        public async Task<InvokeResponse> HandleFileConsentAcceptResponse(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken = default)
        {
            var consentAcceptResponse = new InvokeResponse { Status = (int)HttpStatusCode.OK };

            var responseContext = JsonConvert.DeserializeObject<FileConsentContext>(fileConsentCardResponse.Context?.ToString());
            if (responseContext is null)
            {
                return consentAcceptResponse;
            }

            var candidate = await _candidateService.GetById(responseContext.CandidateId, cancellationToken);
            if (candidate is null)
            {
                return consentAcceptResponse;
            }

            IMessageActivity reply;
            try
            {
                // Upload the file contents to the upload session we got from the invoke value
                // See https://docs.microsoft.com/en-us/onedrive/developer/rest-api/api/driveitem_createuploadsession#upload-bytes-to-the-upload-session
                var bytes = Encoding.UTF8.GetBytes(candidate.Summary);
                using (var stream = new MemoryStream(bytes))
                {
                    using (var content = new StreamContent(stream))
                    {
                        content.Headers.ContentRange = new ContentRangeHeaderValue(0, bytes.LongLength - 1, bytes.LongLength);
                        content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Octet);
                        var client = _clientFactory.CreateClient();
                        var response = await client.PutAsync(fileConsentCardResponse.UploadInfo.UploadUrl, content, cancellationToken);
                        response.EnsureSuccessStatusCode();
                    }
                }

                if (!string.IsNullOrEmpty(turnContext.Activity.ReplyToId))
                {
                    await turnContext.DeleteActivityAsync(turnContext.Activity.ReplyToId, cancellationToken);
                }

                // Send the user a link to the uploaded file
                var fileInfoCard = new FileInfoCard
                {
                    FileType = fileConsentCardResponse.UploadInfo.FileType,
                    UniqueId = fileConsentCardResponse.UploadInfo.UniqueId
                };

                reply = MessageFactory.Attachment(fileInfoCard.ToAttachment(fileConsentCardResponse.UploadInfo.Name, fileConsentCardResponse.UploadInfo.ContentUrl));
            }
            catch (Exception ex)
            {
                reply = MessageFactory.Text($"There was an error uploading the file: {ex.Message}");
            }

            await turnContext.SendActivityAsync(reply, cancellationToken);
            return consentAcceptResponse;
        }

        private async Task<InvokeResponse> PositionMessagingExtensionResponse(ITurnContext<IInvokeActivity> turnContext, int positionId)
        {
            var position = await _positionService.GetById(positionId);
            if (position == null)
            {
                return new InvokeResponse
                {
                    Status = (int)HttpStatusCode.OK
                };
            }

            var positionsTemplate = new PositionTemplateModel
            {
                Items = new List<Position> { position },
            };

            var mainCard = await _positionsTemplate.RenderTemplate(turnContext, null, TemplateConstants.PositionAsAdaptiveCardWithMultipleItems, positionsTemplate);
            var previewCard = await _positionsTemplate.RenderTemplate(turnContext, null, TemplateConstants.PositionAsThumbnailCardWithMultipleItems, positionsTemplate);

            var attachment = mainCard.Attachments.First().ToMessagingExtensionAttachment(previewCard.Attachments.First());
            var messagingExtensionResponse = _mapper.Map<MessagingExtensionAttachment[], MessagingExtensionResponse>(new[] { attachment });
            return _mapper.Map<MessagingExtensionResponse, InvokeResponse>(messagingExtensionResponse);
        }

        private async Task<InvokeResponse> CreateConfirmJobPostingTaskModuleResponse(ITurnContext turnContext, PositionCreateCommand positionCreateCommand, CancellationToken cancellationToken)
        {
            var position = await _positionService.AddNewPosition(positionCreateCommand, cancellationToken);

            positionCreateCommand.CommandId = AppCommands.ConfirmCreationOfNewPosition;
            positionCreateCommand.PositionId = position.PositionId;

            var positionsTemplate = new PositionTemplateModel
            {
                Items = new List<Position> { position },
                ButtonActions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Confirm posting",
                        Data = positionCreateCommand
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = "Cancel"
                    }
                }
            };

            var messageActivity = await _positionsTemplate.RenderTemplate(turnContext, null, TemplateConstants.PositionAsAdaptiveCardWithMultipleItems, positionsTemplate);

            var actionResponse = new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Type = "continue",
                    Value = new TaskModuleTaskInfo
                    {
                        Card = messageActivity.Attachments.First(),
                        Title = "Confirm new posting",
                        Width = "medium",
                        Height = "medium"
                    }
                }
            };

            return _mapper.Map<MessagingExtensionActionResponse, InvokeResponse>(actionResponse);
        }

        private static string GetQueryParameterByName(MessagingExtensionQuery query, string name)
        {
            if (query?.Parameters == null || query.Parameters.Count == 0)
            {
                return string.Empty;
            }

            var parameter = query.Parameters[0];
            if (!string.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return parameter.Value != null ? parameter.Value.ToString() : string.Empty;
        }
    }
}
