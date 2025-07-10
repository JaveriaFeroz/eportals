using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface ICapacityService
    {
        Task<IEnumerable<CapacityListViewModel>> GetCapacitiesAsync(bool activeOnly = true, string searchTerm = null);
        Task<CapacityViewModel> GetCapacityByIdAsync(short id);
        Task<bool> SaveCapacityAsync(CapacityViewModel model);
        Task<bool> DeleteCapacityAsync(short id);
        Task<bool> CapacityExistsAsync(string capacityName, short? excludeId = null);
    }

    public class CapacityService : ICapacityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CapacityService> _logger;

        public CapacityService(ApplicationDbContext context, ILogger<CapacityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<CapacityListViewModel>> GetCapacitiesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Capacities
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.CapacityName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.CapacityName)
                    .Select(x => new CapacityListViewModel
                    {
                        CapacityId = x.CapacityId,
                        CapacityName = x.CapacityName,
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
                _logger.LogError(ex, "Error retrieving capacities");
                throw;
            }
        }

        public async Task<CapacityViewModel> GetCapacityByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Capacities
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.CapacityId == id);

                if (entity == null) return null;

                return new CapacityViewModel
                {
                    CapacityId = entity.CapacityId,
                    CapacityName = entity.CapacityName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving capacity with ID {CapacityId}", id);
                throw;
            }
        }

        public async Task<bool> SaveCapacityAsync(CapacityViewModel model)
        {
            try
            {
                Capacity entity;

                if (model.CapacityId.HasValue && model.CapacityId > 0)
                {
                    // Update existing
                    entity = await _context.Capacities.FindAsync(model.CapacityId.Value);
                    if (entity == null) return false;

                    entity.CapacityName = model.CapacityName;
                    entity.IsActive = model.IsActive;
                }
                else
                {
                    // Create new
                    entity = new Capacity
                    {
                        CapacityName = model.CapacityName,
                        IsActive = model.IsActive
                    };
                    _context.Capacities.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving capacity");
                throw;
            }
        }

        public async Task<bool> DeleteCapacityAsync(short id)
        {
            try
            {
                var entity = await _context.Capacities.FindAsync(id);
                if (entity == null) return false;

                _context.Capacities.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting capacity with ID {CapacityId}", id);
                throw;
            }
        }

        public async Task<bool> CapacityExistsAsync(string capacityName, short? excludeId = null)
        {
            try
            {
                var query = _context.Capacities.Where(x => x.CapacityName == capacityName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.CapacityId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if capacity exists");
                throw;
            }
        }
    }
}
