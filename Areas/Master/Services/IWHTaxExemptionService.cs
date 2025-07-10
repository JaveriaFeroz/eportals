using Microsoft.EntityFrameworkCore;
using ProcureToPay.Data;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IWHTaxExemptionService
    {
        Task<IEnumerable<WHTaxExemptionListViewModel>> GetWHTaxExemptionsAsync(bool activeOnly = true, string searchTerm = null);
        Task<WHTaxExemptionViewModel> GetWHTaxExemptionByIdAsync(short id);
        Task<bool> SaveWHTaxExemptionAsync(WHTaxExemptionViewModel model);
        Task<bool> DeleteWHTaxExemptionAsync(short id);
        Task<bool> DateRangeOverlapsAsync(DateTime dateFrom, DateTime dateTo, short companyId, short? excludeId = null);
        Task<WHTaxExemptionLookupsViewModel> GetLookupsAsync();
    }

    public class WHTaxExemptionService : IWHTaxExemptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WHTaxExemptionService> _logger;

        public WHTaxExemptionService(ApplicationDbContext context, ILogger<WHTaxExemptionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // FIX: Removed companyId from the query. It now fetches all records.
        public async Task<IEnumerable<WHTaxExemptionListViewModel>> GetWHTaxExemptionsAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.WHTaxExemptions
                    .Include(x => x.Company) // Include Company to display its name
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    // Searching on CompanyName as well now.
                    query = query.Where(x => x.Company.CompanyName.Contains(searchTerm) ||
                                             x.DateFrom.ToString().Contains(searchTerm) ||
                                             x.DateTo.ToString().Contains(searchTerm));
                }

                return await query
                    .OrderBy(x => x.Company.CompanyName).ThenBy(x => x.DateFrom)
                    .Select(x => new WHTaxExemptionListViewModel
                    {
                        ExemptionId = x.ExemptionId,
                        DateFrom = x.DateFrom,
                        DateTo = x.DateTo,
                        IsActive = x.IsActive,
                        CreatedBy = x.CreatedByUser != null ? $"{x.CreatedByUser.FirstName} {x.CreatedByUser.LastName}" : "System",
                        CreatedOn = x.CreatedOn
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving WH tax exemptions");
                throw;
            }
        }

        // FIX: Removed companyId check. Fetches by primary key only.
        public async Task<WHTaxExemptionViewModel> GetWHTaxExemptionByIdAsync(short id)
        {
            try
            {
                var entity = await _context.WHTaxExemptions
                    .Include(x => x.Company)
                    .FirstOrDefaultAsync(x => x.ExemptionId == id);

                if (entity == null) return null;

                return new WHTaxExemptionViewModel
                {
                    ExemptionId = entity.ExemptionId,
                    DateFrom = entity.DateFrom,
                    DateTo = entity.DateTo,
                    CompanyId = entity.CompanyId,
                    CompanyName = entity.Company?.CompanyName,
                    IsActive = entity.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving WH tax exemption with ID {ExemptionId}", id);
                throw;
            }
        }

        // FIX: Simplified to take only the view model, like in SKUService.
        public async Task<bool> SaveWHTaxExemptionAsync(WHTaxExemptionViewModel model)
        {
            try
            {
                WHTaxExemption entity;

                if (model.ExemptionId.HasValue && model.ExemptionId > 0)
                {
                    entity = await _context.WHTaxExemptions.FindAsync(model.ExemptionId.Value);
                    if (entity == null) return false;

                    entity.DateFrom = model.DateFrom;
                    entity.DateTo = model.DateTo;
                    entity.CompanyId = model.CompanyId;
                    entity.IsActive = model.IsActive;
                }
                else
                {
                    entity = new WHTaxExemption
                    {
                        DateFrom = model.DateFrom,
                        DateTo = model.DateTo,
                        IsActive = model.IsActive,
                        CompanyId = model.CompanyId
                    };
                    _context.WHTaxExemptions.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving WH tax exemption");
                throw;
            }
        }

        // FIX: Removed companyId check. Deletes by primary key only.
        public async Task<bool> DeleteWHTaxExemptionAsync(short id)
        {
            try
            {
                var entity = await _context.WHTaxExemptions.FindAsync(id);
                if (entity == null) return false;

                _context.WHTaxExemptions.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting WH tax exemption with ID {ExemptionId}", id);
                throw;
            }
        }

        // FIX: This method must retain companyId to function logically.
        public async Task<bool> DateRangeOverlapsAsync(DateTime dateFrom, DateTime dateTo, short companyId, short? excludeId = null)
        {
            try
            {
                var query = _context.WHTaxExemptions
                    .Where(x => x.CompanyId == companyId && x.IsActive)
                    .Where(x => dateFrom <= x.DateTo && dateTo >= x.DateFrom);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.ExemptionId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking date range overlap for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<WHTaxExemptionLookupsViewModel> GetLookupsAsync()
        {
            var lookups = new WHTaxExemptionLookupsViewModel
            {
                Companies = await _context.Companies
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.CompanyName)
                   .Select(x => new Models.CompanyLookupViewModel
                   {
                       CompanyId = x.CompanyId,
                       CompanyName = x.CompanyName
                   })
                   .ToListAsync()
            };
            return lookups;
        }
    }
}