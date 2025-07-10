using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IIndustryVerticalService
    {
        Task<IEnumerable<IndustryVerticalListViewModel>> GetIndustryVerticalsAsync(bool activeOnly = true, string searchTerm = null);
        Task<IndustryVerticalViewModel> GetIndustryVerticalByIdAsync(short id);
        Task<bool> SaveIndustryVerticalAsync(IndustryVerticalViewModel model);
        Task<bool> DeleteIndustryVerticalAsync(short id);
        Task<bool> IndustryVerticalExistsAsync(string industryVerticalName, short? excludeId = null);
    }

    public class IndustryVerticalService : IIndustryVerticalService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndustryVerticalService> _logger;

        public IndustryVerticalService(ApplicationDbContext context, ILogger<IndustryVerticalService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<IndustryVerticalListViewModel>> GetIndustryVerticalsAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.IndustryVerticals
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.IndustryVerticalName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.IndustryVerticalName)
                    .Select(x => new IndustryVerticalListViewModel
                    {
                        IndustryVerticalId = x.IndustryVerticalId,
                        IndustryVerticalName = x.IndustryVerticalName,
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
                _logger.LogError(ex, "Error retrieving industryVerticals");
                throw;
            }
        }

        public async Task<IndustryVerticalViewModel> GetIndustryVerticalByIdAsync(short id)
        {
            try
            {
                var entity = await _context.IndustryVerticals
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.IndustryVerticalId == id);

                if (entity == null) return null;

                return new IndustryVerticalViewModel
                {
                    IndustryVerticalId = entity.IndustryVerticalId,
                    IndustryVerticalName = entity.IndustryVerticalName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving industryVertical with ID {IndustryVerticalId}", id);
                throw;
            }
        }

        public async Task<bool> SaveIndustryVerticalAsync(IndustryVerticalViewModel model)
        {
            try
            {
                IndustryVertical entity;

                if (model.IndustryVerticalId.HasValue && model.IndustryVerticalId > 0)
                {
                    // Update existing
                    entity = await _context.IndustryVerticals.FindAsync(model.IndustryVerticalId.Value);
                    if (entity == null) return false;

                    entity.IndustryVerticalName = model.IndustryVerticalName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new IndustryVertical
                    {
                        IndustryVerticalName = model.IndustryVerticalName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.IndustryVerticals.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving industryVertical");
                throw;
            }
        }

        public async Task<bool> DeleteIndustryVerticalAsync(short id)
        {
            try
            {
                var entity = await _context.IndustryVerticals.FindAsync(id);
                if (entity == null) return false;

                _context.IndustryVerticals.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting industryVertical with ID {IndustryVerticalId}", id);
                throw;
            }
        }

        public async Task<bool> IndustryVerticalExistsAsync(string industryVerticalName, short? excludeId = null)
        {
            try
            {
                var query = _context.IndustryVerticals.Where(x => x.IndustryVerticalName == industryVerticalName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.IndustryVerticalId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if industryVertical exists");
                throw;
            }
        }
    }
}