using Microsoft.EntityFrameworkCore;
using ProcureToPay.Data;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace ProcureToPay.Areas.Master.Services
{
    /// <summary>
    /// Defines the contract for consignee-related business logic.
    /// This interface is implemented by ConsigneeService and injected into ConsigneeController.
    /// </summary>
    public interface IConsigneeService
    {
        Task<IEnumerable<ConsigneeListViewModel>> GetConsigneesAsync(bool activeOnly = true, string searchTerm = null);
        Task<ConsigneeViewModel> GetConsigneeByIdAsync(int id);
        Task<bool> SaveConsigneeAsync(ConsigneeViewModel model);
        Task<bool> DeleteConsigneeAsync(int id);
        Task<bool> ConsigneeNameExistsAsync(string consigneeName, int? excludeId = null);
        // FIX: Renamed method to match the implementation's name for clarity.
        Task<ConsigneeFormLookupsViewModel> GetLookupsAsync();
    }

    /// <summary>
    /// Service to manage all data operations for the Consignee entity.
    /// </summary>
    public class ConsigneeService : IConsigneeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConsigneeService> _logger;

        public ConsigneeService(ApplicationDbContext context, ILogger<ConsigneeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a filtered and sorted list of consignees.
        /// </summary>
        public async Task<IEnumerable<ConsigneeListViewModel>> GetConsigneesAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Consignees
                    .Include(x => x.City)
                    .Include(x => x.Client)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var term = searchTerm.ToLower();
                    query = query.Where(x => x.ConsigneeName.ToLower().Contains(term) ||
                                             x.ContactNo.Contains(term) ||
                                             (x.Client != null && x.Client.ClientName.ToLower().Contains(term)));
                }

                // Project the database entities into the required ViewModel for the list.
                return await query
                    .OrderBy(x => x.ConsigneeName)
                    .Select(x => new ConsigneeListViewModel
                    {
                        ConsigneeId = x.ConsigneeId,
                        ConsigneeName = x.ConsigneeName,
                        CityName = x.City != null ? x.City.CityName : "N/A",
                        ClientName = x.Client != null ? x.Client.ClientName : "N/A",
                        ContactNo = x.ContactNo,
                        Address = x.Address,
                        IsActive = x.IsActive,
                        CreatedBy = x.CreatedByUser != null ? $"{x.CreatedByUser.FirstName} {x.CreatedByUser.LastName}" : "System",
                        CreatedOn = x.CreatedOn,
                        UpdatedBy = x.UpdatedByUser != null ? $"{x.UpdatedByUser.FirstName} {x.UpdatedByUser.LastName}" : null,
                        UpdatedOn = x.UpdatedOn
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consignees list.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a single consignee by its primary key for display or editing.
        /// </summary>
        public async Task<ConsigneeViewModel> GetConsigneeByIdAsync(int id)
        {
            try
            {
                // FIX: Correctly queries by ConsigneeId and includes Company.
                // Projects the data into the structure of your latest ViewModel.
                return await _context.Consignees
                    .AsNoTracking()
                    .Include(x => x.City)
                    .Include(x => x.Client)
                    .Include(x => x.Company)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .Where(x => x.ConsigneeId == id)
                    .Select(entity => new ConsigneeViewModel
                    {
                        ConsigneeId = entity.ConsigneeId,
                        ConsigneeName = entity.ConsigneeName,
                        CityId = entity.CityId,
                        ClientId = entity.ClientId,
                        CompanyId = entity.CompanyId,
                        ContactNo = entity.ContactNo,
                        Address = entity.Address,
                        IsActive = entity.IsActive,
                        LastDeliveryKMs = entity.LastDeliveryKMs,
                        LastDepartureDate = entity.LastDepartureDate,
                        LastDepartureDateTime = entity.LastDepartureDateTime,
                        StandardKMs = entity.StandardKMs,
                        CityName = entity.City != null ? entity.City.CityName : null,
                        ClientName = entity.Client != null ? entity.Client.ClientName : null,
                        CompanyName = entity.Company != null ? entity.Company.CompanyName : null,
                        CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                        CreatedOn = entity.CreatedOn,
                        UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                        UpdatedOn = entity.UpdatedOn
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consignee with ID {ConsigneeId}", id);
                throw;
            }
        }

        /// <summary>
        /// Creates a new consignee or updates an existing one based on the provided ViewModel.
        /// </summary>
        public async Task<bool> SaveConsigneeAsync(ConsigneeViewModel model)
        {
            try
            {
                Consignee entity;

                if (model.ConsigneeId.HasValue && model.ConsigneeId > 0)
                {
                    // Update existing
                    entity = await _context.Consignees.FindAsync(model.ConsigneeId.Value);
                    if (entity == null) return false;

                    // Map properties for update
                    entity.ConsigneeName = model.ConsigneeName;
                    entity.CompanyId = model.CompanyId;
                    entity.CityId = model.CityId;
                    entity.ClientId = model.ClientId;
                    entity.ContactNo = model.ContactNo;
                    entity.Address = model.Address;
                    entity.IsActive = model.IsActive;
                    entity.LastDeliveryKMs = model.LastDeliveryKMs;
                    entity.LastDepartureDate = model.LastDepartureDate;
                    entity.LastDepartureDateTime = model.LastDepartureDateTime;
                    entity.StandardKMs = model.StandardKMs;
                    // Note: Audit fields are not updated here as per the example's pattern
                }
                else
                {
                    // Create new
                    entity = new Consignee
                    {
                        ConsigneeName = model.ConsigneeName,
                        CompanyId = model.CompanyId,
                        CityId = model.CityId,
                        ClientId = model.ClientId,
                        ContactNo = model.ContactNo,
                        Address = model.Address,
                        IsActive = model.IsActive,
                        LastDeliveryKMs = model.LastDeliveryKMs,
                        LastDepartureDate = model.LastDepartureDate,
                        LastDepartureDateTime = model.LastDepartureDateTime,
                        StandardKMs = model.StandardKMs
                        // Note: Audit fields are not set here as per the example's pattern
                    };
                    _context.Consignees.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving consignee data for ConsigneeName: {ConsigneeName}", model.ConsigneeName);
                throw; // Re-throw the exception as per the provided example pattern.
            }
        }
        /// <summary>
        /// Deletes a consignee by its ID.
        /// </summary>
        public async Task<bool> DeleteConsigneeAsync(int id)
        {
            try
            {
                var entity = await _context.Consignees.FindAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("Delete failed. Consignee with ID {ConsigneeId} not found.", id);
                    return false;
                }

                _context.Consignees.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException dbEx) // Catch foreign key violations
            {
                _logger.LogError(dbEx, "DB error deleting consignee with ID {ConsigneeId}. It is likely in use.", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error deleting consignee with ID {ConsigneeId}", id);
                throw;
            }
        }

        /// <summary>
        /// Checks if a consignee name already exists, excluding a specific ID (for updates).
        /// </summary>
        public async Task<bool> ConsigneeNameExistsAsync(string consigneeName, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(consigneeName))
            {
                return false;
            }
            var query = _context.Consignees.Where(x => x.ConsigneeName.ToLower() == consigneeName.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.ConsigneeId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Retrieves all lookup data (for dropdown lists) needed for the create/edit form.
        /// </summary>
        public async Task<ConsigneeFormLookupsViewModel> GetLookupsAsync()
        {
            try
            {
                var lookups = new ConsigneeFormLookupsViewModel();

                lookups.Cities = await _context.Cities.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.CityName)
                    .Select(x => new ViewModels.CityLookupViewModel { CityId = x.CityId, CityName = x.CityName }).ToListAsync();

                lookups.Clients = await _context.Clients.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.ClientName)
                    .Select(x => new ClientLookupViewModel { ClientId = x.ClientId, ClientName = x.ClientName }).ToListAsync();

                lookups.Companies = await _context.Companies.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.CompanyName)
                    .Select(x => new ViewModels.CompanyLookupViewModel { CompanyId = x.CompanyId, CompanyName = x.CompanyName }).ToListAsync();

                return lookups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving form lookups for Consignee module.");
                throw;
            }
        }
    }
}
