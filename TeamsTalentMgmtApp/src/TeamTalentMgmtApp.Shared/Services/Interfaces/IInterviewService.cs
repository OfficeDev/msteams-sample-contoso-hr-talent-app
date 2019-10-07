using System.Threading;
using System.Threading.Tasks;
using TeamTalentMgmtApp.Shared.Models.Commands;

namespace TeamTalentMgmtApp.Shared.Services.Interfaces
{
    public interface IInterviewService
    {
        Task ScheduleInterview(ScheduleInterviewCommand scheduleInterviewCommand, CancellationToken cancellationToken = default);
    }
}