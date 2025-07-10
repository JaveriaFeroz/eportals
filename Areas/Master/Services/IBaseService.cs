using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IBaseService
    {
        Task<IEnumerable<BaseListViewModel>> GetBasesAsync(bool activeOnly = true, string searchTerm = null);
        Task<BaseViewModel> GetBaseByIdAsync(short id);
        Task<bool> SaveBaseAsync(BaseViewModel model);
        Task<bool> DeleteBaseAsync(short id);
        Task<bool> BaseExistsAsync(string baseName, short? excludeId = null);
    }

    public class BaseService : IBaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BaseService> _logger;

        public BaseService(ApplicationDbContext context, ILogger<BaseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<BaseListViewModel>> GetBasesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Bases
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.BaseName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.BaseName)
                    .Select(x => new BaseListViewModel
                    {
                        BaseId = x.BaseId,
                        BaseName = x.BaseName,
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
                _logger.LogError(ex, "Error retrieving bases");
                throw;
            }
        }

        public async Task<BaseViewModel> GetBaseByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Bases
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.BaseId == id);

                if (entity == null) return null;

                return new BaseViewModel
                {
                    BaseId = entity.BaseId,
                    BaseName = entity.BaseName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving base with ID {BaseId}", id);
                throw;
            }
        }

        public async Task<bool> SaveBaseAsync(BaseViewModel model)
        {
            try
            {
                Base entity;

                if (model.BaseId.HasValue && model.BaseId > 0)
                {
                    // Update existing
                    entity = await _context.Bases.FindAsync(model.BaseId.Value);
                    if (entity == null) return false;

                    entity.BaseName = model.BaseName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new Base
                    {
                        BaseName = model.BaseName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.Bases.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving base");
                throw;
            }
        }

        public async Task<bool> DeleteBaseAsync(short id)
        {
            try
            {
                var entity = await _context.Bases.FindAsync(id);
                if (entity == null) return false;

                _context.Bases.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting base with ID {BaseId}", id);
                throw;
            }
        }

        public async Task<bool> BaseExistsAsync(string baseName, short? excludeId = null)
        {
            try
            {
                var query = _context.Bases.Where(x => x.BaseName == baseName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.BaseId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if base exists");
                throw;
            }
        }
    }
}