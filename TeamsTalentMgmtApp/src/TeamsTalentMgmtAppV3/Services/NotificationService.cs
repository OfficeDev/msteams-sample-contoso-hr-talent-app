using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using AutoMapper;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TeamsTalentMgmtAppV3.Constants;
using TeamsTalentMgmtAppV3.Extensions;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;
using TeamsTalentMgmtAppV3.Services.Data;
using TeamsTalentMgmtAppV3.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Services
{
    public sealed class NotificationService : INotificationService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IMapper _mapper;

        public NotificationService(DatabaseContext databaseContext,
            IMapper mapper)
        {
            _databaseContext = databaseContext;
            _mapper = mapper;
        }

        public async Task AddSubscriber(string webhookUrl, CancellationToken cancellationToken)
        {
            var subscription = await _databaseContext.SubscribeEvents.FirstOrDefaultAsync(x => string.Equals(x.WebhookUrl, webhookUrl, StringComparison.OrdinalIgnoreCase), cancellationToken);
            if (subscription is null)
            {
                await _databaseContext.SubscribeEvents.AddAsync(new SubscribeEvent
                {
                    WebhookUrl = webhookUrl
                }, cancellationToken);
                await _databaseContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RemoveSubscriber(string webhookUrl, CancellationToken cancellationToken)
        {
            var subscription = await _databaseContext.SubscribeEvents.FirstOrDefaultAsync(x => string.Equals(x.WebhookUrl, webhookUrl, StringComparison.OrdinalIgnoreCase), cancellationToken);
            if (subscription != null)
            {
                _databaseContext.Remove(subscription);
                await _databaseContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task NotifyAboutStageChange(Candidate candidate, CancellationToken cancellationToken)
        {
            var subscribers = await _databaseContext.SubscribeEvents.ToListAsync(cancellationToken);
            var card = JsonConvert.SerializeObject(_mapper.Map<O365ConnectorCard>(candidate));

            var taskList = new List<Task>(subscribers.Count);
            foreach (var subscriber in subscribers)
            {
                taskList.Add(PostCardAsync(subscriber.WebhookUrl, card));
            }

            await Task.WhenAll(taskList.ToArray());
        }

        public async Task NotifyRecruiterAboutNewOpenPosition(Position position, CancellationToken cancellationToken = default)
        {
            var recruiter = _databaseContext.Recruiters.Find(position.HiringManagerId);
            if (recruiter?.TeamsChannelData is null)
            {
                return;
            }

            var client = new ConnectorClient(new Uri(recruiter.TeamsChannelData.ServiceUrl));
            var appId = ConfigurationManager.AppSettings["MicrosoftAppId"];
            var bot = new ChannelAccount(appId);
            var recipient = new ChannelAccount(recruiter.TeamsChannelData.AccountId);
            
            var conversation = client.Conversations.CreateOrGetDirectConversation(bot, recipient, recruiter.TeamsChannelData.TenantId);
            var message = new Activity
            {
                Text = "You have a new position assigned to you.",
                Type = ActivityTypes.Message,
                Conversation = new ConversationAccount
                {
                    Id = conversation.Id
                } 
            };

            var card = _mapper.Map<AdaptiveCard>(position);
            
            var staticTabEntityId = "OpenPositionsTab"; // you can find this value in manifest definition
            var staticTabName = "Assigned to you";
            
            card.Actions.Add(new AdaptiveOpenUrlAction
            {
                Title = "Show all assigned positions",
                Url = new Uri(string.Format(CommonConstants.DeepLinkUrlFormat, appId, staticTabEntityId, staticTabName))
            });
            message.Attachments = new List<Attachment>
            {
                card.ToAttachment()
            };

            message.NotifyUser();
            await client.Conversations.SendToConversationAsync(message, cancellationToken);
        }

        private static Task PostCardAsync(string webhook, string cardJson)
        {
            var content = new StringContent(cardJson, Encoding.UTF8, "application/json");
            return HttpClientFactory.Create().PostAsync(webhook, content);
        }
    }
}