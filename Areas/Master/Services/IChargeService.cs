using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IChargeService
    {
        Task<IEnumerable<ChargeListViewModel>> GetChargesAsync(bool activeOnly = true, string searchTerm = null);
        Task<ChargeViewModel> GetChargeByIdAsync(short id);
        Task<bool> SaveChargeAsync(ChargeViewModel model);
        Task<bool> DeleteChargeAsync(short id);
        Task<bool> ChargeExistsAsync(string chargeName, short? excludeId = null);
    }

    public class ChargeService : IChargeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChargeService> _logger;

        public ChargeService(ApplicationDbContext context, ILogger<ChargeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ChargeListViewModel>> GetChargesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Charges
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.ChargeName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.ChargeName)
                    .Select(x => new ChargeListViewModel
                    {
                        ChargeId = x.ChargeId,
                        ChargeName = x.ChargeName,
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
                _logger.LogError(ex, "Error retrieving charges");
                throw;
            }
        }

        public async Task<ChargeViewModel> GetChargeByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Charges
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.ChargeId == id);

                if (entity == null) return null;

                return new ChargeViewModel
                {
                    ChargeId = entity.ChargeId,
                    ChargeName = entity.ChargeName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving charge with ID {ChargeId}", id);
                throw;
            }
        }

        public async Task<bool> SaveChargeAsync(ChargeViewModel model)
        {
            try
            {
                Charge entity;

                if (model.ChargeId.HasValue && model.ChargeId > 0)
                {
                    // Update existing
                    entity = await _context.Charges.FindAsync(model.ChargeId.Value);
                    if (entity == null) return false;

                    entity.ChargeName = model.ChargeName;
                    entity.IsActive = model.IsActive;
                }
                else
                {
                    // Create new
                    entity = new Charge
                    {
                        ChargeName = model.ChargeName,
                        IsActive = model.IsActive
                    };
                    _context.Charges.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving charge");
                throw;
            }
        }

        public async Task<bool> DeleteChargeAsync(short id)
        {
            try
            {
                var entity = await _context.Charges.FindAsync(id);
                if (entity == null) return false;

                _context.Charges.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting charge with ID {ChargeId}", id);
                throw;
            }
        }

        public async Task<bool> ChargeExistsAsync(string chargeName, short? excludeId = null)
        {
            try
            {
                var query = _context.Charges.Where(x => x.ChargeName == chargeName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.ChargeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if charge exists");
                throw;
            }
        }
    }
}
