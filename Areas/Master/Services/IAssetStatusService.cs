using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IAssetStatusService
    {
        Task<IEnumerable<AssetStatusListViewModel>> GetAssetStatusesAsync(bool activeOnly = true, string searchTerm = null);
        Task<AssetStatusViewModel> GetAssetStatusByIdAsync(short id);
        Task<bool> SaveAssetStatusAsync(AssetStatusViewModel model);
        Task<bool> DeleteAssetStatusAsync(short id);
        Task<bool> AssetStatusExistsAsync(string statusName, short? excludeId = null);
    }
    public class AssetStatusService : IAssetStatusService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AssetStatusService> _logger;

        public AssetStatusService(ApplicationDbContext context, ILogger<AssetStatusService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AssetStatusListViewModel>> GetAssetStatusesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.AssetStatuses
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.StatusName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.StatusName)
                    .Select(x => new AssetStatusListViewModel
                    {
                        StatusId = x.StatusId,
                        StatusName = x.StatusName,
                        Editable = x.Editable,
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
                _logger.LogError(ex, "Error retrieving asset statuses");
                throw;
            }
        }

        public async Task<AssetStatusViewModel> GetAssetStatusByIdAsync(short id)
        {
            try
            {
                var entity = await _context.AssetStatuses
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.StatusId == id);

                if (entity == null) return null;

                return new AssetStatusViewModel
                {
                    StatusId = entity.StatusId,
                    StatusName = entity.StatusName,
                    Editable = entity.Editable,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving asset status with ID {StatusId}", id);
                throw;
            }
        }

        public async Task<bool> SaveAssetStatusAsync(AssetStatusViewModel model)
        {
            try
            {
                AssetStatus entity;

                if (model.StatusId.HasValue && model.StatusId > 0)
                {
                    // Update existing
                    entity = await _context.AssetStatuses.FindAsync(model.StatusId.Value);
                    if (entity == null) return false;

                    entity.StatusName = model.StatusName;
                    entity.Editable = model.Editable;
                    entity.IsActive = model.IsActive;
                }
                else
                {
                    // Create new
                    entity = new AssetStatus
                    {
                        StatusName = model.StatusName,
                        Editable = model.Editable,
                        IsActive = model.IsActive
                    };
                    _context.AssetStatuses.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving asset status");
                throw;
            }
        }

        public async Task<bool> DeleteAssetStatusAsync(short id)
        {
            try
            {
                var entity = await _context.AssetStatuses.FindAsync(id);
                if (entity == null) return false;

                _context.AssetStatuses.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting asset status with ID {StatusId}", id);
                throw;
            }
        }

        public async Task<bool> AssetStatusExistsAsync(string statusName, short? excludeId = null)
        {
            try
            {
                var query = _context.AssetStatuses.Where(x => x.StatusName == statusName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.StatusId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if asset status exists");
                throw;
            }
        }
    }
}


