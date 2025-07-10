using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IRateTypeService
    {
        Task<IEnumerable<RateTypeListViewModel>> GetRateTypesAsync(bool activeOnly = true, string searchTerm = null);
        Task<RateTypeViewModel> GetRateTypeByIdAsync(short id);
        Task<bool> SaveRateTypeAsync(RateTypeViewModel model);
        Task<bool> DeleteRateTypeAsync(short id);
        Task<bool> RateTypeExistsAsync(string typeName, short? excludeId = null);
    }

    public class RateTypeService : IRateTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RateTypeService> _logger;

        public RateTypeService(ApplicationDbContext context, ILogger<RateTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<RateTypeListViewModel>> GetRateTypesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.RateTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.RateTypeName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.RateTypeName)
                    .Select(x => new RateTypeListViewModel
                    {
                        TypeId = x.RateTypeId,
                        TypeName = x.RateTypeName,
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
                _logger.LogError(ex, "Error retrieving rate types");
                throw;
            }
        }

        public async Task<RateTypeViewModel> GetRateTypeByIdAsync(short id)
        {
            try
            {
                var entity = await _context.RateTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.RateTypeId == id);

                if (entity == null) return null;

                return new RateTypeViewModel
                {
                    TypeId = entity.RateTypeId,
                    TypeName = entity.RateTypeName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rate type with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> SaveRateTypeAsync(RateTypeViewModel model)
        {
            try
            {
                RateType entity;

                if (model.TypeId.HasValue && model.TypeId > 0)
                {
                    entity = await _context.RateTypes.FindAsync(model.TypeId.Value);
                    if (entity == null) return false;

                    entity.RateTypeName = model.TypeName;
                    entity.IsActive = model.IsActive;
                }
                else
                {
                    entity = new RateType
                    {
                        RateTypeName = model.TypeName,
                        IsActive = model.IsActive
                    };
                    _context.RateTypes.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving rate type");
                throw;
            }
        }

        public async Task<bool> DeleteRateTypeAsync(short id)
        {
            try
            {
                var entity = await _context.RateTypes.FindAsync(id);
                if (entity == null) return false;

                _context.RateTypes.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rate type with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> RateTypeExistsAsync(string typeName, short? excludeId = null)
        {
            try
            {
                var query = _context.RateTypes.Where(x => x.RateTypeName == typeName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.RateTypeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if rate type exists");
                throw;
            }
        }
    }
}