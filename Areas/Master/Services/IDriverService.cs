using Microsoft.EntityFrameworkCore;
using ProcureToPay.Data;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IDriverService
    {
        Task<IEnumerable<DriverListViewModel>> GetDriversAsync(bool activeOnly = true, string searchTerm = null);
        Task<DriverViewModel> GetDriverByIdAsync(short id);
        Task<bool> SaveDriverAsync(DriverViewModel model);
        Task<bool> DeleteDriverAsync(short id);
        Task<bool> DriverNameExistsAsync(string driverName, short? excludeId = null);
        Task<DriverLookupsViewModel> GetLookupsAsync();
        //  Task<IEnumerable<WorkflowDriverViewModel>> GetPendingWorkflowFormsAsync();
    }

    public class DriverService : IDriverService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DriverService> _logger;

        public DriverService(ApplicationDbContext context, ILogger<DriverService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<DriverListViewModel>> GetDriversAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Drivers
                    .Include(x => x.Contractor)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.DriverName.Contains(searchTerm) ||
                                           x.EmployeeNo.Contains(searchTerm) ||
                                           x.Designation.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.DriverName)
                    .Select(x => new DriverListViewModel
                    {
                        DriverId = x.DriverId,
                        DriverName = x.DriverName,
                        Designation = x.Designation,
                        ContractorName = x.Contractor != null ? x.Contractor.ContractorName : "",
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
                _logger.LogError(ex, "Error retrieving drivers");
                throw;
            }
        }

        public async Task<DriverViewModel> GetDriverByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Drivers
                    .Include(x => x.Company)
                    .Include(x => x.Contractor)
                    .Include(x => x.Qualification)
                    .Include(x => x.SeparationType)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.DriverId == id);

                if (entity == null) return null;

                return new DriverViewModel
                {
                    DriverId = entity.DriverId,
                    CompanyId = entity.CompanyId,
                    DriverName = entity.DriverName,
                    FatherName = entity.FatherName,
                    BranchId = entity.BranchId,
                    Designation = entity.Designation,
                    EmployeeNo = entity.EmployeeNo,
                    JoiningDate = entity.JoiningDate,
                    ContractorId = entity.ContractorId,
                    MonthlySalary = entity.MonthlySalary,
                    Address = entity.Address,
                    BirthDate = entity.BirthDate,
                    Experience = entity.Experience,
                    CellNo = entity.CellNo,
                    LicenseNo = entity.LicenseNo,
                    LicenseExpiry = entity.LicenseExpiry,
                    CNIC = entity.CNIC,
                    CNICExpiry = entity.CNICExpiry,
                    QualificationId = entity.QualificationId,
                    NoKName = entity.NoKName,
                    PreviousEmployer = entity.PreviousEmployer,
                    SeparationTypeId = entity.SeparationTypeId,
                    SeparationDate = entity.SeparationDate,
                    SeparationReason = entity.SeparationReason,
                    IsActive = entity.IsActive,
                   
                    CompanyName = entity.Company?.CompanyName,
                    ContractorName = entity.Contractor?.ContractorName,
                    QualificationName = entity.Qualification?.QualificationName,
                    SeparationTypeName = entity.SeparationType?.TypeName,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn,
                    
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver with ID {DriverId}", id);
                throw;
            }
        }

        public async Task<bool> SaveDriverAsync(DriverViewModel model)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Driver entity;

                // CORRECTED: A non-nullable 'short' (model.DriverId) cannot have .HasValue or .Value.
                // A simple check against 0 is sufficient to determine if it's a new entity.
                if (model.DriverId > 0)
                {
                    // --- UPDATE LOGIC ---
                    // Find the existing entity.
                    entity = await _context.Drivers.FindAsync(model.DriverId);
                    if (entity == null)
                    {
                        _logger.LogWarning("Attempted to update a non-existent driver with ID {DriverId}", model.DriverId);
                        return false;
                    }
                }
                else
                {
                    // --- CREATE LOGIC ---
                    // Create a new entity instance and add it to the context.
                    entity = new Driver();
                    _context.Drivers.Add(entity);
                }

                // --- COMMON MAPPING LOGIC (REFACTORED) ---
                // Map all properties from the ViewModel to the database Entity.
                // This avoids duplicating the mapping code in both the 'if' and 'else' blocks.
                entity.CompanyId = model.CompanyId;
                entity.DriverName = model.DriverName;
                entity.FatherName = model.FatherName;
                entity.BranchId = model.BranchId;
                entity.Designation = model.Designation;
                entity.EmployeeNo = model.EmployeeNo;
                entity.JoiningDate = model.JoiningDate;
                entity.ContractorId = model.ContractorId;
                entity.MonthlySalary = model.MonthlySalary;
                entity.Address = model.Address;
                entity.BirthDate = model.BirthDate;
                entity.Experience = model.Experience;
                entity.CellNo = model.CellNo;
                entity.LicenseNo = model.LicenseNo;
                entity.LicenseExpiry = model.LicenseExpiry; // Added missing field
                entity.CNIC = model.CNIC;
                entity.CNICExpiry = model.CNICExpiry;       // Added missing field
                entity.QualificationId = model.QualificationId;
                entity.NoKName = model.NoKName;
                entity.NoKRelationId = model.NoKRelationId; // Added missing field
                entity.PreviousEmployer = model.PreviousEmployer;
                entity.SeparationTypeId = model.SeparationTypeId;
                entity.SeparationDate = model.SeparationDate;   // Added missing field
                entity.SeparationReason = model.SeparationReason; // Added missing field
                entity.IsActive = model.IsActive;

                // Note: Audit fields (CreatedBy, UpdatedBy, etc.) should be set in the Controller
                // before this method is called.

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving driver for DriverId {DriverId}", model.DriverId);
                throw; // Re-throwing the exception allows the controller to handle it gracefully.
            }
        }
        public async Task<bool> DeleteDriverAsync(short id)
        {
            try
            {
                var entity = await _context.Drivers.FindAsync(id);
                if (entity == null) return false;

                _context.Drivers.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting driver with ID {DriverId}", id);
                throw;
            }
        }

        public async Task<bool> DriverNameExistsAsync(string driverName, short? excludeId = null)
        {
            try
            {
                var query = _context.Drivers.Where(x => x.DriverName == driverName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.DriverId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if driver name exists");
                throw;
            }
        }

        public async Task<DriverLookupsViewModel> GetLookupsAsync()
        {
            try
            {
                var lookups = new DriverLookupsViewModel();

                lookups.Companies = await _context.Companies
                  .Where(x => x.IsActive)
                  .OrderBy(x => x.CompanyName)
                  .Select(x => new Models.CompanyLookupViewModel
                  {
                      CompanyId = x.CompanyId,
                      CompanyName = x.CompanyName
                  })
                  .ToListAsync();

                lookups.Branches = await _context.Branches
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.BranchName)
                    .Select(x => new BranchLookupViewModel
                    {
                        BranchId = (short)x.BranchId,
                        BranchName = x.BranchName
                    })
                    .ToListAsync();

                lookups.Contractors = await _context.Contractors
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.ContractorName)
                   .Select(x => new ViewModels.ContractorLookupViewModel
                   {
                       ContractorId = x.ContractorId,
                       ContractorName = x.ContractorName
                   })
                   .ToListAsync();

                lookups.Qualifications = await _context.Qualifications
                    .Where(x => x.IsActive)
                .OrderBy(x => x.QualificationName)
                    .Select(x => new QualificationLookupViewModel
                    {
                        QualificationId = x.QualificationId,
                        QualificationName = x.QualificationName
                    })
                    .ToListAsync();

                lookups.NOKRelations = await _context.Relations
                    .Where(x => x.IsActive)
                .OrderBy(x => x.RelationName)
                    .Select(x => new NOKRelationLookupViewModel
                    {
                        RelationId  = x.RelationId,
                        RelationName = x.RelationName
                    })
                    .ToListAsync();

                lookups.SeparationTypes = await _context.SeparationTypes
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.TypeName)
                    .Select(x => new SeparationTypeLookupViewModel
                    {
                        SeparationTypeId = x.TypeId,
                        SeparationTypeName = x.TypeName
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