using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TeamsTalentMgmtAppV4.Models;
using TeamsTalentMgmtAppV4.Models.TemplateModels;
using TeamsTalentMgmtAppV4.Services.Interfaces;
using TeamsTalentMgmtAppV4.Services.Templates;
using TeamTalentMgmtApp.Shared.Constants;
using TeamTalentMgmtApp.Shared.Models.Commands;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;
using TeamTalentMgmtApp.Shared.Services.Interfaces;

namespace TeamsTalentMgmtAppV4.Services
{
    public class BotService : IBotService
    {
        private readonly AppSettings _appSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IRecruiterService _recruiterService;
        private readonly IPositionService _positionService;
        private readonly ICandidateService _candidateService;
        private readonly IInterviewService _interviewService;
        private readonly ILocationService _locationService;
        private readonly PositionsTemplate _positionsTemplate;
        private readonly NewJobPostingToAdaptiveCardTemplate _newJobPostingTemplate;
        private readonly CandidatesTemplate _candidatesTemplate;

        public BotService(
            IOptions<AppSettings> appSettings,
            IHttpClientFactory httpClientFactory,
            IRecruiterService recruiterService,
            IPositionService positionService,
            ICandidateService candidateService,
            IInterviewService interviewService,
            ILocationService locationService,
            CandidatesTemplate candidatesTemplate,
            PositionsTemplate positionsTemplate,
            NewJobPostingToAdaptiveCardTemplate newJobPostingTemplate)
        {
            _appSettings = appSettings.Value;
            _httpClientFactory = httpClientFactory;
            _recruiterService = recruiterService;
            _positionService = positionService;
            _candidateService = candidateService;
            _interviewService = interviewService;
            _locationService = locationService;
            _candidatesTemplate = candidatesTemplate;
            _positionsTemplate = positionsTemplate;
            _newJobPostingTemplate = newJobPostingTemplate;
        }

        public async Task HandleMembersAddedAsync(
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            if (turnContext.Activity?.MembersAdded != null)
            {
                var teamsContext = turnContext.TurnState.Get<ITeamsContext>();
                var channelAccounts = await turnContext.TurnState.Get<IConnectorClient>().Conversations.GetConversationMembersAsync(turnContext.Activity.Conversation.Id, cancellationToken);

                if (turnContext.Activity.MembersAdded.All(m => m.Id != turnContext.Activity.Recipient?.Id))
                {
                    // Bot was added before so grab information about new members.
                    channelAccounts = channelAccounts.Where(ca => turnContext.Activity.MembersAdded.Any(member => member.Id == ca.Id)).ToList();
                }
                else
                {
                    if (turnContext.Activity.Conversation.IsGroup != true)
                    {
                        var card = new HeroCard
                        {
                            Title = "Hi, I'm Talent bot!",
                            Text = "I can assist you with creating new job postings, get details about your candidates, open positions and notify about your candidates stage updates. If you are admin, you can install bot for hiring managers.",
                            Buttons = new List<CardAction>
                            {
                                new CardAction(ActionTypes.ImBack, value: BotCommands.InstallBotDialogCommand, title: "Install bot for hiring managers"),
                                new CardAction(ActionTypes.ImBack, value: BotCommands.HelpDialogCommand, title: "Help")
                            }
                        };

                        await turnContext.SendActivityAsync(MessageFactory.Attachment(card.ToAttachment()), cancellationToken);
                    }
                }

                await _recruiterService.SaveConversationData(
                    turnContext.Activity.ServiceUrl,
                    teamsContext.Tenant?.Id,
                    channelAccounts
                    .Select(member => teamsContext.AsTeamsChannelAccount(member))
                    .ToDictionary(channelAccount => channelAccount.Id, channelAccount => channelAccount.Email),
                    cancellationToken);
            }
        }

        public async Task<IMessageActivity> LeaveInternalCommentAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            var commandData = JsonConvert.DeserializeObject<LeaveCommentCommand>(turnContext.Activity.Value?.ToString());
            if (commandData is null)
            {
                return null;
            }

            await _candidateService.AddComment(commandData, turnContext.Activity.From.Name, cancellationToken);
            var candidate = await _candidateService.GetById(commandData.CandidateId, cancellationToken);
            var interviewers = await _recruiterService.GetAllInterviewers(cancellationToken);

            var templateModel = new CandidateTemplateModel
            {
                Items = new List<Candidate> { candidate },
                Interviewers = interviewers,
                AppSettings = _appSettings,
                NoItemsLabel = "You don't have such candidate."
            };

            var messageActivity = await _candidatesTemplate.RenderTemplate(turnContext, null, TemplateConstants.CandidateAsAdaptiveCardWithMultipleItems, templateModel);

            return messageActivity;
        }

        public async Task<IMessageActivity> OpenPositionAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            var commandData = JsonConvert.DeserializeObject<PositionCreateCommand>(turnContext.Activity.Value?.ToString());
            if (commandData is null)
            {
                return null;
            }

            var position = await _positionService.AddNewPosition(commandData, cancellationToken);
            var positionsTemplate = new PositionTemplateModel
            {
                Items = new List<Position> { position }
            };

            var messageActivity = await _positionsTemplate.RenderTemplate(turnContext, null, TemplateConstants.PositionAsAdaptiveCardWithMultipleItems, positionsTemplate);

            return messageActivity;
        }

        public async Task<IMessageActivity> ScheduleInterviewAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            var commandData = JsonConvert.DeserializeObject<ScheduleInterviewCommand>(turnContext.Activity.Value?.ToString());
            if (commandData is null)
            {
                return null;
            }

            await _interviewService.ScheduleInterview(commandData, cancellationToken);
            var candidate = await _candidateService.GetById(commandData.CandidateId, cancellationToken);
            var interviewers = await _recruiterService.GetAllInterviewers(cancellationToken);

            var templateModel = new CandidateTemplateModel
            {
                Items = new List<Candidate> { candidate },
                Interviewers = interviewers,
                AppSettings = _appSettings,
                NoItemsLabel = "You don't have such candidate."
            };

            var messageActivity = await _candidatesTemplate.RenderTemplate(turnContext, null, TemplateConstants.CandidateAsAdaptiveCardWithMultipleItems, templateModel);

            return messageActivity;
        }

        public async Task HandleFileAttachments(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var description = await TryToExtractDescriptionFromFile(turnContext.Activity.Attachments);
            if (!string.IsNullOrEmpty(description))
            {
                var locations = await _locationService.GetAllLocations(cancellationToken);
                var hiringManagers = await _recruiterService.GetAllHiringManagers(cancellationToken);

                await _newJobPostingTemplate.ReplyWith(turnContext, nameof(NewJobPostingToAdaptiveCardTemplate), new
                {
                    Locations = locations,
                    HiringManagers = hiringManagers,
                    Description = description
                });
            }
        }

        private async Task<string> TryToExtractDescriptionFromFile(IEnumerable<Attachment> attachments)
        {
            var attachment = attachments.First(); // assuming that user sends only one file
            if (attachment.ContentType == FileDownloadInfo.ContentType)
            {
                var downloadInfo = JsonConvert.DeserializeObject<FileDownloadInfo>(attachment.Content.ToString());
                if (downloadInfo != null)
                {
                    var client = _httpClientFactory.CreateClient();

                    // downloadUrl is an unauthenticated URL to the file contents, valid for only a few minutes
                    return await client.GetStringAsync(downloadInfo.DownloadUrl);
                }
            }

            return string.Empty;
        }
    }
}
