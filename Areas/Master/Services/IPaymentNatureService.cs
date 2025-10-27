using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IPaymentNatureService
    {
        Task<IEnumerable<PaymentNatureListViewModel>> GetPaymentNaturesAsync(bool activeOnly = true, string searchTerm = null);
        Task<PaymentNatureViewModel> GetPaymentNatureByIdAsync(short id);
        Task<bool> SavePaymentNatureAsync(PaymentNatureViewModel model);
        Task<bool> DeletePaymentNatureAsync(short id);
        Task<bool> PaymentNatureExistsAsync(string paymentNatureName, short? excludeId = null);
    }

    public class PaymentNatureService : IPaymentNatureService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentNatureService> _logger;

        public PaymentNatureService(ApplicationDbContext context, ILogger<PaymentNatureService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PaymentNatureListViewModel>> GetPaymentNaturesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.PaymentNatures
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.PaymentNatureName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.PaymentNatureName)
                    .Select(x => new PaymentNatureListViewModel
                    {
                        PaymentNatureId = x.PaymentNatureId,
                        PaymentNatureName = x.PaymentNatureName,
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
                _logger.LogError(ex, "Error retrieving paymentNatures");
                throw;
            }
        }

        public async Task<PaymentNatureViewModel> GetPaymentNatureByIdAsync(short id)
        {
            try
            {
                var entity = await _context.PaymentNatures
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.PaymentNatureId == id);

                if (entity == null) return null;

                return new PaymentNatureViewModel
                {
                    PaymentNatureId = entity.PaymentNatureId,
                    PaymentNatureName = entity.PaymentNatureName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paymentNature with ID {PaymentNatureId}", id);
                throw;
            }
        }

        public async Task<bool> SavePaymentNatureAsync(PaymentNatureViewModel model)
        {
            try
            {
                PaymentNature entity;

                if (model.PaymentNatureId.HasValue && model.PaymentNatureId > 0)
                {
                    // Update existing
                    entity = await _context.PaymentNatures.FindAsync(model.PaymentNatureId.Value);
                    if (entity == null) return false;

                    entity.PaymentNatureName = model.PaymentNatureName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new PaymentNature
                    {
                        PaymentNatureName = model.PaymentNatureName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.PaymentNatures.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving paymentNature");
                throw;
            }
        }

        public async Task<bool> DeletePaymentNatureAsync(short id)
        {
            try
            {
                var entity = await _context.PaymentNatures.FindAsync(id);
                if (entity == null) return false;

                _context.PaymentNatures.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting paymentNature with ID {PaymentNatureId}", id);
                throw;
            }
        }

        public async Task<bool> PaymentNatureExistsAsync(string paymentNatureName, short? excludeId = null)
        {
            try
            {
                var query = _context.PaymentNatures.Where(x => x.PaymentNatureName == paymentNatureName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.PaymentNatureId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if paymentNature exists");
                throw;
            }
        }
    }
}