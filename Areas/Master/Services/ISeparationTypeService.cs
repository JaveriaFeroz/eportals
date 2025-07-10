using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface ISeparationTypeService
    {
        Task<IEnumerable<SeparationTypeListViewModel>> GetSeparationTypesAsync(bool activeOnly = true, string searchTerm = null);
        Task<SeparationTypeViewModel> GetSeparationTypeByIdAsync(short id);
        Task<bool> SaveSeparationTypeAsync(SeparationTypeViewModel model);
        Task<bool> DeleteSeparationTypeAsync(short id);
        Task<bool> SeparationTypeExistsAsync(string typeName, short? excludeId = null);
    }

    public class SeparationTypeService : ISeparationTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SeparationTypeService> _logger;

        public SeparationTypeService(ApplicationDbContext context, ILogger<SeparationTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SeparationTypeListViewModel>> GetSeparationTypesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.SeparationTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.TypeName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.TypeName)
                    .Select(x => new SeparationTypeListViewModel
                    {
                        TypeId = x.TypeId,
                        TypeName = x.TypeName,
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
                _logger.LogError(ex, "Error retrieving separationTypes");
                throw;
            }
        }

        public async Task<SeparationTypeViewModel> GetSeparationTypeByIdAsync(short id)
        {
            try
            {
                var entity = await _context.SeparationTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.TypeId == id);

                if (entity == null) return null;

                return new SeparationTypeViewModel
                {
                    TypeId = entity.TypeId,
                    TypeName = entity.TypeName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving separationType with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> SaveSeparationTypeAsync(SeparationTypeViewModel model)
        {
            try
            {
                SeparationType entity;

                if (model.TypeId.HasValue && model.TypeId > 0)
                {
                    // Update existing
                    entity = await _context.SeparationTypes.FindAsync(model.TypeId.Value);
                    if (entity == null) return false;

                    entity.TypeName = model.TypeName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new SeparationType
                    {
                        TypeName = model.TypeName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.SeparationTypes.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving separationType");
                throw;
            }
        }

        public async Task<bool> DeleteSeparationTypeAsync(short id)
        {
            try
            {
                var entity = await _context.SeparationTypes.FindAsync(id);
                if (entity == null) return false;

                _context.SeparationTypes.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting separationType with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> SeparationTypeExistsAsync(string typeName, short? excludeId = null)
        {
            try
            {
                var query = _context.SeparationTypes.Where(x => x.TypeName == typeName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.TypeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if separationType exists");
                throw;
            }
        }
    }
}