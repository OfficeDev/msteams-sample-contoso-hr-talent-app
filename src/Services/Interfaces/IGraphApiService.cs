﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Services.Interfaces
{
    public interface IGraphApiService
    {
        Task<string> GetProactiveChatIdForUser(string aliasUpnOrOid, string tenantId, CancellationToken cancellationToken);

        Task<(Team Team, string DisplayName)> CreateNewTeamForPosition(Position position, string token, CancellationToken cancellationToken);

        Task<InstallResult> InstallBotForUser(string aliasUpnOrOid, string tenantId, CancellationToken cancellationToken);
    }
}
