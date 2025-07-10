using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IProductNatureService
    {
        Task<IEnumerable<ProductNatureListViewModel>> GetProductNaturesAsync(bool activeOnly = true, string searchTerm = null);
        Task<ProductNatureViewModel> GetProductNatureByIdAsync(short id);
        Task<bool> SaveProductNatureAsync(ProductNatureViewModel model);
        Task<bool> DeleteProductNatureAsync(short id);
        Task<bool> ProductNatureExistsAsync(string natureName, short? excludeId = null);

        Task<ProductNatureLookupsViewModel> GetLookupsAsync();

    }

    public class ProductNatureService : IProductNatureService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductNatureService> _logger;

        public ProductNatureService(ApplicationDbContext context, ILogger<ProductNatureService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductNatureListViewModel>> GetProductNaturesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.ProductNatures
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.NatureName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.NatureName)
                    .Select(x => new ProductNatureListViewModel
                    {
                        NatureId = x.NatureId,
                        NatureName = x.NatureName,
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
                _logger.LogError(ex, "Error retrieving product natures");
                throw;
            }
        }

        public async Task<ProductNatureViewModel> GetProductNatureByIdAsync(short id)
        {
            try
            {
                var entity = await _context.ProductNatures
                    .Include(x => x.PurchaseNature)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.NatureId == id);

                if (entity == null) return null;

                return new ProductNatureViewModel
                {
                    NatureId = entity.NatureId,
                    NatureName = entity.NatureName,
                    PurchaseNatureId = entity.PurchaseNatureId,
                    PurchaseNatureName = entity.PurchaseNature?.PurchaseNatureName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product nature with ID {NatureId}", id);
                throw;
            }
        }

        public async Task<bool> SaveProductNatureAsync(ProductNatureViewModel model)
        {
            try
            {
                ProductNature entity;

                if (model.NatureId.HasValue && model.NatureId > 0)
                {
                    // Update existing
                    entity = await _context.ProductNatures.FindAsync(model.NatureId.Value);
                    if (entity == null) return false;

                    entity.NatureName = model.NatureName;
                    entity.PurchaseNatureId = model.PurchaseNatureId;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new ProductNature
                    {
                        NatureName = model.NatureName,
                        PurchaseNatureId = model.PurchaseNatureId,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.ProductNatures.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving product nature");
                throw;
            }
        }

        public async Task<bool> DeleteProductNatureAsync(short id)
        {
            try
            {
                var entity = await _context.ProductNatures.FindAsync(id);
                if (entity == null) return false;

                _context.ProductNatures.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product nature with ID {NatureId}", id);
                throw;
            }
        }

        public async Task<bool> ProductNatureExistsAsync(string natureName, short? excludeId = null)
        {
            try
            {
                var query = _context.ProductNatures.Where(x => x.NatureName == natureName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.NatureId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product nature exists");
                throw;
            }
        }

        public async Task<ProductNatureLookupsViewModel> GetLookupsAsync()
        {
            try
            {
                var lookups = new ProductNatureLookupsViewModel();


                lookups.PurchaseNatures = await _context.PurchaseNatures
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.PurchaseNatureName)
                   .Select(x => new PurchaseNatureLookupViewModel
                   {
                       PurchaseNatureId = x.PurchaseNatureId,
                       PurchaseNatureName = x.PurchaseNatureName
                   })
                   .ToListAsync();

                return lookups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lookups");
                throw;
            }
        }
    }
}