using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using TeamsTalentMgmtAppV3.Extensions;
using TeamsTalentMgmtAppV3.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly IMessagingExtensionService _messagingExtensionService;
        private readonly IBotService _botService;

        public MessagesController(IMessagingExtensionService messagingExtensionService,
            IBotService botService)
        {
            _messagingExtensionService = messagingExtensionService;
            _botService = botService;
        }

        [HttpPost]
        [Route("api/messages")]
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity, CancellationToken cancellationToken)
        {
            if (activity is null)
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            
            if (activity.IsAdaptiveCardActionQuery())
            {
               var isHandled = await _botService.HandleAdaptiveCardAction(activity, cancellationToken);
               if (isHandled)
               {
                   return Request.CreateResponse(HttpStatusCode.Accepted);
               }
            }

            switch (activity.GetActivityType())
            {
                case ActivityTypes.ConversationUpdate:
                    await _botService.HandleConversationUpdate(activity, cancellationToken);
                    break;

                case ActivityTypes.Message:
                    await _botService.HandleMessage(activity, cancellationToken);
                    break;

                case ActivityTypes.Invoke:
                    return await HandleInvoke(activity, cancellationToken);
            }

            return Request.CreateResponse(HttpStatusCode.Accepted);
        }

		private async Task<HttpResponseMessage> HandleInvoke(Activity activity, CancellationToken cancellationToken)
        {
            if (activity.IsSigninStateVerificationQuery())
            {
                await _botService.HandleMessage(activity, cancellationToken);
            }
            
            if (activity.IsComposeExtensionQuery())
            {
                return await _messagingExtensionService.HandleInvokeRequest(Request, activity, cancellationToken);
            }
            
            if (activity.IsFileConsentCardResponse())
            {
                await _botService.HandleFileConsentResponse(activity, cancellationToken);
            }

            // Return empty response.
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}