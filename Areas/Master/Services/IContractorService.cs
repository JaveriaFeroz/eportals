using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IContractorService
    {
        Task<IEnumerable<ContractorListViewModel>> GetContractorsAsync(bool activeOnly = true, string searchTerm = null);
        Task<ContractorViewModel> GetContractorByIdAsync(short id);
        Task<bool> SaveContractorAsync(ContractorViewModel model);
        Task<bool> DeleteContractorAsync(short id);
        Task<bool> ContractorExistsAsync(string contractorName, short? excludeId = null);
    }

    public class ContractorService : IContractorService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContractorService> _logger;

        public ContractorService(ApplicationDbContext context, ILogger<ContractorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ContractorListViewModel>> GetContractorsAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Contractors
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.ContractorName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.ContractorName)
                    .Select(x => new ContractorListViewModel
                    {
                        ContractorId = x.ContractorId,
                        ContractorName = x.ContractorName,
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
                _logger.LogError(ex, "Error retrieving contractors");
                throw;
            }
        }

        public async Task<ContractorViewModel> GetContractorByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Contractors
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.ContractorId == id);

                if (entity == null) return null;

                return new ContractorViewModel
                {
                    ContractorId = entity.ContractorId,
                    ContractorName = entity.ContractorName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contractor with ID {ContractorId}", id);
                throw;
            }
        }

        public async Task<bool> SaveContractorAsync(ContractorViewModel model)
        {
            try
            {
                Contractor entity;

                if (model.ContractorId.HasValue && model.ContractorId > 0)
                {
                    // Update existing
                    entity = await _context.Contractors.FindAsync(model.ContractorId.Value);
                    if (entity == null) return false;

                    entity.ContractorName = model.ContractorName;
                    entity.IsActive = model.IsActive;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new Contractor
                    {
                        ContractorName = model.ContractorName,
                        IsActive = model.IsActive
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.Contractors.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving contractor");
                throw;
            }
        }

        public async Task<bool> DeleteContractorAsync(short id)
        {
            try
            {
                var entity = await _context.Contractors.FindAsync(id);
                if (entity == null) return false;

                _context.Contractors.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contractor with ID {ContractorId}", id);
                throw;
            }
        }

        public async Task<bool> ContractorExistsAsync(string contractorName, short? excludeId = null)
        {
            try
            {
                var query = _context.Contractors.Where(x => x.ContractorName == contractorName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.ContractorId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if contractor exists");
                throw;
            }
        }
    }
}