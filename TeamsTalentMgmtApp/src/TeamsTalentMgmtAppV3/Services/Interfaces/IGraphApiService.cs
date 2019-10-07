using System.Threading.Tasks;
using TeamsTalentMgmtAppV3.Models.MicrosoftGraph;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV3.Services.Interfaces
{
    public interface IGraphApiService
    {
        Task<Team> CreateNewTeamForPosition(Position position, string token);
    }
}