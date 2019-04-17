using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using TeamsTalentMgmtAppV3.Models.Commands;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV3.Services.Interfaces
{
    public interface IPositionService
    {
        Task<ReadOnlyCollection<Position>> GetAllPositions(CancellationToken cancellationToken);
        Task<ReadOnlyCollection<Position>> Search(string searchText, int maxResults, CancellationToken cancellationToken = default);
        Task<Position> GetById(int positionId);
        Task<Position> AddNewPosition(PositionCreateCommand positionCreateCommand, CancellationToken cancellationToken = default);
        Task<ReadOnlyCollection<Position>> GetOpenPositions(string recruiterNameOrAlias, CancellationToken cancellationToken = default);
        Task<Position> GetByExternalId(string externalId, CancellationToken cancellationToken = default);
    }
}