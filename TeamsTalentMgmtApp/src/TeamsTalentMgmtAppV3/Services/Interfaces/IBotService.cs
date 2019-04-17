using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace TeamsTalentMgmtAppV3.Services.Interfaces
{
    public interface IBotService
    {
        Task<bool> HandleAdaptiveCardAction(Activity activity, CancellationToken cancellationToken = default);
        Task HandleMessage(Activity activity, CancellationToken cancellationToken = default);
        Task HandleFileConsentResponse(Activity activity, CancellationToken cancellationToken = default);
        Task HandleConversationUpdate(Activity activity, CancellationToken cancellationToken = default);
    }
}