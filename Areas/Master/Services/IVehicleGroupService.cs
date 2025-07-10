using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IVehicleGroupService
    {
        Task<IEnumerable<VehicleGroupListViewModel>> GetVehicleGroupsAsync(bool activeOnly = true, string searchTerm = null);
        Task<VehicleGroupViewModel> GetVehicleGroupByIdAsync(short id);
        Task<bool> SaveVehicleGroupAsync(VehicleGroupViewModel model);
        Task<bool> DeleteVehicleGroupAsync(short id);
        Task<bool> VehicleGroupExistsAsync(string vehicleGroupName, short? excludeId = null);
    }

    public class VehicleGroupService : IVehicleGroupService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VehicleGroupService> _logger;

        public VehicleGroupService(ApplicationDbContext context, ILogger<VehicleGroupService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<VehicleGroupListViewModel>> GetVehicleGroupsAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.VehicleGroups
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.GroupName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.GroupName)
                    .Select(x => new VehicleGroupListViewModel
                    {
                        GroupId = x.GroupId,
                        GroupName = x.GroupName,
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
                _logger.LogError(ex, "Error retrieving vehicleGroups");
                throw;
            }
        }

        public async Task<VehicleGroupViewModel> GetVehicleGroupByIdAsync(short id)
        {
            try
            {
                var entity = await _context.VehicleGroups
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.GroupId == id);

                if (entity == null) return null;

                return new VehicleGroupViewModel
                {
                    GroupId = entity.GroupId,
                    GroupName = entity.GroupName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicleGroup with ID {GroupId}", id);
                throw;
            }
        }

        public async Task<bool> SaveVehicleGroupAsync(VehicleGroupViewModel model)
        {
            try
            {
                VehicleGroup entity;

                if (model.GroupId.HasValue && model.GroupId > 0)
                {
                    // Update existing
                    entity = await _context.VehicleGroups.FindAsync(model.GroupId.Value);
                    if (entity == null) return false;

                    entity.GroupName = model.GroupName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new VehicleGroup
                    {
                        GroupName = model.GroupName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.VehicleGroups.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving vehicleGroup");
                throw;
            }
        }

        public async Task<bool> DeleteVehicleGroupAsync(short id)
        {
            try
            {
                var entity = await _context.VehicleGroups.FindAsync(id);
                if (entity == null) return false;

                _context.VehicleGroups.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicleGroup with ID {GroupId}", id);
                throw;
            }
        }

        public async Task<bool> VehicleGroupExistsAsync(string vehicleGroupName, short? excludeId = null)
        {
            try
            {
                var query = _context.VehicleGroups.Where(x => x.GroupName == vehicleGroupName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.GroupId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if vehicleGroup exists");
                throw;
            }
        }
    }
}