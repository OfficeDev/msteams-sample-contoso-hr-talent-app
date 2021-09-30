using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;
using TeamTalentMgmtApp.Shared.Services.Interfaces;

namespace TeamTalentMgmtApp.Shared.Services.Data
{
    public sealed class LocationService : ILocationService
    {
        private readonly DatabaseContext _databaseContext;

        public LocationService(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        public async Task<ReadOnlyCollection<Location>> GetAllLocations(CancellationToken cancellationToken)
        {
            var result = await _databaseContext.Locations.ToArrayAsync(cancellationToken);
            return Array.AsReadOnly(result);
        }
    }
}