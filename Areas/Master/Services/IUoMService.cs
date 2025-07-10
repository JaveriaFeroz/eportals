using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IUoMService
    {
        Task<IEnumerable<UoMListViewModel>> GetUoMsAsync(bool activeOnly = true, string searchTerm = null);
        Task<UoMViewModel> GetUoMByIdAsync(short id);
        Task<bool> SaveUoMAsync(UoMViewModel model);
        Task<bool> DeleteUoMAsync(short id);
        Task<bool> UoMExistsAsync(string uomName, short? excludeId = null);
        Task<IEnumerable<UoMListViewModel>> GetActiveUoMsAsync(); // For dropdown lists
    }

    public class UoMService : IUoMService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UoMService> _logger;

        public UoMService(ApplicationDbContext context, ILogger<UoMService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<UoMListViewModel>> GetUoMsAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.UoMs
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.UoMName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.UoMName)
                    .Select(x => new UoMListViewModel
                    {
                        UoMId = x.UoMId,
                        UoMName = x.UoMName,
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
                _logger.LogError(ex, "Error retrieving UoMs");
                throw;
            }
        }

        public async Task<UoMViewModel> GetUoMByIdAsync(short id)
        {
            try
            {
                var entity = await _context.UoMs
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.UoMId == id);

                if (entity == null) return null;

                return new UoMViewModel
                {
                    UoMId = entity.UoMId,
                    UoMName = entity.UoMName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving UoM with ID {UoMId}", id);
                throw;
            }
        }

        public async Task<bool> SaveUoMAsync(UoMViewModel model)
        {
            try
            {
                UoM entity;

                if (model.UoMId.HasValue && model.UoMId > 0)
                {
                    // Update existing
                    entity = await _context.UoMs.FindAsync(model.UoMId.Value);
                    if (entity == null) return false;

                    entity.UoMName = model.UoMName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new UoM
                    {
                        UoMName = model.UoMName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.UoMs.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving UoM");
                throw;
            }
        }

        public async Task<bool> DeleteUoMAsync(short id)
        {
            try
            {
                var entity = await _context.UoMs.FindAsync(id);
                if (entity == null) return false;

                _context.UoMs.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting UoM with ID {UoMId}", id);
                throw;
            }
        }

        public async Task<bool> UoMExistsAsync(string uomName, short? excludeId = null)
        {
            try
            {
                var query = _context.UoMs.Where(x => x.UoMName == uomName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.UoMId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if UoM exists");
                throw;
            }
        }

        public async Task<IEnumerable<UoMListViewModel>> GetActiveUoMsAsync()
        {
            try
            {
                return await _context.UoMs
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.UoMName)
                    .Select(x => new UoMListViewModel
                    {
                        UoMId = x.UoMId,
                        UoMName = x.UoMName,
                        IsActive = x.IsActive
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active UoMs");
                throw;
            }
        }
    }
}