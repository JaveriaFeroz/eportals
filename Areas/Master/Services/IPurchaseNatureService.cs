using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IPurchaseNatureService
    {
        Task<IEnumerable<PurchaseNatureListViewModel>> GetPurchaseNaturesAsync(bool activeOnly = true, string searchTerm = null);
        Task<PurchaseNatureViewModel> GetPurchaseNatureByIdAsync(short id);
        Task<bool> SavePurchaseNatureAsync(PurchaseNatureViewModel model);
        Task<bool> DeletePurchaseNatureAsync(short id);
        Task<bool> PurchaseNatureExistsAsync(string purchaseNatureName, short? excludeId = null);
    }

    public class PurchaseNatureService : IPurchaseNatureService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PurchaseNatureService> _logger;

        public PurchaseNatureService(ApplicationDbContext context, ILogger<PurchaseNatureService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PurchaseNatureListViewModel>> GetPurchaseNaturesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.PurchaseNatures
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.PurchaseNatureName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.PurchaseNatureName)
                    .Select(x => new PurchaseNatureListViewModel
                    {
                        PurchaseNatureId = x.PurchaseNatureId,
                        PurchaseNatureName = x.PurchaseNatureName,
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
                _logger.LogError(ex, "Error retrieving purchaseNatures");
                throw;
            }
        }

        public async Task<PurchaseNatureViewModel> GetPurchaseNatureByIdAsync(short id)
        {
            try
            {
                var entity = await _context.PurchaseNatures
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.PurchaseNatureId == id);

                if (entity == null) return null;

                return new PurchaseNatureViewModel
                {
                    PurchaseNatureId = entity.PurchaseNatureId,
                    PurchaseNatureName = entity.PurchaseNatureName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchaseNature with ID {PurchaseNatureId}", id);
                throw;
            }
        }

        public async Task<bool> SavePurchaseNatureAsync(PurchaseNatureViewModel model)
        {
            try
            {
                PurchaseNature entity;

                if (model.PurchaseNatureId.HasValue && model.PurchaseNatureId > 0)
                {
                    // Update existing
                    entity = await _context.PurchaseNatures.FindAsync(model.PurchaseNatureId.Value);
                    if (entity == null) return false;

                    entity.PurchaseNatureName = model.PurchaseNatureName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new PurchaseNature
                    {
                        PurchaseNatureName = model.PurchaseNatureName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.PurchaseNatures.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving purchaseNature");
                throw;
            }
        }

        public async Task<bool> DeletePurchaseNatureAsync(short id)
        {
            try
            {
                var entity = await _context.PurchaseNatures.FindAsync(id);
                if (entity == null) return false;

                _context.PurchaseNatures.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchaseNature with ID {PurchaseNatureId}", id);
                throw;
            }
        }

        public async Task<bool> PurchaseNatureExistsAsync(string purchaseNatureName, short? excludeId = null)
        {
            try
            {
                var query = _context.PurchaseNatures.Where(x => x.PurchaseNatureName == purchaseNatureName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.PurchaseNatureId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if purchaseNature exists");
                throw;
            }
        }
    }
}