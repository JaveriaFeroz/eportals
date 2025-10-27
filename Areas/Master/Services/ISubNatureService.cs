using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface ISubNatureService
    {
        Task<IEnumerable<SubNatureListViewModel>> GetSubNaturesAsync(bool activeOnly = true, string searchTerm = null);
        Task<SubNatureViewModel> GetSubNatureByIdAsync(short id);
        Task<bool> SaveSubNatureAsync(SubNatureViewModel model);
        Task<bool> DeleteSubNatureAsync(short id);
        Task<bool> SubNatureExistsAsync(string subNatureName, short? excludeId = null);
    }

    public class SubNatureService : ISubNatureService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubNatureService> _logger;

        public SubNatureService(ApplicationDbContext context, ILogger<SubNatureService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SubNatureListViewModel>> GetSubNaturesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.SubNatures
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.SubNatureName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.SubNatureName)
                    .Select(x => new SubNatureListViewModel
                    {
                        SubNatureId = x.SubNatureId,
                        SubNatureName = x.SubNatureName,
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
                _logger.LogError(ex, "Error retrieving subNatures");
                throw;
            }
        }

        public async Task<SubNatureViewModel> GetSubNatureByIdAsync(short id)
        {
            try
            {
                var entity = await _context.SubNatures
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.SubNatureId == id);

                if (entity == null) return null;

                return new SubNatureViewModel
                {
                    SubNatureId = entity.SubNatureId,
                    SubNatureName = entity.SubNatureName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subNature with ID {SubNatureId}", id);
                throw;
            }
        }

        public async Task<bool> SaveSubNatureAsync(SubNatureViewModel model)
        {
            try
            {
                SubNature entity;

                if (model.SubNatureId.HasValue && model.SubNatureId > 0)
                {
                    // Update existing
                    entity = await _context.SubNatures.FindAsync(model.SubNatureId.Value);
                    if (entity == null) return false;

                    entity.SubNatureName = model.SubNatureName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new SubNature
                    {
                        SubNatureName = model.SubNatureName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.SubNatures.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving subNature");
                throw;
            }
        }

        public async Task<bool> DeleteSubNatureAsync(short id)
        {
            try
            {
                var entity = await _context.SubNatures.FindAsync(id);
                if (entity == null) return false;

                _context.SubNatures.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subNature with ID {SubNatureId}", id);
                throw;
            }
        }

        public async Task<bool> SubNatureExistsAsync(string subNatureName, short? excludeId = null)
        {
            try
            {
                var query = _context.SubNatures.Where(x => x.SubNatureName == subNatureName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.SubNatureId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if subNature exists");
                throw;
            }
        }
    }
}