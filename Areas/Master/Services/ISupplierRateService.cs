using Microsoft.EntityFrameworkCore;
using ProcureToPay.Data;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;

namespace ProcureToPay.Areas.Master.Services
{
    public interface ISupplierRateService
    {
        Task<IEnumerable<SupplierRateListViewModel>> GetSupplierRatesAsync(bool activeOnly = true, string searchTerm = null);
        Task<SupplierRateViewModel> GetSupplierRateByIdAsync(int id);
        Task<SupplierRateViewModel> GetSupplierRateBySupplierIdAsync(short supplierId);
        Task<bool> SaveSupplierRateAsync(SupplierRateViewModel model);
        Task<bool> DeleteSupplierRateAsync(int id);
        Task<IEnumerable<SupplierLookupViewModel>> GetSuppliersForFuelAsync();
        Task<bool> SupplierRateExistsAsync(short supplierId, int? excludeId = null);
    }

    public class SupplierRateService : ISupplierRateService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SupplierRateService> _logger;

        public SupplierRateService(ApplicationDbContext context, ILogger<SupplierRateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SupplierRateListViewModel>> GetSupplierRatesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.SupplierRates
                    .Include(x => x.Supplier)
                    .Include(x => x.Details)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.Supplier.SupplierName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.Supplier.SupplierName)
                    .Select(x => new SupplierRateListViewModel
                    {
                        SupplierRateId = x.SupplierRateId,
                        SupplierId = x.SupplierId,
                        SupplierName = x.Supplier.SupplierName,
                        DetailCount = x.Details.Count(d => d.IsActive),
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
                _logger.LogError(ex, "Error retrieving supplier rates");
                throw;
            }
        }

        public async Task<SupplierRateViewModel> GetSupplierRateByIdAsync(int id)
        {
            try
            {
                var entity = await _context.SupplierRates
                    .Include(x => x.Supplier)
                    .Include(x => x.Details.Where(d => d.IsActive))
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.SupplierRateId == id);

                if (entity == null) return null;

                return MapToViewModel(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier rate with ID {SupplierRateId}", id);
                throw;
            }
        }

        public async Task<SupplierRateViewModel> GetSupplierRateBySupplierIdAsync(short supplierId)
        {
            try
            {
                var entity = await _context.SupplierRates
                    .Include(x => x.Supplier)
                    .Include(x => x.Details.Where(d => d.IsActive))
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.SupplierId == supplierId && x.IsActive);

                if (entity == null)
                {
                    // Return a new model with supplier info if no rates exist
                    var supplier = await _context.Suppliers.FindAsync(supplierId);
                    if (supplier != null)
                    {
                        return new SupplierRateViewModel
                        {
                            SupplierId = supplierId,
                            SupplierName = supplier.SupplierName,
                            IsActive = true,
                            Details = new List<SupplierRateDetailViewModel>()
                        };
                    }
                    return null;
                }

                return MapToViewModel(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier rate for supplier ID {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<bool> SaveSupplierRateAsync(SupplierRateViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                SupplierRate entity;

                if (model.SupplierRateId.HasValue && model.SupplierRateId > 0)
                {
                    // Update existing
                    entity = await _context.SupplierRates
                        .Include(x => x.Details)
                        .FirstOrDefaultAsync(x => x.SupplierRateId == model.SupplierRateId.Value);

                    if (entity == null) return false;

                    entity.IsActive = model.IsActive;
                }
                else
                {
                    // Create new
                    entity = new SupplierRate
                    {
                        SupplierId = model.SupplierId,
                        IsActive = model.IsActive
                    };
                    _context.SupplierRates.Add(entity);
                    await _context.SaveChangesAsync(); // Save to get the ID for details
                }

                // Handle details
                await SaveSupplierRateDetailsAsync(entity.SupplierRateId, model.Details);

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving supplier rate");
                throw;
            }
        }

        private async Task SaveSupplierRateDetailsAsync(int supplierRateId, List<SupplierRateDetailViewModel> details)
        {
            // Get existing details
            var existingDetails = await _context.SupplierRateDetails
                .Where(x => x.SupplierRateId == supplierRateId)
                .ToListAsync();

            foreach (var detail in details)
            {
                if (detail.IsDeleted && detail.DetailId.HasValue)
                {
                    // Delete existing detail
                    var existingDetail = existingDetails.FirstOrDefault(x => x.DetailId == detail.DetailId.Value);
                    if (existingDetail != null)
                    {
                        _context.SupplierRateDetails.Remove(existingDetail);
                    }
                }
                else if (detail.DetailId.HasValue && detail.DetailId > 0)
                {
                    // Update existing detail
                    var existingDetail = existingDetails.FirstOrDefault(x => x.DetailId == detail.DetailId.Value);
                    if (existingDetail != null)
                    {
                        existingDetail.FromDate = detail.FromDate;
                        existingDetail.ToDate = detail.ToDate;
                        existingDetail.FuelRate = detail.FuelRate;
                        existingDetail.IsActive = detail.IsActive;
                    }
                }
                else if (!detail.IsDeleted)
                {
                    // Add new detail
                    var newDetail = new SupplierRateDetail
                    {
                        SupplierRateId = supplierRateId,
                        FromDate = detail.FromDate,
                        ToDate = detail.ToDate,
                        FuelRate = detail.FuelRate,
                        IsActive = detail.IsActive
                    };
                    _context.SupplierRateDetails.Add(newDetail);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteSupplierRateAsync(int id)
        {
            try
            {
                var entity = await _context.SupplierRates
                    .Include(x => x.Details)
                    .FirstOrDefaultAsync(x => x.SupplierRateId == id);

                if (entity == null) return false;

                // Soft delete - set IsActive to false
                entity.IsActive = false;
                foreach (var detail in entity.Details)
                {
                    detail.IsActive = false;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier rate with ID {SupplierRateId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<SupplierLookupViewModel>> GetSuppliersForFuelAsync()
        {
            try
            {
                var suppliers = await _context.Suppliers
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SupplierName)
                    .Select(x => new SupplierLookupViewModel
                    {
                        SupplierId = x.SupplierId,
                        SupplierName = x.SupplierName
                    })
                    .ToListAsync();

                return suppliers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving suppliers for fuel");
                throw;
            }
        }

        public async Task<bool> SupplierRateExistsAsync(short supplierId, int? excludeId = null)
        {
            try
            {
                var query = _context.SupplierRates.Where(x => x.SupplierId == supplierId && x.IsActive);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.SupplierRateId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if supplier rate exists");
                throw;
            }
        }

        private SupplierRateViewModel MapToViewModel(SupplierRate entity)
        {
            return new SupplierRateViewModel
            {
                SupplierRateId = entity.SupplierRateId,
                SupplierId = entity.SupplierId,
                SupplierName = entity.Supplier?.SupplierName,
                IsActive = entity.IsActive,
                Details = entity.Details?.Select(d => new SupplierRateDetailViewModel
                {
                    DetailId = d.DetailId,
                    FromDate = d.FromDate,
                    ToDate = d.ToDate,
                    FuelRate = d.FuelRate,
                    IsActive = d.IsActive
                }).OrderBy(d => d.FromDate).ToList() ?? new List<SupplierRateDetailViewModel>(),
                CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                CreatedOn = entity.CreatedOn,
                UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                UpdatedOn = entity.UpdatedOn
            };
        }
    }
}