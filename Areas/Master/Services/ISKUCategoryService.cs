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
    public interface ISKUCategoryService
    {
        Task<IEnumerable<SKUCategoryListViewModel>> GetSKUCategoriesAsync(bool activeOnly = true, string searchTerm = null);
        Task<SKUCategoryViewModel> GetSKUCategoryByIdAsync(short id);
        Task<bool> SaveSKUCategoryAsync(SKUCategoryViewModel model);
        Task<bool> DeleteSKUCategoryAsync(short id);
        Task<bool> CategoryNameExistsAsync(string skuCategoryName, short? excludeId = null);
        Task<SKUCategoryLookupsViewModel> GetLookupsAsync();
        //  Task<IEnumerable<WorkflowSKUCategoryViewModel>> GetPendingWorkflowFormsAsync();
    }

    public class SKUCategoryService : ISKUCategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SKUCategoryService> _logger;

        public SKUCategoryService(ApplicationDbContext context, ILogger<SKUCategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SKUCategoryListViewModel>> GetSKUCategoriesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.SKUCategories
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.CategoryName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.CategoryName)
                    .Select(x => new SKUCategoryListViewModel
                    {
                        CategoryId = x.CategoryId,
                        CategoryName = x.CategoryName,
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

        public async Task<SKUCategoryViewModel> GetSKUCategoryByIdAsync(short id)
        {
            try
            {
                var entity = await _context.SKUCategories
                    .Include(x => x.Company)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.CategoryId == id);

                if (entity == null) return null;

                return new SKUCategoryViewModel
                {
                    CategoryId = entity.CategoryId,
                    CategoryName = entity.CategoryName, // Added this missing property
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
                _logger.LogError(ex, "Error retrieving skuCategory with ID {CategoryId}", id);
                throw;
            }
        }

        public async Task<bool> SaveSKUCategoryAsync(SKUCategoryViewModel model)
        {
            try
            {
                SKUCategory entity;

                if (model.CategoryId.HasValue && model.CategoryId > 0)
                {
                    // Update existing
                    entity = await _context.SKUCategories.FindAsync(model.CategoryId.Value);
                    if (entity == null) return false;

                    entity.CategoryName = model.CategoryName;
                    entity.CompanyId = model.CompanyId;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new SKUCategory
                    {
                        CategoryName = model.CategoryName,
                        CompanyId = model.CompanyId, // FIX: Corrected property assignment
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.SKUCategories.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving skuCategory");
                throw;
            }
        }

        public async Task<bool> DeleteSKUCategoryAsync(short id)
        {
            try
            {
                var entity = await _context.SKUCategories.FindAsync(id);
                if (entity == null) return false;

                _context.SKUCategories.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting skuCategory with ID {CategoryId}", id);
                throw;
            }
        }

        public async Task<bool> CategoryNameExistsAsync(string skuCategoryName, short? excludeId = null)
        {
            try
            {
                var query = _context.SKUCategories.Where(x => x.CategoryName == skuCategoryName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.CategoryId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if skuCategory name exists");
                throw;
            }
        }

        public async Task<SKUCategoryLookupsViewModel> GetLookupsAsync()
        {
            try
            {
                var lookups = new SKUCategoryLookupsViewModel();


                lookups.Companies = await _context.Companies
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.CompanyName)
                   .Select(x => new ViewModels.CompanyLookupViewModel 
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

        //public async Task<IEnumerable<WorkflowSKUCategoryViewModel>> GetPendingWorkflowFormsAsync()
        //{
        //    try
        //    {
        //        // This would depend on your workflow implementation
        //        // Assuming you have a WorkflowForms table or similar
        //        var result = await _context.WorkflowForms
        //            .Include(x => x.SKUCategory)
        //            .Include(x => x.WorkflowState)
        //            .Where(x => x.IsActive && !x.IsCompleted)
        //            .Select(x => new WorkflowSKUCategoryViewModel
        //            {
        //                FormId = x.FormId,
        //                CategoryId = x.CategoryId,
        //                CategoryName = x.SKUCategory.CategoryName,
        //                StateName = x.WorkflowState.StateName
        //            })
        //            .ToListAsync();
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving pending workflow forms");
        //        throw;
        //    }
        //}
    }
}
