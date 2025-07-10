using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IQualificationService
    {
        Task<IEnumerable<QualificationListViewModel>> GetQualificationsAsync(bool activeOnly = true, string searchTerm = null);
        Task<QualificationViewModel> GetQualificationByIdAsync(short id);
        Task<bool> SaveQualificationAsync(QualificationViewModel model);
        Task<bool> DeleteQualificationAsync(short id);
        Task<bool> QualificationExistsAsync(string qualificationName, short? excludeId = null);
    }
    public class QualificationService : IQualificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QualificationService> _logger;

        public QualificationService(ApplicationDbContext context, ILogger<QualificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<QualificationListViewModel>> GetQualificationsAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Qualifications
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.QualificationName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.QualificationName)
                    .Select(x => new QualificationListViewModel
                    {
                        QualificationId = x.QualificationId,
                        QualificationName = x.QualificationName,
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
                _logger.LogError(ex, "Error retrieving qualifications");
                throw;
            }
        }

        public async Task<QualificationViewModel> GetQualificationByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Qualifications
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.QualificationId == id);

                if (entity == null) return null;

                return new QualificationViewModel
                {
                    QualificationId = entity.QualificationId,
                    QualificationName = entity.QualificationName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving qualification with ID {QualificationId}", id);
                throw;
            }
        }

        public async Task<bool> SaveQualificationAsync(QualificationViewModel model)
        {
            try
            {
                Qualification entity;

                if (model.QualificationId.HasValue && model.QualificationId > 0)
                {
                    entity = await _context.Qualifications.FindAsync(model.QualificationId.Value);
                    if (entity == null) return false;

                    entity.QualificationName = model.QualificationName;
                    entity.IsActive = model.IsActive;
                }
                else
                {
                    entity = new Qualification
                    {
                        QualificationName = model.QualificationName,
                        IsActive = model.IsActive
                    };
                    _context.Qualifications.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving qualification");
                throw;
            }
        }

        public async Task<bool> DeleteQualificationAsync(short id)
        {
            try
            {
                var entity = await _context.Qualifications.FindAsync(id);
                if (entity == null) return false;

                _context.Qualifications.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting qualification with ID {QualificationId}", id);
                throw;
            }
        }

        public async Task<bool> QualificationExistsAsync(string qualificationName, short? excludeId = null)
        {
            try
            {
                var query = _context.Qualifications.Where(x => x.QualificationName == qualificationName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.QualificationId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if qualification exists");
                throw;
            }
        }
    }
}
