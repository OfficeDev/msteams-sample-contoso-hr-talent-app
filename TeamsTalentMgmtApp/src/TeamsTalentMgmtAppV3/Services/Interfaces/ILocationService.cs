using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV3.Services.Interfaces
{
    public interface ILocationService
    {
        Task<ReadOnlyCollection<Location>> GetAllLocations(CancellationToken cancellationToken = default);
    }
}