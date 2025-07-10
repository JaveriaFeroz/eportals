using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IMakeService
    {
        Task<IEnumerable<MakeListViewModel>> GetMakesAsync(bool activeOnly = true, string searchTerm = null);
        Task<MakeViewModel> GetMakeByIdAsync(short id);
        Task<bool> SaveMakeAsync(MakeViewModel model);
        Task<bool> DeleteMakeAsync(short id);
        Task<bool> MakeExistsAsync(string makeName, short? excludeId = null);
    }

    public class MakeService : IMakeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MakeService> _logger;

        public MakeService(ApplicationDbContext context, ILogger<MakeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<MakeListViewModel>> GetMakesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Makes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.MakeName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.MakeName)
                    .Select(x => new MakeListViewModel
                    {
                        MakeId = x.MakeId,
                        MakeName = x.MakeName,
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
                _logger.LogError(ex, "Error retrieving makes");
                throw;
            }
        }

        public async Task<MakeViewModel> GetMakeByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Makes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.MakeId == id);

                if (entity == null) return null;

                return new MakeViewModel
                {
                    MakeId = entity.MakeId,
                    MakeName = entity.MakeName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving make with ID {MakeId}", id);
                throw;
            }
        }

        public async Task<bool> SaveMakeAsync(MakeViewModel model)
        {
            try
            {
                Make entity;

                if (model.MakeId.HasValue && model.MakeId > 0)
                {
                    // Update existing
                    entity = await _context.Makes.FindAsync(model.MakeId.Value);
                    if (entity == null) return false;

                    entity.MakeName = model.MakeName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new Make
                    {
                        MakeName = model.MakeName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.Makes.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving make");
                throw;
            }
        }

        public async Task<bool> DeleteMakeAsync(short id)
        {
            try
            {
                var entity = await _context.Makes.FindAsync(id);
                if (entity == null) return false;

                _context.Makes.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting make with ID {MakeId}", id);
                throw;
            }
        }

        public async Task<bool> MakeExistsAsync(string makeName, short? excludeId = null)
        {
            try
            {
                var query = _context.Makes.Where(x => x.MakeName == makeName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.MakeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if make exists");
                throw;
            }
        }
    }
}