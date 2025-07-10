using Microsoft.EntityFrameworkCore;
using ProcureToPay.Data;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IAccessorialChargeService
    {
        Task<IEnumerable<AccessorialChargeListViewModel>> GetAccessorialChargesAsync(bool activeOnly = true, string searchTerm = null);
        Task<AccessorialChargeViewModel> GetAccessorialChargeByIdAsync(short id);
        Task<bool> SaveAccessorialChargeAsync(AccessorialChargeViewModel model);
        Task<bool> DeleteAccessorialChargeAsync(short id);
        Task<bool> AccessorialChargeExistsAsync(string chargeCode, short? excludeId = null);
    }

    public class AccessorialChargeService : IAccessorialChargeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccessorialChargeService> _logger;

        public AccessorialChargeService(ApplicationDbContext context, ILogger<AccessorialChargeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AccessorialChargeListViewModel>> GetAccessorialChargesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.AccessorialCharges
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.ChargeName.Contains(searchTerm) || x.ChargeCode.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.ChargeName)
                    .Select(x => new AccessorialChargeListViewModel
                    {
                        ChargeId = x.ChargeId,
                        ChargeName = x.ChargeName,
                        ChargeCode = x.ChargeCode,
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
                _logger.LogError(ex, "Error retrieving accessorial charges");
                throw;
            }
        }

        public async Task<AccessorialChargeViewModel> GetAccessorialChargeByIdAsync(short id)
        {
            try
            {
                var entity = await _context.AccessorialCharges
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.ChargeId == id);

                if (entity == null) return null;

                return new AccessorialChargeViewModel
                {
                    ChargeId = entity.ChargeId,
                    ChargeName = entity.ChargeName,
                    ChargeCode = entity.ChargeCode,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accessorial charge with ID {ChargeId}", id);
                throw;
            }
        }

        public async Task<bool> SaveAccessorialChargeAsync(AccessorialChargeViewModel model)
        {
            try
            {
                AccessorialCharge entity;

                if (model.ChargeId.HasValue && model.ChargeId > 0)
                {
                    // Update existing
                    entity = await _context.AccessorialCharges.FindAsync(model.ChargeId.Value);
                    if (entity == null) return false;

                    entity.ChargeName = model.ChargeName;
                    entity.ChargeCode = model.ChargeCode;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new AccessorialCharge
                    {
                        ChargeName = model.ChargeName,
                        ChargeCode = model.ChargeCode,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.AccessorialCharges.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving accessorial charge");
                throw;
            }
        }

        public async Task<bool> DeleteAccessorialChargeAsync(short id)
        {
            try
            {
                var entity = await _context.AccessorialCharges.FindAsync(id);
                if (entity == null) return false;

                _context.AccessorialCharges.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting accessorial charge with ID {ChargeId}", id);
                throw;
            }
        }

        public async Task<bool> AccessorialChargeExistsAsync(string chargeCode, short? excludeId = null)
        {
            try
            {
                var query = _context.AccessorialCharges.Where(x => x.ChargeCode == chargeCode);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.ChargeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if accessorial charge exists");
                throw;
            }
        }
    }
}