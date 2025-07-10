using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IInsuranceDocumentTypeService
    {
        Task<IEnumerable<InsuranceDocumentTypeListViewModel>> GetInsuranceDocumentTypesAsync(bool activeOnly = true, string searchTerm = null);
        Task<InsuranceDocumentTypeViewModel> GetInsuranceDocumentTypeByIdAsync(short id);
        Task<bool> SaveInsuranceDocumentTypeAsync(InsuranceDocumentTypeViewModel model);
        Task<bool> DeleteInsuranceDocumentTypeAsync(short id);
        Task<bool> InsuranceDocumentTypeExistsAsync(string insuranceDocumentTypeName, short? excludeId = null);
    }

    public class InsuranceDocumentTypeService : IInsuranceDocumentTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InsuranceDocumentTypeService> _logger;

        public InsuranceDocumentTypeService(ApplicationDbContext context, ILogger<InsuranceDocumentTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<InsuranceDocumentTypeListViewModel>> GetInsuranceDocumentTypesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.InsuranceDocumentTypes
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
                    .Select(x => new InsuranceDocumentTypeListViewModel
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
                _logger.LogError(ex, "Error retrieving insuranceDocumentTypes");
                throw;
            }
        }

        public async Task<InsuranceDocumentTypeViewModel> GetInsuranceDocumentTypeByIdAsync(short id)
        {
            try
            {
                var entity = await _context.InsuranceDocumentTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.TypeId == id);

                if (entity == null) return null;

                return new InsuranceDocumentTypeViewModel
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
                _logger.LogError(ex, "Error retrieving insuranceDocumentType with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> SaveInsuranceDocumentTypeAsync(InsuranceDocumentTypeViewModel model)
        {
            try
            {
                InsuranceDocumentType entity;

                if (model.TypeId.HasValue && model.TypeId > 0)
                {
                    // Update existing
                    entity = await _context.InsuranceDocumentTypes.FindAsync(model.TypeId.Value);
                    if (entity == null) return false;

                    entity.TypeName = model.TypeName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new InsuranceDocumentType
                    {
                        TypeName = model.TypeName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.InsuranceDocumentTypes.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving insuranceDocumentType");
                throw;
            }
        }

        public async Task<bool> DeleteInsuranceDocumentTypeAsync(short id)
        {
            try
            {
                var entity = await _context.InsuranceDocumentTypes.FindAsync(id);
                if (entity == null) return false;

                _context.InsuranceDocumentTypes.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting insuranceDocumentType with ID {TypeId}", id);
                throw;
            }
        }

        public async Task<bool> InsuranceDocumentTypeExistsAsync(string insuranceDocumentTypeName, short? excludeId = null)
        {
            try
            {
                var query = _context.InsuranceDocumentTypes.Where(x => x.TypeName == insuranceDocumentTypeName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.TypeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if insuranceDocumentType exists");
                throw;
            }
        }
    }
}