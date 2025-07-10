using Microsoft.EntityFrameworkCore;
using ProcureToPay.Data;
using ProcureToPay.Areas.Insurance.Models;
using ProcureToPay.Areas.Insurance.ViewModels;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using ProcureToPay.Areas.Master.ViewModels;

namespace ProcureToPay.Areas.Insurance.Services
{
    public interface IInsuranceTypeService
    {
        Task<IEnumerable<InsuranceTypeListViewModel>> GetInsuranceTypesAsync(bool activeOnly = true, string searchTerm = null);
        Task<InsuranceTypeViewModel> GetInsuranceTypeByIdAsync(short id);
        Task<bool> SaveInsuranceTypeAsync(InsuranceTypeViewModel model);
        Task<bool> DeleteInsuranceTypeAsync(short id);
        Task<bool> TypeNameExistsAsync(string skuTypeName, short? excludeId = null);
        Task<InsuranceTypeLookupsViewModel> GetLookupsAsync();
        Task<List<InsuranceDocumentTypeListViewModel>> GetRequiredDocumentsAsync();
    }

    public class InsuranceTypeService : IInsuranceTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InsuranceTypeService> _logger;

        public InsuranceTypeService(ApplicationDbContext context, ILogger<InsuranceTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<InsuranceTypeListViewModel>> GetInsuranceTypesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.InsuranceTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.TypeName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.TypeName)
                    .Select(x => new InsuranceTypeListViewModel
                    {
                        TypeId = x.TypeId,
                        TypeName = x.TypeName,
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
                _logger.LogError(ex, "Error retrieving skuCategories");
                throw;
            }
        }

        public async Task<InsuranceTypeViewModel> GetInsuranceTypeByIdAsync(short id)
        {
            try
            {
                var entity = await _context.InsuranceTypes
                    .Include(x => x.Company)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.TypeId == id);

                if (entity == null) return null;

                return new InsuranceTypeViewModel
                {
                    TypeId = entity.TypeId,
                    TypeName = entity.TypeName, // Added this missing property
                    CompanyId = entity.CompanyId,
                    CompanyName = entity.Company?.CompanyName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving skuType with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> SaveInsuranceTypeAsync(InsuranceTypeViewModel model)
        {
            try
            {
                InsuranceType entity;

                if (model.TypeId.HasValue && model.TypeId > 0)
                {
                    // Update existing
                    entity = await _context.InsuranceTypes.FindAsync(model.TypeId.Value);
                    if (entity == null) return false;

                    entity.TypeName = model.TypeName;
                    entity.CompanyId = model.CompanyId;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new InsuranceType
                    {
                        TypeName = model.TypeName,
                        CompanyId = model.CompanyId, // FIX: Corrected property assignment
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.InsuranceTypes.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving skuType");
                throw;
            }
        }

        public async Task<bool> DeleteInsuranceTypeAsync(short id)
        {
            try
            {
                var entity = await _context.InsuranceTypes.FindAsync(id);
                if (entity == null) return false;

                _context.InsuranceTypes.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting skuType with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> TypeNameExistsAsync(string skuTypeName, short? excludeId = null)
        {
            try
            {
                var query = _context.InsuranceTypes.Where(x => x.TypeName == skuTypeName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.TypeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if skuType name exists");
                throw;
            }
        }

        public async Task<InsuranceTypeLookupsViewModel> GetLookupsAsync()
        {
            try
            {
                var lookups = new InsuranceTypeLookupsViewModel();


                lookups.Companies = await _context.Companies
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.CompanyName)
                   .Select(x => new CompanyLookupViewModel
                   {
                       CompanyId = x.CompanyId,
                       CompanyName = x.CompanyName
                   })
                   .ToListAsync();

                return lookups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lookups");
                throw;
            }
        }

        public async Task<List<InsuranceDocumentTypeListViewModel>> GetRequiredDocumentsAsync()
        {
            try
            {
                return await _context.InsuranceDocumentTypes
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.TypeName)
                    .Select(x => new InsuranceDocumentTypeListViewModel
                    {
                        TypeId = x.TypeId,
                        TypeName = x.TypeName,
                        IsActive = x.IsActive,
                        CreatedBy = x.CreatedByUser != null ? $"{x.CreatedByUser.FirstName} {x.CreatedByUser.LastName}" : "System",
                        CreatedOn = x.CreatedOn,
                        UpdatedBy = x.UpdatedByUser != null ? $"{x.UpdatedByUser.FirstName} {x.UpdatedByUser.LastName}" : null,
                        UpdatedOn = x.UpdatedOn
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving required documents");
                throw;
            }
        }
    }
}
