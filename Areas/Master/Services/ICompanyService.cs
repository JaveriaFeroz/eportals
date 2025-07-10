using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace ProcureToPay.Areas.Master.Services
{
    /// <summary>
    /// Defines the contract for the company service, outlining the operations for managing companies.
    /// </summary>
    public interface ICompanyService
    {
        
        Task<IEnumerable<CompanyListViewModel>> GetCompaniesAsync(bool activeOnly = true, string searchTerm = null);
        Task<CompanyViewModel> GetCompanyByIdAsync(short id);
        Task<bool> SaveCompanyAsync(CompanyViewModel model);
        Task<bool> DeleteCompanyAsync(short id);
        Task<bool> CompanyExistsAsync(string companyName, short? excludeId = null);
    }

    /// <summary>
    /// Implements the ICompanyService interface to provide data access and business logic for companies.
    /// </summary>
    public class CompanyService : ICompanyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompanyService> _logger;

        /// <summary>
        /// Initializes a new instance of the CompanyService class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger for logging service activities.</param>
        public CompanyService(ApplicationDbContext context, ILogger<CompanyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CompanyListViewModel>> GetCompaniesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Companies
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var lowercasedSearchTerm = searchTerm.ToLower();
                    query = query.Where(x => x.CompanyName.ToLower().Contains(lowercasedSearchTerm) ||
                                             (x.CompanyAddress != null && x.CompanyAddress.ToLower().Contains(lowercasedSearchTerm)) ||
                                             (x.NTN != null && x.NTN.ToLower().Contains(lowercasedSearchTerm)));
                }

                var result = await query
                    .OrderBy(x => x.CompanyName)
                    .Select(x => new CompanyListViewModel
                    {
                        CompanyId = x.CompanyId,
                        CompanyName = x.CompanyName,
                        CompanyAddress = x.CompanyAddress,
                        NTN = x.NTN,
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
                _logger.LogError(ex, "Error retrieving companies");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<CompanyViewModel> GetCompanyByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Companies
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.CompanyId == id);

                if (entity == null) return null;

                return new CompanyViewModel
                {
                    CompanyId = entity.CompanyId,
                    CompanyName = entity.CompanyName,
                    CompanyAddress = entity.CompanyAddress,
                    NTN = entity.NTN,
                    PeriodName = entity.PeriodName,
                    DistanceThreshold = entity.DistanceThreshold,
                    ReportGraceHRs = entity.ReportGraceHRs,
                    BankAccountId = entity.BankAccountId,
                    ARPeriodId = entity.ARPeriodId,
                    ARAccountId = entity.ARAccountId,
                    APPeriodId = entity.APPeriodId,
                    APAccountId = entity.APAccountId,
                    GLPeriodId = entity.GLPeriodId,
                    OpsPeriodId = entity.OpsPeriodId,
                    TripRevenueAccountId = entity.TripRevenueAccountId,
                    FuelExpenseAccountId = entity.FuelExpenseAccountId,
                    AdvanceAccountId = entity.AdvanceAccountId,
                    EnableGL = entity.EnableGL,
                    EnablePartialDelivery = entity.EnablePartialDelivery,
                    RouteByConsignee = entity.RouteByConsignee,
                    SeparateFixedInvoice = entity.SeparateFixedInvoice,
                    IsMandatoryDriver2 = entity.IsMandatoryDriver2,
                    AllowTrailer = entity.AllowTrailer,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company with ID {CompanyId}", id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SaveCompanyAsync(CompanyViewModel model)
        {
            try
            {
                Company entity;

                if (model.CompanyId.HasValue && model.CompanyId > 0)
                {
                    // Update existing company
                    entity = await _context.Companies.FindAsync(model.CompanyId.Value);
                    if (entity == null)
                    {
                        _logger.LogWarning("Attempted to update a non-existent company with ID {CompanyId}", model.CompanyId.Value);
                        return false;
                    }
                }
                else
                {
                    // Create new company
                    entity = new Company();
                    _context.Companies.Add(entity);
                }

                // Map properties from ViewModel to Entity
                entity.CompanyName = model.CompanyName;
                entity.CompanyAddress = model.CompanyAddress;
                entity.NTN = model.NTN;
                entity.PeriodName = model.PeriodName;
                entity.DistanceThreshold = model.DistanceThreshold;
                entity.ReportGraceHRs = model.ReportGraceHRs;
                entity.BankAccountId = model.BankAccountId;
                entity.ARPeriodId = model.ARPeriodId;
                entity.ARAccountId = model.ARAccountId;
                entity.APPeriodId = model.APPeriodId;
                entity.APAccountId = model.APAccountId;
                entity.GLPeriodId = model.GLPeriodId;
                entity.OpsPeriodId = model.OpsPeriodId;
                entity.TripRevenueAccountId = model.TripRevenueAccountId;
                entity.FuelExpenseAccountId = model.FuelExpenseAccountId;
                entity.AdvanceAccountId = model.AdvanceAccountId;
                entity.EnableGL = model.EnableGL;
                entity.EnablePartialDelivery = model.EnablePartialDelivery;
                entity.RouteByConsignee = model.RouteByConsignee;
                entity.SeparateFixedInvoice = model.SeparateFixedInvoice;
                entity.IsMandatoryDriver2 = model.IsMandatoryDriver2;
                entity.AllowTrailer = model.AllowTrailer;
                entity.IsActive = model.IsActive;

                // CreatedBy, CreatedOn, UpdatedBy, and UpdatedOn are assumed to be handled
                // by the DbContext's change tracker or a base entity configuration.

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving company for CompanyId: {CompanyId}", model.CompanyId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteCompanyAsync(short id)
        {
            try
            {
                var entity = await _context.Companies.FindAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("Attempted to delete a non-existent company with ID {CompanyId}", id);
                    return false;
                }

                // Hard delete, consistent with ContractorService
                _context.Companies.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company with ID {CompanyId}", id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> CompanyExistsAsync(string companyName, short? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(companyName))
                {
                    return false;
                }

                var query = _context.Companies.Where(x => x.CompanyName.ToLower() == companyName.ToLower());

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.CompanyId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if company exists with name {CompanyName}", companyName);
                throw;
            }
        }
    }
}
