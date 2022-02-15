using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using TeamsTalentMgmtApp.Services.Interfaces;

namespace TeamsTalentMgmtApp.Controllers
{
    [ApiController]
    [Route("api/notify")]
    public class NotifyController : ControllerBase
    {
        private readonly INotifierService _notifier;

        public NotifyController(INotifierService notifier)
        {
            _notifier = notifier;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] UserTenantMessageRequest request, CancellationToken cancellationToken)
        {
            var success = await _notifier.SendProactiveNotification(request.Id, request.TenantId, MessageFactory.Text("This is a proactive notification"), cancellationToken);

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
