using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IDetentionService
    {
        Task<IEnumerable<DetentionListViewModel>> GetDetentionsAsync(bool activeOnly = true, string searchTerm = null);
        Task<DetentionViewModel> GetDetentionByIdAsync(short id);
        Task<bool> SaveDetentionAsync(DetentionViewModel model);
        Task<bool> DeleteDetentionAsync(short id);
        Task<bool> DetentionExistsAsync(string detentionName, short? excludeId = null);
    }

    public class DetentionService : IDetentionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DetentionService> _logger;

        public DetentionService(ApplicationDbContext context, ILogger<DetentionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<DetentionListViewModel>> GetDetentionsAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Detentions
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.DetentionName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.DetentionName)
                    .Select(x => new DetentionListViewModel
                    {
                        DetentionId = x.DetentionId,
                        DetentionName = x.DetentionName,
                        HRsThreshold = x.HRsThreshold,
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
                _logger.LogError(ex, "Error retrieving detentions");
                throw;
            }
        }

        public async Task<DetentionViewModel> GetDetentionByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Detentions
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.DetentionId == id);

                if (entity == null) return null;

                return new DetentionViewModel
                {
                    DetentionId = entity.DetentionId,
                    DetentionName = entity.DetentionName,
                    HRsThreshold = entity.HRsThreshold,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving detention with ID {DetentionId}", id);
                throw;
            }
        }

        public async Task<bool> SaveDetentionAsync(DetentionViewModel model)
        {
            try
            {
                Detention entity;

                if (model.DetentionId.HasValue && model.DetentionId > 0)
                {
                    // Update existing
                    entity = await _context.Detentions.FindAsync(model.DetentionId.Value);
                    if (entity == null) return false;

                    entity.DetentionName = model.DetentionName;
                    entity.HRsThreshold = model.HRsThreshold;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new Detention
                    {
                        DetentionName = model.DetentionName,
                        HRsThreshold = model.HRsThreshold,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.Detentions.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving detention");
                throw;
            }
        }

        public async Task<bool> DeleteDetentionAsync(short id)
        {
            try
            {
                var entity = await _context.Detentions.FindAsync(id);
                if (entity == null) return false;

                _context.Detentions.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting detention with ID {DetentionId}", id);
                throw;
            }
        }

        public async Task<bool> DetentionExistsAsync(string detentionName, short? excludeId = null)
        {
            try
            {
                var query = _context.Detentions.Where(x => x.DetentionName == detentionName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.DetentionId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if detention exists");
                throw;
            }
        }
    }
}