using Microsoft.EntityFrameworkCore;
using ProcureToPay.Data;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IRegionService
    {
        Task<IEnumerable<RegionListViewModel>> GetRegionsAsync(bool activeOnly = true, string searchTerm = null);
        Task<RegionViewModel> GetRegionByIdAsync(short id);
        Task<bool> SaveRegionAsync(RegionViewModel model);
        Task<bool> DeleteRegionAsync(short id);
        Task<bool> RegionExistsAsync(string regionName, short? excludeId = null);
        Task<bool> RegionHasCitiesAsync(short id);
        Task<IEnumerable<Region>> GetActiveRegionsAsync();
    }

    public class RegionService : IRegionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegionService> _logger;

        public RegionService(ApplicationDbContext context, ILogger<RegionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<RegionListViewModel>> GetRegionsAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Regions
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .Include(x => x.Cities) // Include cities to get count
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.RegionName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.RegionName)
                    .Select(x => new RegionListViewModel
                    {
                        RegionId = x.RegionId,
                        RegionName = x.RegionName,
                        TaxRate = x.TaxRate,
                        IsActive = x.IsActive,
                        CreatedBy = x.CreatedByUser != null ? $"{x.CreatedByUser.FirstName} {x.CreatedByUser.LastName}" : "System",
                        CreatedOn = x.CreatedOn,
                        UpdatedBy = x.UpdatedByUser != null ? $"{x.UpdatedByUser.FirstName} {x.UpdatedByUser.LastName}" : null,
                        UpdatedOn = x.UpdatedOn,
                        CitiesCount = x.Cities.Count(c => c.IsActive)
                    })
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving regions");
                throw;
            }
        }

        public async Task<RegionViewModel> GetRegionByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Regions
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.RegionId == id);

                if (entity == null) return null;

                return new RegionViewModel
                {
                    RegionId = entity.RegionId,
                    RegionName = entity.RegionName,
                    TaxRate = entity.TaxRate,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving region with ID {RegionId}", id);
                throw;
            }
        }

        public async Task<bool> SaveRegionAsync(RegionViewModel model)
        {
            try
            {
                Region entity;

                if (model.RegionId.HasValue && model.RegionId > 0)
                {
                    // Update existing
                    entity = await _context.Regions.FindAsync(model.RegionId.Value);
                    if (entity == null) return false;

                    entity.RegionName = model.RegionName;
                    entity.TaxRate = model.TaxRate;
                    entity.IsActive = model.IsActive;
                    // UpdatedBy/UpdatedOn handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new Region
                    {
                        RegionName = model.RegionName,
                        TaxRate = model.TaxRate,
                        IsActive = model.IsActive
                        // CreatedBy/CreatedOn handled automatically by DbContext
                    };
                    _context.Regions.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving region");
                throw;
            }
        }

        public async Task<bool> DeleteRegionAsync(short id)
        {
            try
            {
                var entity = await _context.Regions.FindAsync(id);
                if (entity == null) return false;

                // Check if region has associated cities
                if (await RegionHasCitiesAsync(id))
                {
                    throw new InvalidOperationException("Cannot delete region because it has associated cities.");
                }

                _context.Regions.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting region with ID {RegionId}", id);
                throw;
            }
        }

        public async Task<bool> RegionExistsAsync(string regionName, short? excludeId = null)
        {
            try
            {
                var query = _context.Regions.Where(x => x.RegionName == regionName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.RegionId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if region exists");
                throw;
            }
        }

        public async Task<bool> RegionHasCitiesAsync(short id)
        {
            try
            {
                return await _context.Cities.AnyAsync(c => c.RegionId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if region has cities");
                throw;
            }
        }

        public async Task<IEnumerable<Region>> GetActiveRegionsAsync()
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
                _logger.LogError(ex, "Error retrieving active regions");
                throw;
            }
        }
    }
}