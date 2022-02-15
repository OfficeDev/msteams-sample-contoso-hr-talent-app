using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Services.Interfaces
{
    public interface INotificationService
    {
        Task AddSubscriber(string webhookUrl, CancellationToken cancellationToken = default);

        Task RemoveSubscriber(string webhookUrl, CancellationToken cancellationToken = default);

        Task NotifyAboutStageChange(Candidate candidate, CancellationToken cancellationToken = default);

        Task NotifyRecruiterAboutNewOpenPosition(Position position, CancellationToken cancellationToken = default);

        Task NotifyRecruiterAboutCandidateStageChange(Candidate candidate, CancellationToken cancellationToken = default);

        Task SendToConversation(
            string text,
            IList<Attachment> attachments,
            ConversationData conversationData,
            CancellationToken cancellationToken);
    }
}
