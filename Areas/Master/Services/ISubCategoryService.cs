using Microsoft.EntityFrameworkCore;
using ProcureToPay.Data;
using ProcureToPay.Areas.Master.Models;

namespace ProcureToPay.Areas.Master.Services
{
    public interface ISubCategoryService
    {
        Task<IEnumerable<SubCategoryListViewModel>> GetSubCategoriesAsync(bool activeOnly = true, string searchTerm = null);
        Task<SubCategoryViewModel> GetSubCategoryByIdAsync(short id);
        Task<bool> SaveSubCategoryAsync(SubCategoryViewModel model);
        Task<bool> DeleteSubCategoryAsync(short id);
        Task<bool> SubCategoryExistsAsync(string subCategoryName, short? excludeId = null);
    }

    public class SubCategoryService : ISubCategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubCategoryService> _logger;

        public SubCategoryService(ApplicationDbContext context, ILogger<SubCategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SubCategoryListViewModel>> GetSubCategoriesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.SubCategories
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.SubCategoryName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.SubCategoryName)
                    .Select(x => new SubCategoryListViewModel
                    {
                        SubCategoryId = x.SubCategoryId,
                        SubCategoryName = x.SubCategoryName,
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
                _logger.LogError(ex, "Error retrieving sub categories");
                throw;
            }
        }

        public async Task<SubCategoryViewModel> GetSubCategoryByIdAsync(short id)
        {
            try
            {
                var entity = await _context.SubCategories
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.SubCategoryId == id);

                if (entity == null) return null;

                return new SubCategoryViewModel
                {
                    SubCategoryId = entity.SubCategoryId,
                    SubCategoryName = entity.SubCategoryName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sub category with ID {SubCategoryId}", id);
                throw;
            }
        }

        public async Task<bool> SaveSubCategoryAsync(SubCategoryViewModel model)
        {
            try
            {
                SubCategory entity;

                if (model.SubCategoryId.HasValue && model.SubCategoryId > 0)
                {
                    // Update existing
                    entity = await _context.SubCategories.FindAsync(model.SubCategoryId.Value);
                    if (entity == null) return false;

                    entity.SubCategoryName = model.SubCategoryName;
                    entity.IsActive = model.IsActive;
                    // UpdatedBy/UpdatedOn handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new SubCategory
                    {
                        SubCategoryName = model.SubCategoryName,
                        IsActive = model.IsActive
                        // CreatedBy/CreatedOn handled automatically by DbContext
                    };
                    _context.SubCategories.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving sub category");
                throw;
            }
        }

        public async Task<bool> DeleteSubCategoryAsync(short id)
        {
            try
            {
                var entity = await _context.SubCategories.FindAsync(id);
                if (entity == null) return false;

                _context.SubCategories.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sub category with ID {SubCategoryId}", id);
                throw;
            }
        }

        public async Task<bool> SubCategoryExistsAsync(string subCategoryName, short? excludeId = null)
        {
            try
            {
                var query = _context.SubCategories.Where(x => x.SubCategoryName == subCategoryName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.SubCategoryId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if sub category exists");
                throw;
            }
        }
    }
}