using Microsoft.EntityFrameworkCore;
using ProcureToPay.Data;
using ProcureToPay.Areas.Master.ViewModels; // Keep this for CityListViewModel and CityViewModel
using ProcureToPay.Areas.Master.Models; // Keep this for the Model.City

namespace ProcureToPay.Areas.Master.Services
{
    public interface ICityService
    {
        Task<IEnumerable<CityListViewModel>> GetCitiesAsync(bool activeOnly = true, string searchTerm = null);
        Task<CityViewModel> GetCityByIdAsync(short id);
        Task<bool> SaveCityAsync(CityViewModel model);
        Task<bool> DeleteCityAsync(short id);
        Task<bool> CityExistsAsync(string cityCode, short? excludeId = null);
        Task<IEnumerable<Region>> GetRegionsAsync();
    }

    public class CityService : ICityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CityService> _logger;

        public CityService(ApplicationDbContext context, ILogger<CityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<CityListViewModel>> GetCitiesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Cities // This will correctly refer to ProcureToPay.Areas.Master.Models.City
                    .Include(x => x.Region)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.CityName.Contains(searchTerm) ||
                                           x.CityCode.Contains(searchTerm) ||
                                           x.Region.RegionName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.CityName)
                    .Select(x => new CityListViewModel
                    {
                        CityId = x.CityId,
                        CityName = x.CityName,
                        CityCode = x.CityCode,
                        RegionName = x.Region.RegionName,
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
                _logger.LogError(ex, "Error retrieving cities");
                throw;
            }
        }

        public async Task<CityViewModel> GetCityByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Cities // Correctly refers to ProcureToPay.Areas.Master.Models.City
                    .Include(x => x.Region)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.CityId == id);

                if (entity == null) return null;

                return new CityViewModel
                {
                    CityId = entity.CityId,
                    CityName = entity.CityName,
                    CityCode = entity.CityCode,
                    RegionId = entity.RegionId,
                    RegionName = entity.Region?.RegionName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving city with ID {CityId}", id);
                throw;
            }
        }

        public async Task<bool> SaveCityAsync(CityViewModel model)
        {
            try
            {
                ProcureToPay.Areas.Master.Models.City entity; // Explicitly specify the model City

                if (model.CityId.HasValue && model.CityId > 0)
                {
                    // Update existing
                    entity = await _context.Cities.FindAsync(model.CityId.Value);
                    if (entity == null) return false;

                    entity.CityName = model.CityName;
                    entity.CityCode = model.CityCode;
                    entity.RegionId = model.RegionId.Value;
                    entity.IsActive = model.IsActive;
                    // UpdatedBy/UpdatedOn handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new ProcureToPay.Areas.Master.Models.City // Explicitly specify the model City
                    {
                        CityName = model.CityName,
                        CityCode = model.CityCode,
                        RegionId = model.RegionId.Value,
                        IsActive = model.IsActive
                        // CreatedBy/CreatedOn handled automatically by DbContext
                    };
                    _context.Cities.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving city");
                throw;
            }
        }

        public async Task<bool> DeleteCityAsync(short id)
        {
            try
            {
                var entity = await _context.Cities.FindAsync(id);
                if (entity == null) return false;

                _context.Cities.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting city with ID {CityId}", id);
                throw;
            }
        }

        public async Task<bool> CityExistsAsync(string cityCode, short? excludeId = null)
        {
            try
            {
                var query = _context.Cities.Where(x => x.CityCode == cityCode);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.CityId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if city exists");
                throw;
            }
        }

        public async Task<IEnumerable<Region>> GetRegionsAsync()
        {
            try
            {
                return await _context.Regions
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.RegionName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving regions");
                throw;
            }
        }
    }
}