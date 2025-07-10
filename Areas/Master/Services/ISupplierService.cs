using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierListViewModel>> GetSuppliersAsync(bool activeOnly = true, string? searchTerm = null);
        Task<IEnumerable<SupplierListViewModel>> GetFuelSuppliersAsync(bool activeOnly = true);
        Task<IEnumerable<SupplierListViewModel>> GetOutSourcedVehicleSuppliersAsync(bool activeOnly = true);
        Task<SupplierViewModel?> GetSupplierByIdAsync(short id);
        Task<bool> SaveSupplierAsync(SupplierViewModel model);
        Task<bool> DeleteSupplierAsync(short id);
        Task<bool> SupplierExistsAsync(string supplierName, short? excludeId = null);


        // --- CORRECTED AND NEW VALIDATION METHODS ---
        Task<bool> SupplierEmailExistsAsync(string email, short? excludeId = null);
        Task<bool> SupplierPhoneExistsAsync(string phoneNo, short? excludeId = null);
        Task<bool> SupplierControlIdExistsAsync(string controlSupplierId, short? excludeId = null);
        Task<SupplierLookupsViewModel> GetLookupsAsync();
    }

    public class SupplierService : ISupplierService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(ApplicationDbContext context, ILogger<SupplierService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SupplierListViewModel>> GetSuppliersAsync(bool activeOnly = true, string? searchTerm = null)
        {
            try
            {
                var query = _context.Suppliers
                   .Include(x => x.SupplierType)
                   .Include(x => x.City)
                   .Include(x => x.CreatedByUser)
                   .Include(x => x.UpdatedByUser)
                   .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.SupplierName.Contains(searchTerm) ||
                                           x.ContactName.Contains(searchTerm) ||
                                           x.Email.Contains(searchTerm));
                }
                var result = await query
    .OrderBy(x => x.SupplierName)
    .Select(x => new SupplierListViewModel
    {
        SupplierId = x.SupplierId,
        SupplierName = x.SupplierName,
        SupplierTypeName = x.SupplierType != null ? x.SupplierType.SupplierTypeName : "",
        CityName = x.City != null ? x.City.CityName : null,
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
                _logger.LogError(ex, "Error retrieving suppliers");
                throw;
            }
        }


        public async Task<IEnumerable<SupplierListViewModel>> GetFuelSuppliersAsync(bool activeOnly = true)
        {
            try
            {
                var query = _context.Suppliers
                    .Include(x => x.SupplierType)
                    .Where(x => x.SupplierType.SupplierTypeName.Contains("Fuel")) // Adjust based on your business logic
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                var result = await query
                    .OrderBy(x => x.SupplierName)
                    .Select(x => new SupplierListViewModel
                    {
                        SupplierId = x.SupplierId,
                        SupplierName = x.SupplierName,
                        SupplierTypeName = x.SupplierType.SupplierTypeName,
                        IsActive = x.IsActive
                    })
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fuel suppliers");
                throw;
            }
        }

        public async Task<IEnumerable<SupplierListViewModel>> GetOutSourcedVehicleSuppliersAsync(bool activeOnly = true)
        {
            try
            {
                var query = _context.Suppliers
                    .Include(x => x.SupplierType)
                    .Where(x => x.SupplierType.SupplierTypeName.Contains("Vehicle") ||
                               x.SupplierType.SupplierTypeName.Contains("Transport")) // Adjust based on your business logic
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                var result = await query
                    .OrderBy(x => x.SupplierName)
                    .Select(x => new SupplierListViewModel
                    {
                        SupplierId = x.SupplierId,
                        SupplierName = x.SupplierName,
                        SupplierTypeName = x.SupplierType.SupplierTypeName,
                        IsActive = x.IsActive
                    })
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving outsourced vehicle suppliers");
                throw;
            }
        }

        public async Task<SupplierViewModel?> GetSupplierByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Suppliers
                    .Include(x => x.SupplierType)
                    .Include(x => x.City)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.SupplierId == id);

                if (entity == null) return null;

                return new SupplierViewModel
                {
                    SupplierId = entity.SupplierId,
                    SupplierTypeId = entity.SupplierTypeId,
                    SupplierName = entity.SupplierName,
                    SCRate = entity.SCRate,
                    Address = entity.Address,
                    CityId = entity.CityId,
                    Email = entity.Email,
                    PhoneNo = entity.PhoneNo,
                    FaxNo = entity.FaxNo,
                    ContactName = entity.ContactName,
                    MobileNo = entity.MobileNo,
                    NTN = entity.NTN,
                    URL = entity.URL,
                    ControlSupplierId = entity.ControlSupplierId,
                    IsActive = entity.IsActive,
                    SupplierTypeName = entity.SupplierType?.SupplierTypeName,
                    CityName = entity.City?.CityName,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier with ID {SupplierId}", id);
                throw;
            }
        }

        public async Task<bool> SaveSupplierAsync(SupplierViewModel model)
        {
            try
            {
                Supplier entity;

                if (model.SupplierId.HasValue && model.SupplierId > 0)
                {
                    // Update existing
                    entity = await _context.Suppliers.FindAsync(model.SupplierId.Value);
                    if (entity == null) return false;

                    entity.SupplierTypeId = model.SupplierTypeId;
                    entity.SupplierName = model.SupplierName;
                    entity.SCRate = model.SCRate;
                    entity.Address = model.Address;
                    entity.CityId = model.CityId;
                    entity.Email = model.Email;
                    entity.PhoneNo = model.PhoneNo;
                    entity.FaxNo = model.FaxNo;
                    entity.ContactName = model.ContactName;
                    entity.MobileNo = model.MobileNo;
                    entity.NTN = model.NTN;
                    entity.URL = model.URL;
                    entity.ControlSupplierId = model.ControlSupplierId;
                    entity.IsActive = model.IsActive;
                }
                else
                {
                    // Create new
                    entity = new Supplier
                    {
                        SupplierTypeId = model.SupplierTypeId,
                        SupplierName = model.SupplierName,
                        SCRate = model.SCRate,
                        Address = model.Address,
                        CityId = model.CityId,
                        Email = model.Email,
                        PhoneNo = model.PhoneNo,
                        FaxNo = model.FaxNo,
                        ContactName = model.ContactName,
                        MobileNo = model.MobileNo,
                        NTN = model.NTN,
                        URL = model.URL,
                        ControlSupplierId = model.ControlSupplierId,
                        IsActive = model.IsActive
                    };
                    _context.Suppliers.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving supplier");
                throw;
            }
        }

        public async Task<bool> DeleteSupplierAsync(short id)
        {
            try
            {
                var entity = await _context.Suppliers.FindAsync(id);
                if (entity == null) return false;

                _context.Suppliers.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier with ID {SupplierId}", id);
                throw;
            }
        }

        public async Task<bool> SupplierExistsAsync(string supplierName, short? excludeId = null)
        {
            try
            {
                var query = _context.Suppliers.Where(x => x.SupplierName == supplierName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.SupplierId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if supplier exists");
                throw;
            }
        }


        public async Task<bool> SupplierEmailExistsAsync(string email, short? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var normalizedEmail = email.ToLower();
            var query = _context.Suppliers.Where(x => x.Email.ToLower() == normalizedEmail);
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.SupplierId != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<bool> SupplierPhoneExistsAsync(string phoneNo, short? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(phoneNo)) return false;
            var query = _context.Suppliers.Where(x => x.PhoneNo == phoneNo);
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.SupplierId != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<bool> SupplierControlIdExistsAsync(string controlSupplierId, short? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(controlSupplierId)) return false;
            var normalizedId = controlSupplierId.ToLower();
            var query = _context.Suppliers.Where(x => x.ControlSupplierId.ToLower() == normalizedId);
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.SupplierId != excludeId.Value);
            }
            return await query.AnyAsync();
        }


        public async Task<SupplierLookupsViewModel> GetLookupsAsync()
        {
            try
            {
                var supplierTypes = await _context.SupplierTypes
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SupplierTypeName)
                    .Select(x => new SupplierTypeViewModel
                    {
                        TypeId = x.TypeId,
                        SupplierTypeName = x.SupplierTypeName
                    })
                    .ToListAsync();

                var cities = await _context.Cities
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.CityName)
                    .Select(x => new CityViewModel
                    {
                        CityId = x.CityId,
                        CityName = x.CityName
                    })
                    .ToListAsync();

                return new SupplierLookupsViewModel
                {
                    SupplierTypes = supplierTypes,
                    Cities = cities
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lookups");
                throw;
            }
        }
    }
}

