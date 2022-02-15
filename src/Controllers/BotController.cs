using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace TeamsTalentMgmtApp.Controllers
{
    [ApiController]
    [Route("api/messages")]
    public class BotController : ControllerBase
    {
        private readonly IBot _bot;
        private readonly IBotFrameworkHttpAdapter _adapter;

        public BotController(IBot bot, IBotFrameworkHttpAdapter adapter)
        {
            _bot = bot;
            _adapter = adapter;
        }

        [HttpPost]
        public Task PostAsync(CancellationToken cancellationToken)
            => _adapter.ProcessAsync(Request, Response, _bot, cancellationToken);
    }
}
