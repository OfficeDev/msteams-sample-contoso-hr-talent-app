using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using TeamsTalentMgmtApp.Models;
using TeamsTalentMgmtApp.Services.Interfaces;

namespace TeamsTalentMgmtApp.Controllers
{
    public class NotifierService : INotifierService
    {
        private readonly IGraphApiService _graphApiService;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IBotFrameworkHttpAdapter _adapter;

        public NotifierService(IGraphApiService graphApiService, IOptions<AppSettings> appSettings, IBotFrameworkHttpAdapter adapter)
        {
            _graphApiService = graphApiService;
            _appSettings = appSettings;
            _adapter = adapter;
        }

        public async Task<bool> SendProactiveNotification(string upnOrOid, string tenantId, IActivity activityToSend, CancellationToken cancellationToken = default)
        {
            var chatId = await _graphApiService.GetProactiveChatIdForUser(tenantId, upnOrOid, cancellationToken);

            if (chatId == null)
            {
                return false;
            }

            var credentials = new MicrosoftAppCredentials(_appSettings.Value.MicrosoftAppId, _appSettings.Value.MicrosoftAppPassword);

            var connectorClient = new ConnectorClient(new Uri(_appSettings.Value.ServiceUrl), credentials);

            var members = await connectorClient.Conversations.GetConversationMembersAsync(chatId);

            var conversationParameters = new ConversationParameters
            {
                IsGroup = false,
                Bot = new ChannelAccount
                {
                    Id = "28:" + credentials.MicrosoftAppId,
                    Name = "This is your bot!"
                },
                Members = new ChannelAccount[] { members[0] },
                TenantId = tenantId
            };

            await ((CloudAdapter)_adapter).CreateConversationAsync(credentials.MicrosoftAppId, null, _appSettings.Value.ServiceUrl, credentials.OAuthScope, conversationParameters, async (t1, c1) =>
            {
                var conversationReference = t1.Activity.GetConversationReference();
                await ((CloudAdapter)_adapter).ContinueConversationAsync(credentials.MicrosoftAppId, conversationReference, async (t2, c2) =>
                {
                    await t2.SendActivityAsync(activityToSend, c2);
                }, cancellationToken);
            }, cancellationToken);

            return true;
        }
    }
}
