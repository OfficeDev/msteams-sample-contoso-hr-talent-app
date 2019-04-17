using System;
using AdaptiveCards;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV3.Services.Interfaces
{
    public interface ITemplateService
    {
        AdaptiveCard GetAdaptiveCardForNewJobPosting(string description = null);
        AdaptiveCard GetAdaptiveCardForInterviewRequest(Candidate candidate, DateTime interviewDate);
    }
}
