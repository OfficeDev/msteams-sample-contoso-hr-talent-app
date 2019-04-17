using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TeamsTalentMgmtAppV3.Models.Commands;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;
using TeamsTalentMgmtAppV3.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Services.Data
{
    public sealed class PositionService : IPositionService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public PositionService(DatabaseContext databaseContext,
            INotificationService notificationService,
            IMapper mapper)
        {
            _databaseContext = databaseContext;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task<ReadOnlyCollection<Position>> GetAllPositions(CancellationToken cancellationToken)
            => (await _databaseContext.Positions.ToListAsync(cancellationToken)).AsReadOnly();

        public async Task<ReadOnlyCollection<Position>> Search(string searchText, int maxResults, CancellationToken cancellationToken)
        {
            Position[] positions;
            if (string.IsNullOrEmpty(searchText))
            {
                positions = await _databaseContext.Positions
                    .OrderBy(x => x.DaysOpen)
                    .Take(maxResults)
                    .ToArrayAsync(cancellationToken);
            }
            else
            {
                searchText = searchText.ToLowerInvariant();
                
                positions = await _databaseContext.Positions
                    .Where(x => x.Title.ToLowerInvariant().Contains(searchText) ||
                               x.PositionExternalId.ToLowerInvariant().Contains(searchText))
                    .OrderBy(x => x.DaysOpen)
                    .Take(maxResults)
                    .ToArrayAsync(cancellationToken);
            }

            return Array.AsReadOnly(positions);
        }

        public Task<Position> GetById(int positionId)
        {
            return _databaseContext.Positions.FindAsync(positionId);
        }

        public async Task<Position> AddNewPosition(PositionCreateCommand positionCreateCommand, CancellationToken cancellationToken)
        {
            var position = _mapper.Map<Position>(positionCreateCommand);

            await _databaseContext.Positions.AddAsync(position, cancellationToken);
            await _databaseContext.SaveChangesAsync(cancellationToken);

            await _notificationService.NotifyRecruiterAboutNewOpenPosition(position, cancellationToken);

            return position;
        }

        public async Task<ReadOnlyCollection<Position>> GetOpenPositions(string recruiterNameOrAlias, CancellationToken cancellationToken)
        {
            var recruiter = await _databaseContext.Recruiters.FirstOrDefaultAsync(x => 
                string.Equals(x.Alias, recruiterNameOrAlias, StringComparison.OrdinalIgnoreCase) || 
                string.Equals(x.Name, recruiterNameOrAlias), cancellationToken);
            
            Position[] positions;
            if (recruiter is null)
            {
                positions = await _databaseContext.Positions.ToArrayAsync(cancellationToken);
            }
            else
            {
                positions = recruiter.Positions.ToArray();
            }

            return Array.AsReadOnly(positions);
        }

        public Task<Position> GetByExternalId(string externalId, CancellationToken cancellationToken)
        {
            return _databaseContext.Positions.FirstOrDefaultAsync(
                x => string.Equals(x.PositionExternalId, externalId, StringComparison.OrdinalIgnoreCase),
                cancellationToken);
        }
    }
}