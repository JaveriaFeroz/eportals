using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IProductTypeService
    {
        Task<IEnumerable<ProductTypeListViewModel>> GetProductTypesAsync(bool activeOnly = true, string searchTerm = null);
        Task<ProductTypeViewModel> GetProductTypeByIdAsync(short id);
        Task<bool> SaveProductTypeAsync(ProductTypeViewModel model);
        Task<bool> DeleteProductTypeAsync(short id);
        Task<bool> ProductTypeExistsAsync(string typeName, short? excludeId = null);

    }

    public class ProductTypeService : IProductTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductTypeService> _logger;

        public ProductTypeService(ApplicationDbContext context, ILogger<ProductTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductTypeListViewModel>> GetProductTypesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.ProductTypes
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
                    .Select(x => new ProductTypeListViewModel
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
                _logger.LogError(ex, "Error retrieving product types");
                throw;
            }
        }

        public async Task<ProductTypeViewModel> GetProductTypeByIdAsync(short id)
        {
            try
            {
                var entity = await _context.ProductTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.TypeId == id);

                if (entity == null) return null;

                return new ProductTypeViewModel
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
                _logger.LogError(ex, "Error retrieving product type with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> SaveProductTypeAsync(ProductTypeViewModel model)
        {
            try
            {
                ProductType entity;

                if (model.TypeId.HasValue && model.TypeId > 0)
                {
                    // Update existing
                    entity = await _context.ProductTypes.FindAsync(model.TypeId.Value);
                    if (entity == null) return false;

                    entity.TypeName = model.TypeName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new ProductType
                    {
                        TypeName = model.TypeName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.ProductTypes.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving product type");
                throw;
            }
        }

        public async Task<bool> DeleteProductTypeAsync(short id)
        {
            try
            {
                var entity = await _context.ProductTypes.FindAsync(id);
                if (entity == null) return false;

                _context.ProductTypes.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product type with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> ProductTypeExistsAsync(string typeName, short? excludeId = null)
        {
            try
            {
                var query = _context.ProductTypes.Where(x => x.TypeName == typeName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.TypeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product type exists");
                throw;
            }
        }
      
    }
}