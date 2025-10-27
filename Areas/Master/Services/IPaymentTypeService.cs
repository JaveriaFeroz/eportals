using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IPaymentTypeService
    {
        Task<IEnumerable<PaymentTypeListViewModel>> GetPaymentTypesAsync(bool activeOnly = true, string searchTerm = null);
        Task<PaymentTypeViewModel> GetPaymentTypeByIdAsync(short id);
        Task<bool> SavePaymentTypeAsync(PaymentTypeViewModel model);
        Task<bool> DeletePaymentTypeAsync(short id);
        Task<bool> PaymentTypeExistsAsync(string paymentTypeName, short? excludeId = null);
    }

    public class PaymentTypeService : IPaymentTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentTypeService> _logger;

        public PaymentTypeService(ApplicationDbContext context, ILogger<PaymentTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PaymentTypeListViewModel>> GetPaymentTypesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.PaymentTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.PaymentTypeName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.PaymentTypeName)
                    .Select(x => new PaymentTypeListViewModel
                    {
                        PaymentTypeId = x.PaymentTypeId,
                        PaymentTypeName = x.PaymentTypeName,
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
                _logger.LogError(ex, "Error retrieving paymentTypes");
                throw;
            }
        }

        public async Task<PaymentTypeViewModel> GetPaymentTypeByIdAsync(short id)
        {
            try
            {
                var entity = await _context.PaymentTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.PaymentTypeId == id);

                if (entity == null) return null;

                return new PaymentTypeViewModel
                {
                    PaymentTypeId = entity.PaymentTypeId,
                    PaymentTypeName = entity.PaymentTypeName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paymentType with ID {PaymentTypeId}", id);
                throw;
            }
        }

        public async Task<bool> SavePaymentTypeAsync(PaymentTypeViewModel model)
        {
            try
            {
                PaymentType entity;

                if (model.PaymentTypeId.HasValue && model.PaymentTypeId > 0)
                {
                    // Update existing
                    entity = await _context.PaymentTypes.FindAsync(model.PaymentTypeId.Value);
                    if (entity == null) return false;

                    entity.PaymentTypeName = model.PaymentTypeName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new PaymentType
                    {
                        PaymentTypeName = model.PaymentTypeName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.PaymentTypes.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving paymentType");
                throw;
            }
        }

        public async Task<bool> DeletePaymentTypeAsync(short id)
        {
            try
            {
                var entity = await _context.PaymentTypes.FindAsync(id);
                if (entity == null) return false;

                _context.PaymentTypes.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting paymentType with ID {PaymentTypeId}", id);
                throw;
            }
        }

        public async Task<bool> PaymentTypeExistsAsync(string paymentTypeName, short? excludeId = null)
        {
            try
            {
                var query = _context.PaymentTypes.Where(x => x.PaymentTypeName == paymentTypeName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.PaymentTypeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if paymentType exists");
                throw;
            }
        }
    }
}