using System;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;

namespace TeamTalentMgmtApp.Shared.Models.Dto
{
    public sealed class InterviewDto
    {
        public int InterviewId { get; set; }

        public DateTime InterviewDate { get; set; }

        public string FeedbackText { get; set; }

        public int CandidateId { get; set; }

        public int RecruiterId { get; set; }

        public Recruiter Recruiter { get; set; }
    }
}
