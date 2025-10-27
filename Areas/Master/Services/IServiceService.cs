using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IServiceService
    {
        Task<IEnumerable<ServiceListViewModel>> GetServicesAsync(bool activeOnly = true, string searchTerm = null);
        Task<ServiceViewModel> GetServiceByIdAsync(short id);
        Task<bool> SaveServiceAsync(ServiceViewModel model);
        Task<bool> DeleteServiceAsync(short id);
        Task<bool> ServiceExistsAsync(string serviceName, short? excludeId = null);
        Task<ServiceLookupsViewModel> GetLookupsAsync();
    }

    public class ServiceService : IServiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ServiceService> _logger;

        public ServiceService(ApplicationDbContext context, ILogger<ServiceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ServiceListViewModel>> GetServicesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Services
                    .Include(x => x.UoM)
                    .Include(x => x.ServiceNature)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.ServiceName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.ServiceName)
                    .Select(x => new ServiceListViewModel
                    {
                        ServiceId = x.ServiceId,
                        ServiceName = x.ServiceName,
                        UnitPrice = x.UnitPrice,
                        UoMName = x.UoM != null ? x.UoM.UoMName : "",
                        ServiceNatureName = x.ServiceNature != null ? x.ServiceNature.NatureName : "",
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
                _logger.LogError(ex, "Error retrieving services");
                throw;
            }
        }

        public async Task<ServiceViewModel> GetServiceByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Services
                    .Include(x => x.UoM)
                    .Include(x => x.ServiceNature)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.ServiceId == id);

                if (entity == null) return null;

                var viewModel = new ServiceViewModel
                {
                    ServiceId = entity.ServiceId,
                    ServiceName = entity.ServiceName,
                    UnitPrice = entity.UnitPrice,
                    UoMId = entity.UoMId,
                    ServiceNatureId = entity.ServiceNatureId,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };

                // Load dropdowns
                await LoadDropdownsAsync(viewModel);

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service with ID {ServiceId}", id);
                throw;
            }
        }

        public async Task<bool> SaveServiceAsync(ServiceViewModel model)
        {
            try
            {
                Service entity;

                if (model.ServiceId.HasValue && model.ServiceId > 0)
                {
                    // Update existing
                    entity = await _context.Services.FindAsync(model.ServiceId.Value);
                    if (entity == null) return false;

                    entity.ServiceName = model.ServiceName;
                    entity.UnitPrice = model.UnitPrice;
                    entity.UoMId = model.UoMId;
                    entity.ServiceNatureId = model.ServiceNatureId;
                    entity.IsActive = model.IsActive;
                    // UpdatedBy/UpdatedOn handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new Service
                    {
                        ServiceName = model.ServiceName,
                        UnitPrice = model.UnitPrice,
                        UoMId = model.UoMId,
                        ServiceNatureId = model.ServiceNatureId,
                        IsActive = model.IsActive
                        // CreatedBy/CreatedOn handled automatically by DbContext
                    };
                    _context.Services.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving service");
                throw;
            }
        }

        public async Task<bool> DeleteServiceAsync(short id)
        {
            try
            {
                var entity = await _context.Services.FindAsync(id);
                if (entity == null) return false;

                _context.Services.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service with ID {ServiceId}", id);
                throw;
            }
        }

        public async Task<bool> ServiceExistsAsync(string serviceName, short? excludeId = null)
        {
            try
            {
                var query = _context.Services.Where(x => x.ServiceName == serviceName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.ServiceId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if service exists");
                throw;
            }
        }

        public async Task<ServiceLookupsViewModel> GetLookupsAsync()
        {
            try
            {
                var lookups = new ServiceLookupsViewModel
                {

                    UoMs = await _context.UoMs
                        .Where(x => x.IsActive)
                        .OrderBy(x => x.UoMName)
                        .Select(x => new UoMListViewModel
                        {
                            UoMId = x.UoMId,
                            UoMName = x.UoMName,
                            IsActive = x.IsActive
                        })
                        .ToListAsync(),

                    ServiceNatures = await _context.ServiceNatures
                        .Where(x => x.IsActive)
                        .OrderBy(x => x.NatureName)
                        .Select(x => new ServiceNatureListViewModel
                        {
                            NatureId = x.NatureId,
                            NatureName = x.NatureName,
                            IsActive = x.IsActive
                        })
                        .ToListAsync()
                };

                return lookups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service lookups");
                throw;
            }
        }

        private async Task LoadDropdownsAsync(ServiceViewModel model)
        {
            var lookups = await GetLookupsAsync();

            model.UoMs = new SelectList(lookups.UoMs, "UoMId", "UoMName", model.UoMId);
            model.ServiceNatures = new SelectList(lookups.ServiceNatures, "NatureId", "NatureName", model.ServiceNatureId);
        }
    }
}