using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface ISKUTypeService
    {
        Task<IEnumerable<SKUTypeListViewModel>> GetSKUTypesAsync(bool activeOnly = true, string searchTerm = null);
        Task<SKUTypeViewModel> GetSKUTypeByIdAsync(short id);
        Task<bool> SaveSKUTypeAsync(SKUTypeViewModel model);
        Task<bool> DeleteSKUTypeAsync(short id);
        Task<bool> SKUTypeExistsAsync(string skuTypeName, short? excludeId = null);
    }

    public class SKUTypeService : ISKUTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SKUTypeService> _logger;

        public SKUTypeService(ApplicationDbContext context, ILogger<SKUTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SKUTypeListViewModel>> GetSKUTypesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.SKUTypes
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
                    .Select(x => new SKUTypeListViewModel
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
                _logger.LogError(ex, "Error retrieving skuTypes");
                throw;
            }
        }

        public async Task<SKUTypeViewModel> GetSKUTypeByIdAsync(short id)
        {
            try
            {
                var entity = await _context.SKUTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.TypeId == id);

                if (entity == null) return null;

                return new SKUTypeViewModel
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
                _logger.LogError(ex, "Error retrieving skuType with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> SaveSKUTypeAsync(SKUTypeViewModel model)
        {
            try
            {
                SKUType entity;

                if (model.TypeId.HasValue && model.TypeId > 0)
                {
                    // Update existing
                    entity = await _context.SKUTypes.FindAsync(model.TypeId.Value);
                    if (entity == null) return false;

                    entity.TypeName = model.TypeName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new SKUType
                    {
                        TypeName = model.TypeName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.SKUTypes.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving skuType");
                throw;
            }
        }

        public async Task<bool> DeleteSKUTypeAsync(short id)
        {
            try
            {
                var entity = await _context.SKUTypes.FindAsync(id);
                if (entity == null) return false;

                _context.SKUTypes.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting skuType with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> SKUTypeExistsAsync(string skuTypeName, short? excludeId = null)
        {
            try
            {
                var query = _context.SKUTypes.Where(x => x.TypeName == skuTypeName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.TypeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if skuType exists");
                throw;
            }
        }
    }
}