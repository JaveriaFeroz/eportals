using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IPriorityService
    {
        Task<IEnumerable<PriorityListViewModel>> GetPrioritiesAsync(bool activeOnly = true, string searchTerm = null);
        Task<PriorityViewModel> GetPriorityByIdAsync(short id);
        Task<bool> SavePriorityAsync(PriorityViewModel model);
        Task<bool> DeletePriorityAsync(short id);
        Task<bool> PriorityExistsAsync(string priorityName, short? excludeId = null);
    }

    public class PriorityService : IPriorityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PriorityService> _logger;

        public PriorityService(ApplicationDbContext context, ILogger<PriorityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PriorityListViewModel>> GetPrioritiesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Priorities
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.PriorityName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.PriorityName)
                    .Select(x => new PriorityListViewModel
                    {
                        PriorityId = x.PriorityId,
                        PriorityName = x.PriorityName,
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
                _logger.LogError(ex, "Error retrieving priorities");
                throw;
            }
        }

        public async Task<PriorityViewModel> GetPriorityByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Priorities
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.PriorityId == id);

                if (entity == null) return null;

                return new PriorityViewModel
                {
                    PriorityId = entity.PriorityId,
                    PriorityName = entity.PriorityName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving priority with ID {PriorityId}", id);
                throw;
            }
        }

        public async Task<bool> SavePriorityAsync(PriorityViewModel model)
        {
            try
            {
                Priority entity;

                if (model.PriorityId.HasValue && model.PriorityId > 0)
                {
                    entity = await _context.Priorities.FindAsync(model.PriorityId.Value);
                    if (entity == null) return false;

                    entity.PriorityName = model.PriorityName;
                    entity.IsActive = model.IsActive;
                }
                else
                {
                    entity = new Priority
                    {
                        PriorityName = model.PriorityName,
                        IsActive = model.IsActive
                    };
                    _context.Priorities.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving priority");
                throw;
            }
        }

        public async Task<bool> DeletePriorityAsync(short id)
        {
            try
            {
                var entity = await _context.Priorities.FindAsync(id);
                if (entity == null) return false;

                _context.Priorities.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting priority with ID {PriorityId}", id);
                throw;
            }
        }

        public async Task<bool> PriorityExistsAsync(string priorityName, short? excludeId = null)
        {
            try
            {
                var query = _context.Priorities.Where(x => x.PriorityName == priorityName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.PriorityId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if priority exists");
                throw;
            }
        }
    }

}
