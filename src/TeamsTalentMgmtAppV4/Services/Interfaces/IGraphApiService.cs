using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV4.Services.Interfaces
{
    public interface IGraphApiService
    {
        Task<string> GetProactiveChatIdForUser(string token, string tenantId, string alias, string message, CancellationToken cancellationToken);

        Task<(Team Team, string DisplayName)> CreateNewTeamForPosition(Position position, string token, CancellationToken cancellationToken);

        Task<bool> InstallBotForUser(string token, string alias, CancellationToken cancellationToken);
    }
}
