//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using TeamTalentMgmtApp.Shared.Models.DatabaseContext;
//using TeamTalentMgmtApp.Shared.Services.Interfaces;

//namespace TeamsTalentMgmtAppV4.Controllers
//{
//    [ApiController]
//    public class ConnectorController : ControllerBase
//    {
//        private readonly INotificationService _notificationService;

//        public ConnectorController(INotificationService notificationService)
//        {
//            _notificationService = notificationService;
//        }

//        [HttpPost]
//        [Route("api/connector/subscribe")]
//        public async Task<ActionResult> Subscribe(
//            [FromForm] SubscribeEvent subscribeEvent,
//            CancellationToken cancellationToken)
//        {
//            await _notificationService.AddSubscriber(subscribeEvent?.WebhookUrl, cancellationToken);
//            return Ok();
//        }

//        [HttpDelete]
//        [Route("api/connector/unsubscribe")]
//        public async Task<ActionResult> Unsubscribe(
//            [FromQuery] string webHookUrl,
//            CancellationToken cancellationToken)
//        {
//            await _notificationService.RemoveSubscriber(webHookUrl, cancellationToken);
//            return Ok();
//        }
//    }
//}
