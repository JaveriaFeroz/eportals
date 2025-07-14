using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IServiceNatureService
    {
        Task<IEnumerable<ServiceNatureListViewModel>> GetServiceNaturesAsync(bool activeOnly = true, string searchTerm = null);
        Task<ServiceNatureViewModel> GetServiceNatureByIdAsync(short id);
        Task<bool> SaveServiceNatureAsync(ServiceNatureViewModel model);
        Task<bool> DeleteServiceNatureAsync(short id);
        Task<bool> ServiceNatureExistsAsync(string natureName, short? excludeId = null);

    }

    public class ServiceNatureService : IServiceNatureService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ServiceNatureService> _logger;

        public ServiceNatureService(ApplicationDbContext context, ILogger<ServiceNatureService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ServiceNatureListViewModel>> GetServiceNaturesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.ServiceNatures
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.NatureName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.NatureName)
                    .Select(x => new ServiceNatureListViewModel
                    {
                        NatureId = x.NatureId,
                        NatureName = x.NatureName,
                        IsOpex = x.IsOpex,
                        IsCapex = x.IsCapex,
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
                _logger.LogError(ex, "Error retrieving service natures");
                throw;
            }
        }

        public async Task<ServiceNatureViewModel> GetServiceNatureByIdAsync(short id)
        {
            try
            {
                var entity = await _context.ServiceNatures
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.NatureId == id);

                if (entity == null) return null;

                return new ServiceNatureViewModel
                {
                    NatureId = entity.NatureId,
                    NatureName = entity.NatureName,
                    IsOpex = entity.IsOpex,
                    IsCapex = entity.IsCapex,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service nature with ID {NatureId}", id);
                throw;
            }
        }

        public async Task<bool> SaveServiceNatureAsync(ServiceNatureViewModel model)
        {
            try
            {
                ServiceNature entity;

                if (model.NatureId.HasValue && model.NatureId > 0)
                {
                    // Update existing
                    entity = await _context.ServiceNatures.FindAsync(model.NatureId.Value);
                    if (entity == null) return false;

                    entity.NatureName = model.NatureName;
                    entity.IsOpex = model.IsOpex;
                    entity.IsCapex = model.IsCapex;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new ServiceNature
                    {
                        NatureName = model.NatureName,
                        IsOpex = model.IsOpex,
                        IsCapex = model.IsCapex,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.ServiceNatures.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving service nature");
                throw;
            }
        }

        public async Task<bool> DeleteServiceNatureAsync(short id)
        {
            try
            {
                var entity = await _context.ServiceNatures.FindAsync(id);
                if (entity == null) return false;

                _context.ServiceNatures.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service nature with ID {NatureId}", id);
                throw;
            }
        }

        public async Task<bool> ServiceNatureExistsAsync(string natureName, short? excludeId = null)
        {
            try
            {
                var query = _context.ServiceNatures.Where(x => x.NatureName == natureName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.NatureId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if service nature exists");
                throw;
            }
        }

    }
}