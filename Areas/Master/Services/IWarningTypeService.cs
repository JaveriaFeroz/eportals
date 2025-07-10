using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IWarningTypeService
    {
        Task<IEnumerable<WarningTypeListViewModel>> GetWarningTypesAsync(bool activeOnly = true, string searchTerm = null);
        Task<WarningTypeViewModel> GetWarningTypeByIdAsync(short id);
        Task<bool> SaveWarningTypeAsync(WarningTypeViewModel model);
        Task<bool> DeleteWarningTypeAsync(short id);
        Task<bool> WarningTypeExistsAsync(string warningTypeName, short? excludeId = null);
    }

    public class WarningTypeService : IWarningTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WarningTypeService> _logger;

        public WarningTypeService(ApplicationDbContext context, ILogger<WarningTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<WarningTypeListViewModel>> GetWarningTypesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.WarningTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.TypeName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.TypeName)
                    .Select(x => new WarningTypeListViewModel
                    {
                        TypeId = x.TypeId,
                        TypeName = x.TypeName,
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
                _logger.LogError(ex, "Error retrieving warningTypes");
                throw;
            }
        }

        public async Task<WarningTypeViewModel> GetWarningTypeByIdAsync(short id)
        {
            try
            {
                var entity = await _context.WarningTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.TypeId == id);

                if (entity == null) return null;

                return new WarningTypeViewModel
                {
                    TypeId = entity.TypeId,
                    TypeName = entity.TypeName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warningType with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> SaveWarningTypeAsync(WarningTypeViewModel model)
        {
            try
            {
                WarningType entity;

                if (model.TypeId.HasValue && model.TypeId > 0)
                {
                    // Update existing
                    entity = await _context.WarningTypes.FindAsync(model.TypeId.Value);
                    if (entity == null) return false;

                    entity.TypeName = model.TypeName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new WarningType
                    {
                        TypeName = model.TypeName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.WarningTypes.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving warningType");
                throw;
            }
        }

        public async Task<bool> DeleteWarningTypeAsync(short id)
        {
            try
            {
                var entity = await _context.WarningTypes.FindAsync(id);
                if (entity == null) return false;

                _context.WarningTypes.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting warningType with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> WarningTypeExistsAsync(string warningTypeName, short? excludeId = null)
        {
            try
            {
                var query = _context.WarningTypes.Where(x => x.TypeName == warningTypeName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.TypeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if warningType exists");
                throw;
            }
        }
    }
}