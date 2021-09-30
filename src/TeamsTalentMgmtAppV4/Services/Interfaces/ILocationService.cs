using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;

namespace TeamTalentMgmtApp.Shared.Services.Interfaces
{
    public interface ILocationService
    {
        Task<ReadOnlyCollection<Location>> GetAllLocations(CancellationToken cancellationToken = default);
    }
}