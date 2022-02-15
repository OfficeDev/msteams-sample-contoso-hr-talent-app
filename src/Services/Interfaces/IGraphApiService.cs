using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Services.Interfaces
{
    public interface IGraphApiService
    {
        Task<string> GetProactiveChatIdForAlias(string tenantId, string alias, CancellationToken cancellationToken);

        Task<string> GetProactiveChatIdForUserPrincipalName(string tenantId, string upn, CancellationToken cancellationToken);

        Task<(Team Team, string DisplayName)> CreateNewTeamForPosition(Position position, string token, CancellationToken cancellationToken);

        Task<bool> InstallBotForUser(string tenantId, string upn, CancellationToken cancellationToken);

        Task<string> GetUpnFromAlias(string alias, string tenantId, CancellationToken cancellationToken);
    }
}
