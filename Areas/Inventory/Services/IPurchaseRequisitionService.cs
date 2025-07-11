using Microsoft.EntityFrameworkCore;
using ProcureToPay.Data;
using ProcureToPay.Areas.Inventory.Models; // Ensure this namespace is correct
using ProcureToPay.Areas.Master.Models; // Still needed if you use models from Master
using Microsoft.Extensions.Logging; // Make sure this is present for ILogger
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
        Task<IEnumerable<ProductNatureLookupViewModel>> GetProductNaturesByPurchaseNatureIdAsync(short purchaseNatureId);
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
                    .Include(x => x.PurchaseNature)
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
                        PurchaseNatureName = x.PurchaseNature != null ? x.PurchaseNature.PurchaseNatureName : "",
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

        public async Task<IEnumerable<WorkflowPurchaseRequisitionViewModel>> GetPurchaseRequisitionsForRWBAsync(bool completedOnly = false)
        {
            try
            {
                var query = _context.PurchaseRequisitions.AsQueryable();

                if (completedOnly)
                {
                    query = query.Where(x => x.IsCompleted == true);
                }

                var result = await query
                    .OrderByDescending(x => x.PRNo)
                    .Select(x => new WorkflowPurchaseRequisitionViewModel
                    {
                        PRNo = x.PRNo,
                        CompanyCode = x.CompanyCode,
                        StateName = "Saved", // Hardcoded StateName as per request
                        Owner = x.Owner
                    })
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase requisitions for RWB");
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
                    .Include(x => x.PurchaseNature)
                    // .Include(x => x.State) // Removed as per request
                    // .Include(x => x.ServiceGroup) // Removed as per request
                    // .Include(x => x.RequestType) // Removed as per request
                    // .Include(x => x.Workflow) // Removed as per request
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.PRNo == id); // Using PRNo as primary key

                if (entity == null) return null;

                return new PurchaseRequisitionViewModel
                {
                    PRNo = entity.PRNo,
                    CompanyCode = entity.CompanyCode,
                    BranchId = entity.BranchId,
                    DepartmentId = entity.DepartmentId,
                    ProductNatureId = (short)entity.ProductNatureId,
                    PurchaseNatureId = (short)entity.PurchaseNatureId,
                    RequiredBy = entity.RequiredBy,
                    StateId = entity.StateId, // Still gets StateId from entity
                    //ProductGroupId = entity.ProductGroupId,
                    //ServiceGroupId = entity.ServiceGroupId,
                    //RequestNatureId = entity.RequestNatureId,
                    //RequestTypeId = entity.RequestTypeId,
                    //WorkFlowId = entity.WorkFlowId,
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
                    PurchaseNatureName = entity.PurchaseNature?.PurchaseNatureName,
                    StateName = "Saved", // Hardcoded StateName as per request
                    //ServiceGroupName = null, // Set to null as ServiceGroup is not included
                    //RequestTypeName = null, // Set to null as RequestType is not included
                    //WorkflowName = null // Set to null as Workflow is not included
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase requisition with PR No. {PRNo}", id);
                throw;
            }
        }

        public async Task<bool> SavePurchaseRequisitionAsync(PurchaseRequisitionViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                PurchaseRequisition entity;

                // Placeholder for getting current user's ID/Name
                // In a real application, you'd typically get this from IHttpContextAccessor or an authentication service.
                // Example: var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                // Example: var currentUserName = _httpContextAccessor.HttpContext.User.Identity.Name;

                // For demonstration, let's assume a placeholder user name
                string currentUserName = "CurrentUser"; // Replace with actual current user logic

                // Hardcode StateId for "Saved"
                // Assuming '1' is the ID for the "Saved" state. Adjust if your actual State table's ID for "Saved" is different.
                short savedStateId = 1;

                if (model.PRNo > 0) // Check if we are UPDATING an existing purchase requisition
                {
                    entity = await _context.PurchaseRequisitions
                        .FirstOrDefaultAsync(x => x.PRNo == model.PRNo);

                    if (entity == null)
                    {
                        _logger.LogWarning("Attempted to update a purchase requisition that does not exist. PR No: {PRNo}", model.PRNo);
                        return false;
                    }

                    // --- UPDATE LOGIC ---
                    entity.CompanyCode = model.CompanyCode;
                    entity.BranchId = model.BranchId;
                    entity.DepartmentId = model.DepartmentId;
                    entity.ProductNatureId = model.ProductNatureId;
                    entity.PurchaseNatureId = model.PurchaseNatureId;
                    entity.RequiredBy = model.RequiredBy;
                    entity.StateId = savedStateId; // Set State to the hardcoded ID for "Saved"
                    //entity.ProductGroupId = model.ProductGroupId;
                    //entity.ServiceGroupId = model.ServiceGroupId;
                    //entity.RequestNatureId = model.RequestNatureId.GetValueOrDefault();
                    //entity.RequestTypeId = model.RequestTypeId.GetValueOrDefault();
                    //entity.WorkFlowId = model.WorkFlowId.GetValueOrDefault();
                    entity.Owner = currentUserName; // Set Owner to current user
                    entity.IsCompleted = model.IsCompleted;
                    entity.Approved = model.Approved;
                    entity.Rejected = model.Rejected;
                    entity.Budgeted = model.Budgeted;
                    entity.BudgetAmount = model.BudgetAmount;
                    entity.BudgetRemarks = model.BudgetRemarks;
                    entity.Justification = model.Justification;

                    entity.UpdatedOn = DateTime.Now;
                    // entity.UpdatedBy = currentUserId; // Set updated by user ID if available
                }
                else // Otherwise, we are CREATING a new purchase requisition
                {
                    // --- CREATE LOGIC ---
                    entity = new PurchaseRequisition
                    {
                        CompanyCode = model.CompanyCode,
                        BranchId = model.BranchId,
                        DepartmentId = model.DepartmentId,
                        ProductNatureId = model.ProductNatureId,
                        PurchaseNatureId = model.PurchaseNatureId,
                        RequiredBy = model.RequiredBy,
                        StateId = savedStateId, // Set State to the hardcoded ID for "Saved"
                        //ProductGroupId = model.ProductGroupId,
                        //ServiceGroupId = model.ServiceGroupId,
                        //RequestNatureId = model.RequestNatureId.GetValueOrDefault(),
                        //RequestTypeId = model.RequestTypeId.GetValueOrDefault(),
                        //WorkFlowId = model.WorkFlowId.GetValueOrDefault(),
                        Owner = currentUserName, // Set Owner to current user
                        IsCompleted = model.IsCompleted,
                        Approved = model.Approved,
                        Rejected = model.Rejected,
                        Budgeted = model.Budgeted,
                        BudgetAmount = model.BudgetAmount,
                        BudgetRemarks = model.BudgetRemarks,
                        Justification = model.Justification,
                        CreatedOn = DateTime.Now,
                        // CreatedBy = currentUserId; // Set created by user ID if available
                    };
                    _context.PurchaseRequisitions.Add(entity);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving purchase requisition");
                throw;
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
                       NatureName = x.NatureName
                   })
                   .ToListAsync();

                lookups.PurchaseNatures = await _context.PurchaseNatures
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.PurchaseNatureName)
                   .Select(x => new PurchaseNatureLookupViewModel
                   {
                       PurchaseNatureId = x.PurchaseNatureId,
                       PurchaseNatureName = x.PurchaseNatureName
                   })
                   .ToListAsync();

                //lookups.States = await _context.States
                //    .Where(x => x.IsActive)
                //    .OrderBy(x => x.StateName)
                //    .Select(x => new StateLookupViewModel
                //    {
                //        StateId = (short)x.StateId,
                //        StateName = x.StateName
                //    })
                //    .ToListAsync();

                //lookups.ServiceGroups = await _context.ServiceGroups
                //    .Where(x => x.IsActive)
                //    .OrderBy(x => x.ServiceGroupName)
                //    .Select(x => new ServiceGroupLookupViewModel
                //    {
                //        ServiceGroupId = (short)x.ServiceGroupId,
                //        ServiceGroupName = x.ServiceGroupName
                //    })
                //    .ToListAsync();

                //lookups.RequestTypes = await _context.RequestTypes
                //    .Where(x => x.IsActive)
                //    .OrderBy(x => x.RequestTypeName)
                //    .Select(x => new RequestTypeLookupViewModel
                //    {
                //        RequestTypeId = (short)x.RequestTypeId,
                //        RequestTypeName = x.RequestTypeName
                //    })
                //    .ToListAsync();

                //lookups.Workflows = await _context.Workflows
                //    .Where(x => x.IsActive)
                //    .OrderBy(x => x.WorkflowName)
                //    .Select(x => new WorkflowLookupViewModel
                //    {
                //        WorkflowId = (short)x.WorkflowId,
                //        WorkflowName = x.WorkflowName
                //    })
                //    .ToListAsync();

                return lookups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lookups for purchase requisition");
                throw;
            }
        }
        public async Task<IEnumerable<ProductNatureLookupViewModel>> GetProductNaturesByPurchaseNatureIdAsync(short purchaseNatureId)
        {
            try
            {
                var productNatures = await _context.ProductNatures
                    .Where(pn => pn.PurchaseNatureId == purchaseNatureId && pn.IsActive)
                    .OrderBy(pn => pn.NatureName)
                    .Select(pn => new ProductNatureLookupViewModel
                    {
                        NatureId = pn.NatureId,
                        NatureName = pn.NatureName,
                        PurchaseNatureId = pn.PurchaseNatureId
                    })
                    .ToListAsync();
                return productNatures;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product natures by PurchaseNatureId {purchaseNatureId}", purchaseNatureId);
                throw;
            }
        }

    }
}