using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace TeamsTalentMgmtAppV4.Services.Interfaces
{
    public interface IBotService
    {
        Task HandleMembersAddedAsync(
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken);

        Task<IMessageActivity> OpenPositionAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken);

        Task<IMessageActivity> LeaveInternalCommentAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken);

        Task<IMessageActivity> ScheduleInterviewAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken);

        Task HandleFileAttachments(ITurnContext turnContext, CancellationToken cancellationToken);
    }
}
