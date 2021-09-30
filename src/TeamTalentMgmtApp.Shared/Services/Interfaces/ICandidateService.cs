using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using TeamTalentMgmtApp.Shared.Models.Commands;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;

namespace TeamTalentMgmtApp.Shared.Services.Interfaces
{
    public interface ICandidateService
    {
        Task AddComment(LeaveCommentCommand leaveCommentCommand, string authorName, CancellationToken cancellationToken = default);
        Task UpdateCandidateStage(int candidateId, InterviewStageType newStage, CancellationToken cancellationToken = default);
        Task<ReadOnlyCollection<Candidate>> Search(string searchText, int maxResults, CancellationToken cancellationToken = default);
        Task<Candidate> GetById(int id, CancellationToken cancellationToken);
    }
}
