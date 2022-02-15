using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Services.Interfaces
{
    public interface IGraphApiService
    {
        Task<string> GetProactiveChatIdForUser(string tenantId, string alias, CancellationToken cancellationToken);

        Task<(Team Team, string DisplayName)> CreateNewTeamForPosition(Position position, string token, CancellationToken cancellationToken);

        Task<bool> InstallBotForUser(string tenantId, string alias, CancellationToken cancellationToken);

        Task<string> GetDomainForUser(string token, CancellationToken cancellationToken);
    }
}
