using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Teams.Models;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV3.Services.Interfaces
{
    public interface IRecruiterService
    {
        Task<Recruiter> GetById(int id);
        Task<ReadOnlyCollection<Recruiter>> GetAllHiringManagers(CancellationToken cancellationToken = default);
        Task<ReadOnlyCollection<Recruiter>> GetAllInterviewers(CancellationToken cancellationToken = default);
        Task SaveTeamsChannelData(string serviceUrl, string tenantId, List<TeamsChannelAccount> channelAccounts, CancellationToken cancellationToken = default);
    }
}