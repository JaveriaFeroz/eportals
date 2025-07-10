using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IPaymentModeService
    {
        Task<IEnumerable<PaymentModeListViewModel>> GetPaymentModesAsync(bool activeOnly = true, string searchTerm = null);
        Task<PaymentModeViewModel> GetPaymentModeByIdAsync(short id);
        Task<bool> SavePaymentModeAsync(PaymentModeViewModel model);
        Task<bool> DeletePaymentModeAsync(short id);
        Task<bool> PaymentModeExistsAsync(string paymentModeName, short? excludeId = null);
    }

    public class PaymentModeService : IPaymentModeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentModeService> _logger;

        public PaymentModeService(ApplicationDbContext context, ILogger<PaymentModeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PaymentModeListViewModel>> GetPaymentModesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.PaymentModes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.PaymentModeName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.PaymentModeName)
                    .Select(x => new PaymentModeListViewModel
                    {
                        PaymentModeId = x.PaymentModeId,
                        PaymentModeName = x.PaymentModeName,
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
                _logger.LogError(ex, "Error retrieving paymentModes");
                throw;
            }
        }

        public async Task<PaymentModeViewModel> GetPaymentModeByIdAsync(short id)
        {
            try
            {
                var entity = await _context.PaymentModes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.PaymentModeId == id);

                if (entity == null) return null;

                return new PaymentModeViewModel
                {
                    PaymentModeId = entity.PaymentModeId,
                    PaymentModeName = entity.PaymentModeName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paymentMode with ID {PaymentModeId}", id);
                throw;
            }
        }

        public async Task<bool> SavePaymentModeAsync(PaymentModeViewModel model)
        {
            try
            {
                PaymentMode entity;

                if (model.PaymentModeId.HasValue && model.PaymentModeId > 0)
                {
                    // Update existing
                    entity = await _context.PaymentModes.FindAsync(model.PaymentModeId.Value);
                    if (entity == null) return false;

                    entity.PaymentModeName = model.PaymentModeName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new PaymentMode
                    {
                        PaymentModeName = model.PaymentModeName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.PaymentModes.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving paymentMode");
                throw;
            }
        }

        public async Task<bool> DeletePaymentModeAsync(short id)
        {
            try
            {
                var entity = await _context.PaymentModes.FindAsync(id);
                if (entity == null) return false;

                _context.PaymentModes.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting paymentMode with ID {PaymentModeId}", id);
                throw;
            }
        }

        public async Task<bool> PaymentModeExistsAsync(string paymentModeName, short? excludeId = null)
        {
            try
            {
                var query = _context.PaymentModes.Where(x => x.PaymentModeName == paymentModeName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.PaymentModeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if paymentMode exists");
                throw;
            }
        }
    }
}