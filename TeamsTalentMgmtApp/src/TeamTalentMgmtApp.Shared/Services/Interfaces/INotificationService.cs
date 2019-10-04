using System.Threading;
using System.Threading.Tasks;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;

namespace TeamTalentMgmtApp.Shared.Services.Interfaces
{
    public interface INotificationService
    {
        Task AddSubscriber(string webhookUrl, CancellationToken cancellationToken = default);

        Task RemoveSubscriber(string webhookUrl, CancellationToken cancellationToken = default);

        Task NotifyAboutStageChange(Candidate candidate, CancellationToken cancellationToken = default);

        Task NotifyRecruiterAboutNewOpenPosition(Position position, CancellationToken cancellationToken = default);

        Task NotifyRecruiterAboutCandidateStageChange(Candidate candidate, CancellationToken cancellationToken = default);
    }
}
