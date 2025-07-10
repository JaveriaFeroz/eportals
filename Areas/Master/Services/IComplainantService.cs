using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IComplainantService
    {
        Task<IEnumerable<ComplainantListViewModel>> GetComplainantsAsync(bool activeOnly = true, string searchTerm = null);
        Task<ComplainantViewModel> GetComplainantByIdAsync(short id);
        Task<bool> SaveComplainantAsync(ComplainantViewModel model);
        Task<bool> DeleteComplainantAsync(short id);
        Task<bool> ComplainantExistsAsync(string complainantName, short? excludeId = null);
    }

    public class ComplainantService : IComplainantService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ComplainantService> _logger;

        public ComplainantService(ApplicationDbContext context, ILogger<ComplainantService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ComplainantListViewModel>> GetComplainantsAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Complainants
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.ComplainantName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.ComplainantName)
                    .Select(x => new ComplainantListViewModel
                    {
                        ComplainantId = x.ComplainantId,
                        ComplainantName = x.ComplainantName,
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
                _logger.LogError(ex, "Error retrieving complainants");
                throw;
            }
        }

        public async Task<ComplainantViewModel> GetComplainantByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Complainants
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.ComplainantId == id);

                if (entity == null) return null;

                return new ComplainantViewModel
                {
                    ComplainantId = entity.ComplainantId,
                    ComplainantName = entity.ComplainantName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving complainant with ID {ComplainantId}", id);
                throw;
            }
        }

        public async Task<bool> SaveComplainantAsync(ComplainantViewModel model)
        {
            try
            {
                Complainant entity;

                if (model.ComplainantId.HasValue && model.ComplainantId > 0)
                {
                    // Update existing
                    entity = await _context.Complainants.FindAsync(model.ComplainantId.Value);
                    if (entity == null) return false;

                    entity.ComplainantName = model.ComplainantName;
                    entity.IsActive = model.IsActive;
                    // UpdatedBy/UpdatedOn handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new Complainant
                    {
                        ComplainantName = model.ComplainantName,
                        IsActive = model.IsActive
                        // CreatedBy/CreatedOn handled automatically by DbContext
                    };
                    _context.Complainants.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving complainant");
                throw;
            }
        }

        public async Task<bool> DeleteComplainantAsync(short id)
        {
            try
            {
                var entity = await _context.Complainants.FindAsync(id);
                if (entity == null) return false;

                _context.Complainants.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting complainant with ID {ComplainantId}", id);
                throw;
            }
        }

        public async Task<bool> ComplainantExistsAsync(string complainantName, short? excludeId = null)
        {
            try
            {
                var query = _context.Complainants.Where(x => x.ComplainantName == complainantName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.ComplainantId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if complainant exists");
                throw;
            }
        }
    }
}