using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IRelationService
    {
        Task<IEnumerable<RelationListViewModel>> GetRelationsAsync(bool activeOnly = true, string searchTerm = null);
        Task<RelationViewModel> GetRelationByIdAsync(short id);
        Task<bool> SaveRelationAsync(RelationViewModel model);
        Task<bool> DeleteRelationAsync(short id);
        Task<bool> RelationExistsAsync(string relationName, short? excludeId = null);
    }

    public class RelationService : IRelationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RelationService> _logger;

        public RelationService(ApplicationDbContext context, ILogger<RelationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<RelationListViewModel>> GetRelationsAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Relations
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.RelationName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.RelationName)
                    .Select(x => new RelationListViewModel
                    {
                        RelationId = x.RelationId,
                        RelationName = x.RelationName,
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
                _logger.LogError(ex, "Error retrieving relations");
                throw;
            }
        }

        public async Task<RelationViewModel> GetRelationByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Relations
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.RelationId == id);

                if (entity == null) return null;

                return new RelationViewModel
                {
                    RelationId = entity.RelationId,
                    RelationName = entity.RelationName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving relation with ID {RelationId}", id);
                throw;
            }
        }

        public async Task<bool> SaveRelationAsync(RelationViewModel model)
        {
            try
            {
                Relation entity;

                if (model.RelationId.HasValue && model.RelationId > 0)
                {
                    // Update existing
                    entity = await _context.Relations.FindAsync(model.RelationId.Value);
                    if (entity == null) return false;

                    entity.RelationName = model.RelationName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new Relation
                    {
                        RelationName = model.RelationName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.Relations.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving relation");
                throw;
            }
        }

        public async Task<bool> DeleteRelationAsync(short id)
        {
            try
            {
                var entity = await _context.Relations.FindAsync(id);
                if (entity == null) return false;

                _context.Relations.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting relation with ID {RelationId}", id);
                throw;
            }
        }

        public async Task<bool> RelationExistsAsync(string relationName, short? excludeId = null)
        {
            try
            {
                var query = _context.Relations.Where(x => x.RelationName == relationName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.RelationId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if relation exists");
                throw;
            }
        }
    }
}