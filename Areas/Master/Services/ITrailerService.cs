using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface ITrailerService
    {
        Task<IEnumerable<TrailerListViewModel>> GetTrailersAsync(bool activeOnly = true, string searchTerm = null);
        Task<TrailerViewModel> GetTrailerByIdAsync(short id);
        Task<bool> SaveTrailerAsync(TrailerViewModel model);
        Task<bool> DeleteTrailerAsync(short id);
        Task<bool> TrailerExistsAsync(string trailerName, short? excludeId = null);
    }

    public class TrailerService : ITrailerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TrailerService> _logger;

        public TrailerService(ApplicationDbContext context, ILogger<TrailerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TrailerListViewModel>> GetTrailersAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Trailers
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.TrailerName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.TrailerName)
                    .Select(x => new TrailerListViewModel
                    {
                        TrailerId = x.TrailerId,
                        TrailerName = x.TrailerName,
                        IsActive = x.IsActive,
                        CreatedBy = x.CreatedByUser != null ? $"{x.CreatedByUser.FirstName} {x.CreatedByUser.LastName}" : "System",
                        CreatedOn = x.CreatedOn,
                        UpdatedBy = x.UpdatedByUser != null ? $"{x.UpdatedByUser.FirstName} {x.UpdatedByUser.LastName}" : null,
                        UpdatedOn = x.UpdatedOn
                    })
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trailers");
                throw;
            }
        }

        public async Task<TrailerViewModel> GetTrailerByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Trailers
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.TrailerId == id);

                if (entity == null) return null;

                return new TrailerViewModel
                {
                    TrailerId = entity.TrailerId,
                    TrailerName = entity.TrailerName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trailer with ID {TrailerId}", id);
                throw;
            }
        }

        public async Task<bool> SaveTrailerAsync(TrailerViewModel model)
        {
            try
            {
                Trailer entity;

                if (model.TrailerId.HasValue && model.TrailerId > 0)
                {
                    // Update existing
                    entity = await _context.Trailers.FindAsync(model.TrailerId.Value);
                    if (entity == null) return false;

                    entity.TrailerName = model.TrailerName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new Trailer
                    {
                        TrailerName = model.TrailerName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.Trailers.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving trailer");
                throw;
            }
        }

        public async Task<bool> DeleteTrailerAsync(short id)
        {
            try
            {
                var entity = await _context.Trailers.FindAsync(id);
                if (entity == null) return false;

                _context.Trailers.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting trailer with ID {TrailerId}", id);
                throw;
            }
        }

        public async Task<bool> TrailerExistsAsync(string trailerName, short? excludeId = null)
        {
            try
            {
                var query = _context.Trailers.Where(x => x.TrailerName == trailerName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.TrailerId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if trailer exists");
                throw;
            }
        }
    }
}