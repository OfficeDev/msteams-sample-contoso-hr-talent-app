using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;
using TeamsTalentMgmtAppV3.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Controllers
{
    public class ConnectorController : ApiController
    {
        private readonly INotificationService _notificationService;

        public ConnectorController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        [Route("api/connector/subscribe")]
        public async Task<IHttpActionResult> Subscribe([FromBody] SubscribeEvent subscribeEvent, CancellationToken cancellationToken)
        {
            await _notificationService.AddSubscriber(subscribeEvent?.WebhookUrl, cancellationToken);
            return Ok();
        }

        [HttpDelete]
        [Route("api/connector/unsubscribe")]
        public async Task<IHttpActionResult> Unsubscribe([FromUri] string webHookUrl, CancellationToken cancellationToken)
        {
            await _notificationService.RemoveSubscriber(webHookUrl, cancellationToken);
            return Ok();
        }
    }
}