using Microsoft.EntityFrameworkCore;
using ProcureToPay.Data;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;

namespace ProcureToPay.Areas.Master.Services
{
    public interface ISupplierTypeService
    {
        Task<IEnumerable<SupplierTypeListViewModel>> GetSupplierTypesAsync(bool activeOnly = true, string searchTerm = null);
        Task<SupplierTypeViewModel> GetSupplierTypeByIdAsync(short id);
        Task<bool> SaveSupplierTypeAsync(SupplierTypeViewModel model);
        Task<bool> DeleteSupplierTypeAsync(short id);
        Task<bool> SupplierTypeExistsAsync(string typeName, short? excludeId = null);
    }

    public class SupplierTypeService : ISupplierTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SupplierTypeService> _logger;

        public SupplierTypeService(ApplicationDbContext context, ILogger<SupplierTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SupplierTypeListViewModel>> GetSupplierTypesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.SupplierTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.SupplierTypeName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.SupplierTypeName)
                    .Select(x => new SupplierTypeListViewModel
                    {
                        TypeId = x.TypeId,
                        SupplierTypeName = x.SupplierTypeName,
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
                _logger.LogError(ex, "Error retrieving supplier types");
                throw;
            }
        }

        public async Task<SupplierTypeViewModel> GetSupplierTypeByIdAsync(short id)
        {
            try
            {
                var entity = await _context.SupplierTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.TypeId == id);

                if (entity == null) return null;

                return new SupplierTypeViewModel
                {
                    TypeId = entity.TypeId,
                    SupplierTypeName = entity.SupplierTypeName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier type with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> SaveSupplierTypeAsync(SupplierTypeViewModel model)
        {
            try
            {
                SupplierType entity;

                if (model.TypeId.HasValue && model.TypeId > 0)
                {
                    // Update existing
                    entity = await _context.SupplierTypes.FindAsync(model.TypeId.Value);
                    if (entity == null) return false;

                    entity.SupplierTypeName = model.SupplierTypeName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new SupplierType
                    {
                        SupplierTypeName = model.SupplierTypeName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.SupplierTypes.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving supplier type");
                throw;
            }
        }

        public async Task<bool> DeleteSupplierTypeAsync(short id)
        {
            try
            {
                var entity = await _context.SupplierTypes.FindAsync(id);
                if (entity == null) return false;

                _context.SupplierTypes.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier type with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> SupplierTypeExistsAsync(string typeName, short? excludeId = null)
        {
            try
            {
                var query = _context.SupplierTypes.Where(x => x.SupplierTypeName == typeName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.TypeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if supplier type exists");
                throw;
            }
        }
    }
}