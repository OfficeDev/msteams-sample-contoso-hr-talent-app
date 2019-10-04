using System;
using AdaptiveCards;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV3.Services.Interfaces
{
    public interface ITemplateService
    {
        AdaptiveCard GetAdaptiveCardForNewJobPosting(string description = null);
        AdaptiveCard GetAdaptiveCardForInterviewRequest(Candidate candidate, DateTime interviewDate);
    }
}
