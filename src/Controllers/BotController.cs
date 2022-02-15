using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using TeamsTalentMgmtApp.Services.Interfaces;

namespace TeamsTalentMgmtApp.Controllers
{
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBot _bot;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly INotificationService _notificationService;

        public BotController(IBot bot, IBotFrameworkHttpAdapter adapter, INotificationService notificationService)
        {
            _bot = bot;
            _adapter = adapter;
            _notificationService = notificationService;
        }

        [HttpPost]
        [Route("api/messages")]
        public Task PostMessageAsync(CancellationToken cancellationToken)
            => _adapter.ProcessAsync(Request, Response, _bot, cancellationToken);


        [HttpPost]
        [Route("api/notify")]
        public async Task<IActionResult> PostAsync([FromBody] UserTenantMessageRequest request, CancellationToken cancellationToken)
        {
            var activity = MessageFactory.Text("This is a proactive notification from the api/notify");
            var success = false;

            if (request.Id.Contains("@"))
            {
                success = await _notificationService.SendProactiveNotificationByUpn(request.Id, request.TenantId, activity, cancellationToken);
            }
            else
            {
                success = await _notificationService.SendProactiveNotificationByAlias(request.Id, request.TenantId, activity, cancellationToken);
            }
            

            if (!success)
            {
                // Precondition failed - app not installed!
                return StatusCode(412);
            }

            return Accepted();
        }
    }

    public class UserTenantMessageRequest
    {
        public string Id { get; set; }
        public string TenantId { get; set; }
    }
}
