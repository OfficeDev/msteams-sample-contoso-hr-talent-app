using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Services.Interfaces
{
    public interface INotificationService
    {
        Task<bool> SendProactiveNotification(string aliasUpnOrOid, string tenantId, IActivity activityToSend, CancellationToken cancellationToken = default);
        Task NotifyRecruiterAboutCandidateStageChange(string tenantId, Candidate candidate, CancellationToken cancellationToken);
        Task NotifyRecruiterAboutNewOpenPosition(string tenantId, Position position, CancellationToken cancellationToken);
    }
}
