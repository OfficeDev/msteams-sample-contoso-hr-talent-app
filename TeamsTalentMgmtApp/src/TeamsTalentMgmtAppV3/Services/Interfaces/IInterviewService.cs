using System.Threading;
using System.Threading.Tasks;
using TeamsTalentMgmtAppV3.Models.Commands;

namespace TeamsTalentMgmtAppV3.Services.Interfaces
{
    public interface IInterviewService
    {
        Task ScheduleInterview(ScheduleInterviewCommand scheduleInterviewCommand, CancellationToken cancellationToken = default);
    }
}