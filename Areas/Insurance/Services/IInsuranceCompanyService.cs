using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Insurance.Models;
using ProcureToPay.Areas.Insurance.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Insurance.Services
{
    public interface IInsuranceCompanyService
    {
        Task<IEnumerable<InsuranceCompanyListViewModel>> GetInsuranceCompaniesAsync(bool activeOnly = true, string searchTerm = null);
        Task<InsuranceCompanyViewModel> GetInsuranceCompanyByIdAsync(short id);
        Task<bool> SaveInsuranceCompanyAsync(InsuranceCompanyViewModel model);
        Task<bool> DeleteInsuranceCompanyAsync(short id);
        Task<bool> InsuranceCompanyExistsAsync(string insuranceCompanyName, short? excludeId = null);
    }

    public class InsuranceCompanyService : IInsuranceCompanyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InsuranceCompanyService> _logger;

        public InsuranceCompanyService(ApplicationDbContext context, ILogger<InsuranceCompanyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<InsuranceCompanyListViewModel>> GetInsuranceCompaniesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.InsuranceCompanies
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.CompanyName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.CompanyName)
                    .Select(x => new InsuranceCompanyListViewModel
                    {
                        CompanyId = x.CompanyId,
                        CompanyName = x.CompanyName,
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
                _logger.LogError(ex, "Error retrieving insuranceCompanys");
                throw;
            }
        }

        public async Task<InsuranceCompanyViewModel> GetInsuranceCompanyByIdAsync(short id)
        {
            try
            {
                var entity = await _context.InsuranceCompanies
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.CompanyId == id);

                if (entity == null) return null;

                return new InsuranceCompanyViewModel
                {
                    CompanyId = entity.CompanyId,
                    CompanyName = entity.CompanyName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving insuranceCompany with ID {CompanyId}", id);
                throw;
            }
        }

        public async Task<bool> SaveInsuranceCompanyAsync(InsuranceCompanyViewModel model)
        {
            try
            {
                InsuranceCompany entity;

                if (model.CompanyId.HasValue && model.CompanyId > 0)
                {
                    // Update existing
                    entity = await _context.InsuranceCompanies.FindAsync(model.CompanyId.Value);
                    if (entity == null) return false;

                    entity.CompanyName = model.CompanyName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new InsuranceCompany
                    {
                        CompanyName = model.CompanyName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.InsuranceCompanies.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving insuranceCompany");
                throw;
            }
        }

        public async Task<bool> DeleteInsuranceCompanyAsync(short id)
        {
            try
            {
                var entity = await _context.InsuranceCompanies.FindAsync(id);
                if (entity == null) return false;

                _context.InsuranceCompanies.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting insuranceCompany with ID {CompanyId}", id);
                throw;
            }
        }

        public async Task<bool> InsuranceCompanyExistsAsync(string insuranceCompanyName, short? excludeId = null)
        {
            try
            {
                var query = _context.InsuranceCompanies.Where(x => x.CompanyName == insuranceCompanyName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.CompanyId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if insuranceCompany exists");
                throw;
            }
        }
    }
}