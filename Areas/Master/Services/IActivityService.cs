using Microsoft.EntityFrameworkCore;
using ProcureToPay.Data;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IActivityService
    {
        Task<IEnumerable<ActivityListViewModel>> GetActivitiesAsync(bool activeOnly = true, string searchTerm = null);
        Task<ActivityViewModel> GetActivityByIdAsync(short id);
        Task<bool> SaveActivityAsync(ActivityViewModel model);
        Task<bool> DeleteActivityAsync(short id);
        Task<bool> ActivityExistsAsync(string activityName, short? excludeId = null);
    }

    public class ActivityService : IActivityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ActivityService> _logger;

        public ActivityService(ApplicationDbContext context, ILogger<ActivityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ActivityListViewModel>> GetActivitiesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Activities
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.ActivityName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.ActivityName)
                    .Select(x => new ActivityListViewModel
                    {
                        ActivityId = x.ActivityId,
                        ActivityName = x.ActivityName,
                        EstHrsReq = x.EstHrsReq,
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
                _logger.LogError(ex, "Error retrieving activities");
                throw;
            }
        }

        public async Task<ActivityViewModel> GetActivityByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Activities
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.ActivityId == id);

                if (entity == null) return null;

                return new ActivityViewModel
                {
                    ActivityId = entity.ActivityId,
                    ActivityName = entity.ActivityName,
                    EstHrsReq = entity.EstHrsReq,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity with ID {ActivityId}", id);
                throw;
            }
        }

        public async Task<bool> SaveActivityAsync(ActivityViewModel model)
        {
            try
            {
                Activity entity;

                if (model.ActivityId.HasValue && model.ActivityId > 0)
                {
                    // Update existing
                    entity = await _context.Activities.FindAsync(model.ActivityId.Value);
                    if (entity == null) return false;

                    entity.ActivityName = model.ActivityName;
                    entity.EstHrsReq = model.EstHrsReq;
                    entity.IsActive = model.IsActive;
                    // UpdatedBy/UpdatedOn handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new Activity
                    {
                        ActivityName = model.ActivityName,
                        EstHrsReq = model.EstHrsReq,
                        IsActive = model.IsActive
                        // CreatedBy/CreatedOn handled automatically by DbContext
                    };
                    _context.Activities.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving activity");
                throw;
            }
        }

        public async Task<bool> DeleteActivityAsync(short id)
        {
            try
            {
                var entity = await _context.Activities.FindAsync(id);
                if (entity == null) return false;

                _context.Activities.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity with ID {ActivityId}", id);
                throw;
            }
        }

        public async Task<bool> ActivityExistsAsync(string activityName, short? excludeId = null)
        {
            try
            {
                var query = _context.Activities.Where(x => x.ActivityName == activityName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.ActivityId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if activity exists");
                throw;
            }
        }
    }
}