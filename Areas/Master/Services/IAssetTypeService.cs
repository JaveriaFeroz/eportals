using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IAssetTypeService
    {
        Task<IEnumerable<AssetTypeListViewModel>> GetAssetTypesAsync(bool activeOnly = true, string searchTerm = null);
        Task<AssetTypeViewModel> GetAssetTypeByIdAsync(short id);
        Task<bool> SaveAssetTypeAsync(AssetTypeViewModel model);
        Task<bool> DeleteAssetTypeAsync(short id);
        Task<bool> AssetTypeExistsAsync(string typeName, short? excludeId = null);
    }
    public class AssetTypeService : IAssetTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AssetTypeService> _logger;

        public AssetTypeService(ApplicationDbContext context, ILogger<AssetTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AssetTypeListViewModel>> GetAssetTypesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.AssetTypes
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
                    .Select(x => new AssetTypeListViewModel
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
                _logger.LogError(ex, "Error retrieving asset types");
                throw;
            }
        }

        public async Task<AssetTypeViewModel> GetAssetTypeByIdAsync(short id)
        {
            try
            {
                var entity = await _context.AssetTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.TypeId == id);

                if (entity == null) return null;

                return new AssetTypeViewModel
                {
                    TypeId = entity.TypeId,
                    TypeName = entity.TypeName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving asset type with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> SaveAssetTypeAsync(AssetTypeViewModel model)
        {
            try
            {
                AssetType entity;

                if (model.TypeId.HasValue && model.TypeId > 0)
                {
                    // Update existing
                    entity = await _context.AssetTypes.FindAsync(model.TypeId.Value);
                    if (entity == null) return false;

                    entity.TypeName = model.TypeName;
                    entity.IsActive = model.IsActive;
                }
                else
                {
                    // Create new
                    entity = new AssetType
                    {
                        TypeName = model.TypeName,
                        IsActive = model.IsActive
                    };
                    _context.AssetTypes.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving asset type");
                throw;
            }
        }

        public async Task<bool> DeleteAssetTypeAsync(short id)
        {
            try
            {
                var entity = await _context.AssetTypes.FindAsync(id);
                if (entity == null) return false;

                _context.AssetTypes.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting asset type with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> AssetTypeExistsAsync(string typeName, short? excludeId = null)
        {
            try
            {
                var query = _context.AssetTypes.Where(x => x.TypeName == typeName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.TypeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if asset type exists");
                throw;
            }
        }
    }
}