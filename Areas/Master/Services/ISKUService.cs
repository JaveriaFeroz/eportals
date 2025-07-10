using Microsoft.EntityFrameworkCore;
using ProcureToPay.Data;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ProcureToPay.Areas.Master.Services
{
    public interface ISKUService
    {
        Task<IEnumerable<SKUListViewModel>> GetSKUsAsync(bool activeOnly = true, string searchTerm = null);
        Task<SKUViewModel> GetSKUByIdAsync(short id);
        Task<bool> SaveSKUAsync(SKUViewModel model);
        Task<bool> DeleteSKUAsync(short id);
        Task<bool> SKUNameExistsAsync(string skuName, short? excludeId = null);
        Task<SKULookupsViewModel> GetLookupsAsync();
    }

    public class SKUService : ISKUService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SKUService> _logger;

        public SKUService(ApplicationDbContext context, ILogger<SKUService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SKUListViewModel>> GetSKUsAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.SKUs
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.SKUName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.SKUName)
                    .Select(x => new SKUListViewModel
                    {
                        SKUId = x.SKUId,
                        SKUName = x.SKUName,
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
                // FIX: Corrected log message.
                _logger.LogError(ex, "Error retrieving SKUs");
                throw;
            }
        }

        public async Task<SKUViewModel> GetSKUByIdAsync(short id)
        {
            try
            {
                var entity = await _context.SKUs
                    .Include(x => x.SKUType)
                    .Include(x => x.Company)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.SKUId == id);

                if (entity == null) return null;

                return new SKUViewModel
                {
                    SKUId = entity.SKUId,
                    SKUName = entity.SKUName,
                    CompanyId = entity.CompanyId,
                    SKUTypeId = entity.SKUTypeId,
                    CompanyName = entity.Company?.CompanyName,
                    TypeName = entity.SKUType?.TypeName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sku with ID {SKUId}", id);
                throw;
            }
        }

        public async Task<bool> SaveSKUAsync(SKUViewModel model)
        {
            try
            {
                SKU entity;

                if (model.SKUId.HasValue && model.SKUId > 0)
                {
                    // Update existing
                    entity = await _context.SKUs.FindAsync(model.SKUId.Value);
                    if (entity == null) return false;

                    entity.SKUName = model.SKUName;
                    entity.CompanyId = model.CompanyId;
                    entity.SKUTypeId = model.SKUTypeId; // FIX: Added SKUTypeId update
                    entity.IsActive = model.IsActive;
                }
                else
                {
                    // Create new
                    entity = new SKU
                    {
                        SKUName = model.SKUName,
                        CompanyId = model.CompanyId,
                        SKUTypeId = model.SKUTypeId, // FIX: Added SKUTypeId for new entities
                        IsActive = model.IsActive
                    };
                    _context.SKUs.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving sku");
                throw;
            }
        }

        public async Task<bool> DeleteSKUAsync(short id)
        {
            try
            {
                var entity = await _context.SKUs.FindAsync(id);
                if (entity == null) return false;

                _context.SKUs.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sku with ID {SKUId}", id);
                throw;
            }
        }

        public async Task<bool> SKUNameExistsAsync(string skuName, short? excludeId = null)
        {
            try
            {
                var query = _context.SKUs.Where(x => x.SKUName == skuName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.SKUId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if sku name exists");
                throw;
            }
        }

        public async Task<SKULookupsViewModel> GetLookupsAsync()
        {
            try
            {
                var lookups = new SKULookupsViewModel();

                lookups.Companies = await _context.Companies
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.CompanyName)
                   .Select(x => new ViewModels.CompanyLookupViewModel
                   {
                       CompanyId = x.CompanyId,
                       CompanyName = x.CompanyName
                   })
                   .ToListAsync();

                // FIX: Added logic to fetch SKU Types for the dropdown.
                lookups.Types = await _context.SKUTypes
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.TypeName)
                   .Select(x => new SKUTypeLookupViewModel
                   {
                       SKUTypeId = x.TypeId,
                       SKUTypeName = x.TypeName
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
    }
}