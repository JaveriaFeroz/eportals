using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Make sure this is present for ILogger
using ProcureToPay.Areas.Inventory.Models; // Ensure this namespace is correct
using ProcureToPay.Areas.Master.Models; // Still needed if you use models from Master
using ProcureToPay.Data;
using ProcureToPay.Enums;
using System.Security.Claims; // Needed if you uncomment HttpContextAccessor later

namespace ProcureToPay.Areas.Inventory.Services // Changed namespace to Inventory.Services
{
    // --- IPurchaseRequisitionService Interface ---
    public interface IPurchaseRequisitionService
    {
        Task<IEnumerable<PurchaseRequisitionListViewModel>> GetPurchaseRequisitionsAsync(bool completedOnly = false, string searchTerm = null);
        Task<PurchaseRequisitionViewModel> GetPurchaseRequisitionByIdAsync(int id); // Changed to int for PRNo
        Task<bool> SavePurchaseRequisitionAsync(PurchaseRequisitionViewModel model);
        Task<bool> DeletePurchaseRequisitionAsync(int id); // Changed to int for PRNo
        Task<bool> PurchaseRequisitionExistsAsync(int prNo, int? excludeId = null); // Renamed and changed to int for PRNo
        Task<PurchaseRequisitionLookupsViewModel> GetLookupsAsync();
        Task<IEnumerable<ProductNatureLookupViewModel>> GetProductNaturesByPurchaseNatureTypeAsync(PurchaseNatureType type); // New method
        Task<IEnumerable<ServiceNatureLookupViewModel>> GetServiceNaturesByPurchaseNatureTypeAsync(PurchaseNatureType type); // New method
    }

    // --- PurchaseRequisitionService Implementation ---
    public class PurchaseRequisitionService : IPurchaseRequisitionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PurchaseRequisitionService> _logger;
        // private readonly IHttpContextAccessor _httpContextAccessor; // Uncomment and inject if you need current user details

        public PurchaseRequisitionService(ApplicationDbContext context, ILogger<PurchaseRequisitionService> logger /*, IHttpContextAccessor httpContextAccessor */)
        {
            _context = context;
            _logger = logger;
            // _httpContextAccessor = httpContextAccessor; // Assign if injected
        }

        public async Task<IEnumerable<PurchaseRequisitionListViewModel>> GetPurchaseRequisitionsAsync(bool completedOnly = false, string searchTerm = null)
        {
            try
            {
                var query = _context.PurchaseRequisitions
                    .Include(x => x.Branch) // Assuming navigation properties exist
                    .Include(x => x.Department)
                    .Include(x => x.ProducNature)
                    .Include(x => x.ServiceNature)
                    // .Include(x => x.State) // Removed as per request to hardcode StateName
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (completedOnly)
                {
                    query = query.Where(x => x.IsCompleted == true); // Filter by IsCompleted
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.CompanyCode.Contains(searchTerm) ||
                                             x.Owner.Contains(searchTerm) ||
                                             (x.Branch != null && x.Branch.BranchName.Contains(searchTerm)) ||
                                             (x.Department != null && x.Department.DepartmentName.Contains(searchTerm)));
                }

                var result = await query
                    .OrderByDescending(x => x.PRNo) // Order by PRNo
                    .Select(x => new PurchaseRequisitionListViewModel
                    {
                        PRNo = x.PRNo,
                        CompanyCode = x.CompanyCode,
                        BranchName = x.Branch != null ? x.Branch.BranchName : "",
                        DepartmentName = x.Department != null ? x.Department.DepartmentName : "",
                        ProductNatureName = x.ProducNature != null ? x.ProducNature.NatureName : "",
                        ServiceNatureName = x.ServiceNature != null ? x.ServiceNature.NatureName : "",
                        RequiredBy = x.RequiredBy.HasValue ? x.RequiredBy.Value : default, // Handle nullable DateTime
                        StateName = "Saved", // Hardcoded StateName as per request
                        Owner = x.Owner,
                        IsCompleted = x.IsCompleted.GetValueOrDefault(), // Handle nullable bool
                        Approved = x.Approved.GetValueOrDefault(),       // Handle nullable bool
                        Rejected = x.Rejected.GetValueOrDefault(),       // Handle nullable bool
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
                _logger.LogError(ex, "Error retrieving purchase requisitions");
                throw;
            }
        }


        public async Task<PurchaseRequisitionViewModel> GetPurchaseRequisitionByIdAsync(int id)
        {
            try
            {
                var entity = await _context.PurchaseRequisitions
                    .Include(x => x.Branch)
                    .Include(x => x.Department)
                    .Include(x => x.ProducNature)
                    .Include(x => x.ServiceNature)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    // >>> NEW: Include PR Details and their navigation properties <<<
                    .Include(x => x.Details) // Assuming 'Details' is the ICollection<PurchaseRequisitionDetail> property in PurchaseRequisition model
                        .ThenInclude(d => d.Products) // Include Product nav prop for detail Product name
                    .Include(x => x.Details)
                        .ThenInclude(d => d.UoMs) // Include UoM nav prop for detail UoM name
                    .Include(x => x.Details)
                    .FirstOrDefaultAsync(x => x.PRNo == id); // Using PRNo as primary key

                if (entity == null) return null;

                var viewModel = new PurchaseRequisitionViewModel
                {
                    PRNo = entity.PRNo,
                    CompanyCode = entity.CompanyCode,
                    BranchId = entity.BranchId,
                    DepartmentId = entity.DepartmentId,
                    ProductNatureId = entity.ProductNatureId,
                    ServiceNatureId = entity.ServiceNatureId,
                    RequiredBy = entity.RequiredBy,
                    StateId = entity.StateId,
                    Owner = entity.Owner,
                    IsCompleted = entity.IsCompleted,
                    Approved = entity.Approved,
                    Rejected = entity.Rejected,
                    Budgeted = entity.Budgeted,
                    BudgetAmount = entity.BudgetAmount,
                    BudgetRemarks = entity.BudgetRemarks,
                    Justification = entity.Justification,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn,
                    BranchName = entity.Branch?.BranchName,
                    DepartmentName = entity.Department?.DepartmentName,
                    ProductNatureName = entity.ProducNature?.NatureName,
                    ServiceNatureName = entity.ServiceNature?.NatureName,
                    StateName = "Saved", // Hardcoded

                    // Important: Map PurchaseNatureType and PurchaseItemType from entity fields
                    PurchaseNatureType = (PurchaseNatureType)(entity.RequestTypeId.HasValue ? entity.RequestTypeId.Value : 0), // Adjust default if 0 is not valid
                    // PurchaseItemType = (PurchaseItemType)(entity.PurchaseItemTypeId.HasValue ? entity.PurchaseItemTypeId.Value : 0), // You need to decide how to map this if it's not RequestTypeId

                    // >>> NEW: Map PurchaseRequisitionDetail entities to PurchaseRequisitionDetailViewModels <<<
                    PRDetails = entity.Details.Select(d => new PurchaseRequisitionDetailViewModel
                    {
                        Id = d.Id, // maps to DetailId from entity's Id
                        PRNo = d.PRNo,
                        ProductId = d.ProductId,
                        Remarks = d.Remarks, // Maps Narration
                        Quantity = d.Quantity,
                        UoMId = d.UoMId,
                        Price = d.Price,
                        GSTRate = d.GSTRate,

                        // Calculated properties in ViewModel will automatically compute
                        ProductName = d.Products?.ProductName, // Nav prop from ProductId
                        UoMName = d.UoMs?.UoMName, // Nav prop from UoMId
                    }).ToList()
                    // >>> END NEW <<<
                };

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase requisition with PR No. {PRNo}", id);
                throw;
            }
        }
public async Task<bool> SavePurchaseRequisitionAsync(PurchaseRequisitionViewModel model)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    PurchaseRequisition entity;

                    string currentUserName = "CurrentUser"; // Placeholder for demonstration  
                    string currentUserId = "current_user_id"; // Placeholder for demonstration  

                    short savedStateId = 1;

                    if (model.PRNo > 0) // Updating existing PR  
                    {
                        entity = await _context.PurchaseRequisitions
                                               .Include(pr => pr.Details)
                                               .FirstOrDefaultAsync(pr => pr.PRNo == model.PRNo);

                        if (entity == null)
                        {
                            _logger.LogWarning("Attempted to update a purchase requisition that does not exist. PR No: {PRNo}", model.PRNo);
                            return false;
                        }

                        // --- UPDATE MAIN PR LOGIC ---  
                        entity.CompanyCode = model.CompanyCode;
                        entity.BranchId = model.BranchId;
                        entity.DepartmentId = model.DepartmentId;
                        entity.ProductNatureId = model.ProductNatureId;
                        entity.ServiceNatureId = model.ServiceNatureId;
                        entity.RequiredBy = model.RequiredBy;
                        entity.StateId = savedStateId; // Set State to the hardcoded ID for "Saved"  
                        entity.Owner = currentUserName;
                        entity.Justification = model.Justification;
                    }
                    else // Creating new PR  
                    {
                        entity = new PurchaseRequisition
                        {
                            CompanyCode = model.CompanyCode,
                            BranchId = model.BranchId,
                            DepartmentId = model.DepartmentId,
                            ProductNatureId = model.ProductNatureId,
                            ServiceNatureId = model.ServiceNatureId,
                            RequiredBy = model.RequiredBy,
                            StateId = savedStateId, // Set State to the hardcoded ID for "Saved"  
                            Owner = currentUserName,
                            Justification = model.Justification,
                            // Audit fields (CreatedOn, CreatedBy)  
                            CreatedOn = DateTime.Now,
                        };
                        _context.PurchaseRequisitions.Add(entity);
                    }

                    await _context.SaveChangesAsync(); // Save main PR first to get PRNo for new entities  

                    // --- Handle Purchase Requisition Details ---  
                    // Get IDs of details that were submitted from the form and are not marked for deletion  
                    var incomingDetailIds = model.PRDetails
                                                .Where(d => d.Id > 0) // Filter existing & not deleted  
                                                .Select(d => d.Id)
                                                .ToList();

                    foreach (var detailViewModel in model.PRDetails)
                    {
                        PurchaseRequisitionDetail detailEntity;
                        if (detailViewModel.Id > 0) // Existing detail item (update)
                        {
                            detailEntity = entity.Details.FirstOrDefault(d => d.Id == detailViewModel.Id);
                            if (detailEntity != null)
                            {
                                detailEntity.ProductId = detailViewModel.ProductId;
                                detailEntity.Remarks = detailViewModel.Remarks; // Maps Narration
                                detailEntity.Quantity = detailViewModel.Quantity;
                                detailEntity.UoMId = detailViewModel.UoMId;
                                detailEntity.Price = detailViewModel.Price;
                                detailEntity.GSTRate = detailViewModel.GSTRate;
                            }
                        }
                        else // New detail item (add)
                        {
                            detailEntity = new PurchaseRequisitionDetail
                            {
                                PRNo = entity.PRNo, // Link to the parent PR's PRNo (now guaranteed to have a value)
                                ProductId = detailViewModel.ProductId,
                                Remarks = detailViewModel.Remarks, // Maps Narration
                                Quantity = detailViewModel.Quantity,
                                UoMId = detailViewModel.UoMId,
                                Price = detailViewModel.Price,
                                GSTRate = detailViewModel.GSTRate,
                            };
                            entity.Details.Add(detailEntity); // Correctly add to the parent entity's Details collection
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving purchase requisition with details");
                    throw;
                }
            }
        }

        public async Task<bool> DeletePurchaseRequisitionAsync(int id)
        {
            try
            {
                var entity = await _context.PurchaseRequisitions.FindAsync(id);
                if (entity == null) return false;

                _context.PurchaseRequisitions.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase requisition with PR No. {PRNo}", id);
                throw;
            }
        }

        public async Task<bool> PurchaseRequisitionExistsAsync(int prNo, int? excludeId = null)
        {
            try
            {
                // For a new purchase requisition, if PRNo is 0, it means it's a new record
                // and we're not checking for an existing PRNo.
                if (prNo == 0 && !excludeId.HasValue)
                {
                    return false; // A new record won't have an existing PRNo yet.
                }

                var query = _context.PurchaseRequisitions.Where(x => x.PRNo == prNo);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.PRNo != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if purchase requisition PR No. exists");
                throw;
            }
        }

        public async Task<PurchaseRequisitionLookupsViewModel> GetLookupsAsync()
        {
            try
            {
                var lookups = new PurchaseRequisitionLookupsViewModel();

                lookups.Branches = await _context.Branches
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.BranchName)
                    .Select(x => new Models.BranchLookupViewModel
                    {
                        BranchId = (short)x.BranchId,
                        BranchName = x.BranchName
                    })
                    .ToListAsync();

                lookups.Departments = await _context.Departments
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.DepartmentName)
                    .Select(x => new DepartmentLookupViewModel
                    {
                        DepartmentId = (short)x.DepartmentId,
                        DepartmentName = x.DepartmentName
                    })
                    .ToListAsync();

                lookups.ProductNatures = await _context.ProductNatures
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.NatureName)
                   .Select(x => new ProductNatureLookupViewModel
                   {
                       NatureId = x.NatureId,
                       NatureName = x.NatureName,
                       IsOpex = x.IsOpex,
                       IsCapex = x.IsCapex
                   })
                   .ToListAsync();

                lookups.ServiceNatures = await _context.ServiceNatures
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.NatureName)
                   .Select(x => new ServiceNatureLookupViewModel
                   {
                       NatureId = x.NatureId,
                       NatureName = x.NatureName,
                       IsOpex = x.IsOpex,
                       IsCapex = x.IsCapex
                   })
                   .ToListAsync();

                return lookups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lookups for purchase requisition");
                throw;
            }
        }
        public async Task<IEnumerable<ProductNatureLookupViewModel>> GetProductNaturesByPurchaseNatureTypeAsync(PurchaseNatureType type)
        {
            try
            {
                IQueryable<Master.Models.ProductNature> query = _context.ProductNatures.Where(pn => pn.IsActive);

                if (type == PurchaseNatureType.Opex)
                {
                    query = query.Where(pn => pn.IsOpex);
                }
                else if (type == PurchaseNatureType.Capex)
                {
                    query = query.Where(pn => pn.IsCapex);
                }
                else // If "None" or other, return empty or all as per business logic
                {
                    return new List<ProductNatureLookupViewModel>();
                }

                return await query
                    .OrderBy(pn => pn.NatureName)
                    .Select(pn => new ProductNatureLookupViewModel
                    {
                        NatureId = pn.NatureId,
                        NatureName = pn.NatureName,
                        IsOpex = pn.IsOpex,
                        IsCapex = pn.IsCapex
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product natures by purchase nature type: {type}", type);
                throw;
            }
        }

        // New method to get Service Natures based on Opex/Capex
        public async Task<IEnumerable<ServiceNatureLookupViewModel>> GetServiceNaturesByPurchaseNatureTypeAsync(PurchaseNatureType type)
        {
            try
            {
                IQueryable<Master.Models.ServiceNature> query = _context.ServiceNatures.Where(sn => sn.IsActive);

                if (type == PurchaseNatureType.Opex)
                {
                    query = query.Where(sn => sn.IsOpex);
                }
                else if (type == PurchaseNatureType.Capex)
                {
                    query = query.Where(sn => sn.IsCapex);
                }
                else // If "None" or other, return empty or all as per business logic
                {
                    return new List<ServiceNatureLookupViewModel>();
                }

                return await query
                    .OrderBy(sn => sn.NatureName)
                    .Select(sn => new ServiceNatureLookupViewModel
                    {
                        NatureId = sn.NatureId,
                        NatureName = sn.NatureName,
                        IsOpex = sn.IsOpex,
                        IsCapex = sn.IsCapex
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service natures by purchase nature type: {type}", type);
                throw;
            }
        }

    }
}