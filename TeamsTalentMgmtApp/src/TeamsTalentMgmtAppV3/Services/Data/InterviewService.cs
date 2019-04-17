using System.Threading;
using System.Threading.Tasks;
using TeamsTalentMgmtAppV3.Models.Commands;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;
using TeamsTalentMgmtAppV3.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Services.Data
{
    public sealed class InterviewService : IInterviewService
    {
        private readonly DatabaseContext _databaseContext;

        public InterviewService(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        
        public async Task ScheduleInterview(ScheduleInterviewCommand scheduleInterviewCommand, CancellationToken cancellationToken = default)
        {
            var candidate = await _databaseContext.Candidates.FindAsync(scheduleInterviewCommand.CandidateId);
            if (candidate != null)
            {
                candidate.Stage = InterviewStageType.Interviewing;
                
                await _databaseContext.Interviews.AddAsync(new Interview
                {
                    CandidateId = candidate.CandidateId,
                    InterviewDate = scheduleInterviewCommand.InterviewDate,
                    RecruiterId = scheduleInterviewCommand.InterviewerId,
                    FeedbackText = "N/A"
                }, cancellationToken);
                
                await _databaseContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}