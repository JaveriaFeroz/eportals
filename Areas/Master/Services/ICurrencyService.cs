using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface ICurrencyService
    {
        Task<IEnumerable<CurrencyListViewModel>> GetCurrenciesAsync(bool activeOnly = true, string searchTerm = null);
        Task<CurrencyViewModel> GetCurrencyByIdAsync(short id);
        Task<bool> SaveCurrencyAsync(CurrencyViewModel model);
        Task<bool> DeleteCurrencyAsync(short id);
        Task<bool> CurrencyExistsAsync(string currencyName, short? excludeId = null);
    }

    public class CurrencyService : ICurrencyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CurrencyService> _logger;

        public CurrencyService(ApplicationDbContext context, ILogger<CurrencyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<CurrencyListViewModel>> GetCurrenciesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Currencies
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.CurrencyName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.CurrencyName)
                    .Select(x => new CurrencyListViewModel
                    {
                        CurrencyId = x.CurrencyId,
                        CurrencyName = x.CurrencyName,
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
                _logger.LogError(ex, "Error retrieving currencies");
                throw;
            }
        }

        public async Task<CurrencyViewModel> GetCurrencyByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Currencies
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.CurrencyId == id);

                if (entity == null) return null;

                return new CurrencyViewModel
                {
                    CurrencyId = entity.CurrencyId,
                    CurrencyName = entity.CurrencyName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving currency with ID {CurrencyId}", id);
                throw;
            }
        }

        public async Task<bool> SaveCurrencyAsync(CurrencyViewModel model)
        {
            try
            {
                Currency entity;

                if (model.CurrencyId.HasValue && model.CurrencyId > 0)
                {
                    // Update existing
                    entity = await _context.Currencies.FindAsync(model.CurrencyId.Value);
                    if (entity == null) return false;

                    entity.CurrencyName = model.CurrencyName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new Currency
                    {
                        CurrencyName = model.CurrencyName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.Currencies.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving currency");
                throw;
            }
        }

        public async Task<bool> DeleteCurrencyAsync(short id)
        {
            try
            {
                var entity = await _context.Currencies.FindAsync(id);
                if (entity == null) return false;

                _context.Currencies.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting currency with ID {CurrencyId}", id);
                throw;
            }
        }

        public async Task<bool> CurrencyExistsAsync(string currencyName, short? excludeId = null)
        {
            try
            {
                var query = _context.Currencies.Where(x => x.CurrencyName == currencyName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.CurrencyId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if currency exists");
                throw;
            }
        }
    }
}