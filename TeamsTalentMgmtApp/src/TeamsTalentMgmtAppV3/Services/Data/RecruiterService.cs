using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Teams.Models;
using Microsoft.EntityFrameworkCore;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;
using TeamsTalentMgmtAppV3.Services.Interfaces;
using TeamsChannelData = TeamsTalentMgmtAppV3.Models.DatabaseContext.TeamsChannelData;

namespace TeamsTalentMgmtAppV3.Services.Data
{
    public sealed class RecruiterService : IRecruiterService
    {
        private readonly DatabaseContext _databaseContext;

        public RecruiterService(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Task<Recruiter> GetById(int id) => _databaseContext.Recruiters.FindAsync(id);

        public async Task<ReadOnlyCollection<Recruiter>> GetAllHiringManagers(CancellationToken cancellationToken)
        {
            var result = await _databaseContext.Recruiters
                .Where(x => x.Role == RecruiterRole.HiringManager)
                .ToArrayAsync(cancellationToken);
            
            return Array.AsReadOnly(result);
        }

        public async Task<ReadOnlyCollection<Recruiter>> GetAllInterviewers(CancellationToken cancellationToken)
        {
            var result = await _databaseContext.Recruiters
                .Where(x => x.Role != RecruiterRole.HiringManager)
                .ToArrayAsync(cancellationToken);
            
            return Array.AsReadOnly(result);
        }

        public async Task SaveTeamsChannelData(string serviceUrl, string tenantId, List<TeamsChannelAccount> channelAccounts, CancellationToken cancellationToken)
        {
            var channelAccountsMap = new Dictionary<string, TeamsChannelData>(channelAccounts.Count);
            foreach (var channelAccount in channelAccounts)
            {
                var key = channelAccount.Email
                    .Substring(0, channelAccount.Email.IndexOf('@'))
                    .ToLowerInvariant();

                var channelData = new TeamsChannelData
                {
                    AccountId = channelAccount.Id,
                    ServiceUrl = serviceUrl,
                    TenantId = tenantId
                };
                
                channelAccountsMap.Add(key, channelData);
            }

            var aliases = channelAccountsMap.Keys.ToList();
            
            var recruiters = await _databaseContext.Recruiters
                .Where(x => aliases.Contains(x.Alias.ToLowerInvariant()))
                .ToListAsync(cancellationToken);

            foreach (var recruiter in recruiters)
            {
                recruiter.TeamsChannelData = channelAccountsMap[recruiter.Alias.ToLowerInvariant()];
            }

            await _databaseContext.SaveChangesAsync(cancellationToken);
        }
    }
}