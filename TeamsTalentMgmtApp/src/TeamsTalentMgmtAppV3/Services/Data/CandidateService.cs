using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TeamsTalentMgmtAppV3.Extensions;
using TeamsTalentMgmtAppV3.Models.Commands;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;
using TeamsTalentMgmtAppV3.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Services.Data
{
    public sealed class CandidateService : ICandidateService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly INotificationService _notificationService;

        public CandidateService(
            DatabaseContext databaseContext,
            INotificationService notificationService)
        {
            _databaseContext = databaseContext;
            _notificationService = notificationService;
        }
        public async Task AddComment(LeaveCommentCommand leaveCommentCommand, string authorName, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(leaveCommentCommand?.Comment))
            {
                var candidate = await _databaseContext.Candidates.FindAsync(leaveCommentCommand.CandidateId);
                var recruiter = await _databaseContext.Recruiters.FirstOrDefaultAsync(x => string.Equals(x.Name, authorName), cancellationToken);
                
                candidate?.Comments.Add(new Comment
                {
                    CandidateId = leaveCommentCommand.CandidateId,
                    Text = leaveCommentCommand.Comment,
                    AuthorName = authorName,
                    AuthorProfilePicture = recruiter?.ProfilePicture,
                    AuthorRole = recruiter?.Role.ToString()
                });
                await _databaseContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task UpdateCandidateStage(int candidateId, InterviewStageType newStage, CancellationToken cancellationToken = default)
        {
            var candidate = await _databaseContext.Candidates.FindAsync(candidateId);
            if (candidate != null && candidate.Stage != newStage)
            {
                candidate.PreviousStage = candidate.Stage;
                candidate.Stage = newStage;
                await _databaseContext.SaveChangesAsync(cancellationToken);
                await _notificationService.NotifyAboutStageChange(candidate, cancellationToken);
            }
        }

        public async Task<ReadOnlyCollection<Candidate>> Search(string searchText, int maxResults, CancellationToken cancellationToken = default)
        {
            Candidate[] candidates;
            if (searchText.HasValue())
            {
                if (int.TryParse(searchText, out var id))
                {
                    var candidate = await _databaseContext.Candidates.FindAsync(id);
                    candidates = new[] {candidate};
                }
                else
                {
                    candidates = await _databaseContext.Candidates
                        .Where(x => x.Name.ToLowerInvariant().Contains(searchText.ToLowerInvariant()))
                        .Take(maxResults)
                        .ToArrayAsync(cancellationToken);
                }
            }
            else
            {
                candidates = await _databaseContext.Candidates
                    .Take(maxResults)
                    .ToArrayAsync(cancellationToken);
            }

            return Array.AsReadOnly(candidates);
        }

        public Task<Candidate> GetById(int id) => _databaseContext.Candidates.FindAsync(id);
    }
}