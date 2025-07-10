using Microsoft.EntityFrameworkCore;
using ProcureToPay.Data;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IShipperService
    {
        Task<IEnumerable<ShipperListViewModel>> GetShippersAsync(bool activeOnly = true, string searchTerm = null);
        Task<ShipperViewModel> GetShipperByIdAsync(short id);
        Task<bool> SaveShipperAsync(ShipperViewModel model);
        Task<bool> DeleteShipperAsync(int id);
        Task<bool> ShipperNameExistsAsync(string shipperName, short? excludeId = null);
        Task<ShipperLookupsViewModel> GetLookupsAsync();
    }

    public class ShipperService : IShipperService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShipperService> _logger;

        public ShipperService(ApplicationDbContext context, ILogger<ShipperService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ShipperListViewModel>> GetShippersAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Shippers
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.ShipperName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.ShipperName)
                    .Select(x => new ShipperListViewModel
                    {
                        ShipperId = x.ShipperId,
                        ShipperName = x.ShipperName,
                        Address = x.Address,
                        ContactNo = x.ContactNo,
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
                // FIX: Corrected log message.
                _logger.LogError(ex, "Error retrieving Shippers");
                throw;
            }
        }

        public async Task<ShipperViewModel> GetShipperByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Shippers
                    .Include(x => x.City)
                    .Include(x => x.Client)
                    .Include(x => x.Company)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.ShipperId == id);

                if (entity == null) return null;

                return new ShipperViewModel
                {
                    ShipperId = entity.ShipperId,
                    ShipperName = entity.ShipperName,
                    Address = entity.Address,
                    ContactNo = entity.ContactNo,
                    CompanyId = entity.CompanyId,
                    ClientId = entity.ClientId,
                    CityId = entity.CityId,
                    CityName = entity.City?.CityName,
                    ClientName = entity.Client?.ClientName,
                    CompanyName = entity.Company?.CompanyName,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shipper with ID {ShipperId}", id);
                throw;
            }
        }

        public async Task<bool> SaveShipperAsync(ShipperViewModel model)
        {
            try
            {
                Shipper entity;

                if (model.ShipperId.HasValue && model.ShipperId > 0)
                {
                    // Update existing
                    entity = await _context.Shippers.FindAsync(model.ShipperId.Value);
                    if (entity == null) return false;

                    entity.ShipperName = model.ShipperName;
                    entity.Address = model.Address;
                    entity.ContactNo = model.ContactNo;
                    entity.CompanyId = model.CompanyId;
                    entity.ClientId = model.ClientId;
                    entity.CityId = model.CityId;
                    entity.IsActive = model.IsActive;
                }
                else
                {
                    // Create new
                    entity = new Shipper
                    {
                        ShipperName = model.ShipperName,
                        Address = model.Address,
                        ContactNo = model.ContactNo,
                        CompanyId = model.CompanyId,
                        ClientId = model.ClientId,
                        CityId = model.CityId,
                        IsActive = model.IsActive
                    };
                    _context.Shippers.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving shipper");
                throw;
            }
        }

        public async Task<bool> DeleteShipperAsync(int id)
        {
            try
            {
                var entity = await _context.Shippers.FindAsync(id);
                if (entity == null) return false;

                _context.Shippers.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting shipper with ID {ShipperId}", id);
                throw;
            }
        }

        public async Task<bool> ShipperNameExistsAsync(string shipperName, short? excludeId = null)
        {
            try
            {
                var query = _context.Shippers.Where(x => x.ShipperName == shipperName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.ShipperId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if shipper name exists");
                throw;
            }
        }

        public async Task<ShipperLookupsViewModel> GetLookupsAsync()
        {
            try
            {
                var lookups = new ShipperLookupsViewModel();

                lookups.Clients = await _context.Clients
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.ClientName)
                   .Select(x => new ViewModels.ClientLookupViewModel
                   {
                       ClientId = x.ClientId,
                       ClientName = x.ClientName
                   })
                   .ToListAsync();


                lookups.Cities = await _context.Cities
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.CityName)
                   .Select(x => new ViewModels.CitiesLookupViewModel
                   {
                       CityId = x.CityId,
                       CityName = x.CityName
                   })
                   .ToListAsync();
                lookups.Companies = await _context.Companies
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.CompanyName)
                   .Select(x => new ViewModels.CompanyLookupViewModel
                   {
                       CompanyId = x.CompanyId,
                       CompanyName = x.CompanyName
                   })
                   .ToListAsync();

                // FIX: Added logic to fetch Shipper Types for the dropdown.
                lookups.Shippers = await _context.Shippers
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.ShipperName)
                   .Select(x => new ShipperTypeLookupViewModel
                   {
                       ShipperId = x.ShipperId,
                       ShipperName = x.ShipperName
                   })
                   .ToListAsync();

                return lookups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lookups");
                throw;
            }
        }
    }
}