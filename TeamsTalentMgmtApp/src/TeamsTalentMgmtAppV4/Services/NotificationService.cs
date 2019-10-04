using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamsTalentMgmtAppV4.Models;
using TeamsTalentMgmtAppV4.Models.TemplateModels;
using TeamsTalentMgmtAppV4.Services.Interfaces;
using TeamsTalentMgmtAppV4.Services.Templates;
using TeamTalentMgmtApp.Shared.Constants;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;
using TeamTalentMgmtApp.Shared.Services.Data;
using TeamTalentMgmtApp.Shared.Services.Interfaces;

namespace TeamsTalentMgmtAppV4.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IConnectorService _connectorService;
        private readonly DatabaseContext _databaseContext;
        private readonly IRecruiterService _recruiterService;
        private readonly PositionsTemplate _positionsTemplate;
        private readonly CandidatesTemplate _candidatesTemplate;
        private readonly AppSettings _appSettings;

        public NotificationService(
            PositionsTemplate positionsTemplate,
            CandidatesTemplate candidatesTemplate,
            IOptions<AppSettings> appSettings,
            DatabaseContext databaseContext,
            IRecruiterService recruiterService,
            IConnectorService connectorService)
        {
            _connectorService = connectorService;
            _positionsTemplate = positionsTemplate;
            _candidatesTemplate = candidatesTemplate;
            _appSettings = appSettings.Value;
            _recruiterService = recruiterService;
            _databaseContext = databaseContext;
        }

        public async Task AddSubscriber(string webhookUrl, CancellationToken cancellationToken)
        {
            var subscription = await _databaseContext
                .SubscribeEvents
                .FirstOrDefaultAsync(x => string.Equals(x.WebhookUrl, webhookUrl, StringComparison.OrdinalIgnoreCase), cancellationToken);
            if (subscription is null)
            {
                await _databaseContext.SubscribeEvents.AddAsync(
                    new SubscribeEvent
                    {
                        WebhookUrl = webhookUrl
                    }, cancellationToken);
                await _databaseContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task NotifyAboutStageChange(
            Candidate candidate,
            CancellationToken cancellationToken = default)
        {
            var subscribers = await _databaseContext.SubscribeEvents.ToArrayAsync(cancellationToken);
            var configurationCompletedCard = new O365ConnectorCardSection(
                activityTitle: $"Stage has been changed for {candidate.Name}",
                activityImage: $"{_appSettings.BaseUrl}/api/candidates/{candidate.CandidateId}/profilePicture",
                facts: new List<O365ConnectorCardFact>
                {
                                new O365ConnectorCardFact("Stage:", $"**{candidate.PreviousStage.ToString()}** --> **{candidate.Stage.ToString()}**"),
                                new O365ConnectorCardFact("Current role:", candidate.CurrentRole),
                                new O365ConnectorCardFact("Location:", candidate.Location?.LocationAddress ?? string.Empty),
                                new O365ConnectorCardFact("Position applied:", candidate.Position.Title),
                                new O365ConnectorCardFact("Phone number:", candidate.Phone)
                },
                markdown: true);

            if (candidate.Comments.Any() || candidate.Interviews.Any())
            {
                var contentUrl = $"{_appSettings.BaseUrl}/StaticViews/CandidateFeedback.html?{Uri.EscapeDataString($"candidateId={candidate.CandidateId}")}";

                var openFeedback = new Uri(string.Format(
                    CommonConstants.TaskModuleUrlFormat,
                    _appSettings.TeamsAppId,
                    contentUrl,
                    "Feedback for " + candidate.Name,
                    _appSettings.MicrosoftAppId,
                    "large",
                    "large"));

                configurationCompletedCard.PotentialAction = new List<O365ConnectorCardActionBase>
                {
                    new O365ConnectorCardViewAction(O365ConnectorCardViewAction.Type)
                    {
                        Name = "Open candidate feedback",
                        Target = new List<string> { openFeedback.ToString() }
                    }
                };
            }

            var connectorCard = new O365ConnectorCard
            {
                Sections = new List<O365ConnectorCardSection> { configurationCompletedCard },
                Summary = $"Stage has been changed for {candidate.Name}"
            };

            var taskList = subscribers.Select(subscriber => _connectorService.SendToChannelAsync(subscriber.WebhookUrl, connectorCard, cancellationToken));
            await Task.WhenAll(taskList);
        }

        public async Task NotifyRecruiterAboutCandidateStageChange(
            Candidate candidate,
            CancellationToken cancellationToken = default)
        {
            if (candidate?.Position != null)
            {
                var recruiter = await _recruiterService.GetById(candidate.Position.HiringManagerId, cancellationToken);
                if (recruiter?.ConversationData is null)
                {
                    return;
                }

                var interviewers = await _recruiterService.GetAllInterviewers(cancellationToken);
                var templateModel = new CandidateTemplateModel
                {
                    Items = new List<Candidate> { candidate },
                    Interviewers = interviewers,
                    AppSettings = _appSettings
                };

                var attachments = (await _candidatesTemplate.RenderTemplate(null, null, TemplateConstants.CandidateAsAdaptiveCardWithMultipleItems, templateModel)).Attachments;

                await SendToConversation($"Candidate stage has been changed for {candidate.Name} from {candidate.PreviousStage} to {candidate.Stage}", attachments, recruiter.ConversationData, cancellationToken);
            }
        }

        public async Task NotifyRecruiterAboutNewOpenPosition(
            Position position,
            CancellationToken cancellationToken = default)
        {
            var recruiter = await _recruiterService.GetById(position.HiringManagerId, cancellationToken);
            if (recruiter?.ConversationData is null)
            {
                return;
            }

            var staticTabEntityId = "OpenPositionsTab"; // you can find this value in manifest definition
            var staticTabName = "Potential candidates";

            PositionTemplateModel positionsTemplate = new PositionTemplateModel
            {
                Items = new List<Position> { position },
                ButtonActions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Title = "Show all assigned positions",
                        Url = new Uri(string.Format(CommonConstants.DeepLinkUrlFormat, _appSettings.MicrosoftAppId, staticTabEntityId, staticTabName))
                    }
                }
            };

            var attachments = (await _positionsTemplate.RenderTemplate(null, null, TemplateConstants.PositionAsAdaptiveCardWithMultipleItems, positionsTemplate)).Attachments;

            await SendToConversation("You have a new position assigned to you.", attachments, recruiter.ConversationData, cancellationToken);
        }

        public async Task RemoveSubscriber(string webhookUrl, CancellationToken cancellationToken = default)
        {
            var subscription = await _databaseContext
                .SubscribeEvents
                .FirstOrDefaultAsync(x => string.Equals(x.WebhookUrl, webhookUrl, StringComparison.OrdinalIgnoreCase), cancellationToken);
            if (subscription != null)
            {
                _databaseContext.Remove(subscription);
                await _databaseContext.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task SendToConversation(
            string text,
            IList<Attachment> attachments,
            ConversationData conversationData,
            CancellationToken cancellationToken)
        {
            MicrosoftAppCredentials.TrustServiceUrl(conversationData.ServiceUrl);
            using (var client = new ConnectorClient(
                new Uri(conversationData.ServiceUrl),
                new MicrosoftAppCredentials(_appSettings.MicrosoftAppId, _appSettings.MicrosoftAppPassword)))
            {
                var conversationParameters = new ConversationParameters
                {
                    Bot = new ChannelAccount(_appSettings.MicrosoftAppId),
                    ChannelData = new TeamsChannelData
                    {
                        Tenant = new TenantInfo
                        {
                            Id = conversationData.TenantId
                        }
                    },
                    Members = new List<ChannelAccount> { new ChannelAccount(conversationData.AccountId) }
                };

                var conversation = await client.Conversations.CreateConversationAsync(conversationParameters, cancellationToken);

                var activity = MessageFactory.Text(text);
                activity.Conversation = new ConversationAccount { Id = conversation.Id };
                activity.ChannelData = new TeamsChannelData
                {
                    Notification = new NotificationInfo
                    {
                        Alert = true
                    }
                };

                if (attachments != null && attachments.Count > 0)
                {
                    activity.Attachments = attachments;
                }

                await client.Conversations.SendToConversationAsync(activity, cancellationToken);
            }
        }
    }
}
