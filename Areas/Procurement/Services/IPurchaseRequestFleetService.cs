using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Procurement.Enums;
using ProcureToPay.Areas.Procurement.Models;
using ProcureToPay.Areas.Procurement.ViewModels;
using ProcureToPay.Data; // Assuming ApplicationDbContext is in ProcureToPay.Data
using ProcureToPay.Helpers;
using System.Linq; // Needed for Any() and ToList() on collections

namespace ProcureToPay.Areas.Procurement.Services
{
    public interface IPurchaseRequestFleetService
    {
        Task<IEnumerable<PurchaseRequestFleetListViewModel>> GetAllAsync(PurchaseRequestFleetFilterViewModel filter);
        Task<DisplayPurchaseRequestFleetViewModel?> GetByIdAsync(int id);
        Task<CreatePurchaseRequestFleetViewModel> GetCreateViewModelAsync();
        Task<EditPurchaseRequestFleetViewModel?> GetEditViewModelAsync(int id);
        Task<int> CreateAsync(CreatePurchaseRequestFleetViewModel model, int userId);
        Task<bool> UpdateAsync(int id, EditPurchaseRequestFleetViewModel model, int userId);
        Task<bool> DeleteAsync(int id, int userId);
        Task<bool> SubmitAsync(int id, int userId);
        Task<bool> ApproveAsync(int id, int userId, string remarks = "");
        Task<bool> RejectAsync(int id, int userId, string remarks);
        Task<PurchaseRequestFleetSummaryViewModel> GetSummaryAsync();
        Task<PurchaseRequestDetailFleetSummaryViewModel> GetDetailSummaryAsync(int prFleetId);
        Task<bool> ValidateBusinessRulesAsync(PurchaseRequestsFleet entity);
        Task<int?> GeneratePRNumberAsync();
    }

    public class PurchaseRequestFleetService : IPurchaseRequestFleetService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PurchaseRequestFleetService> _logger;

        public PurchaseRequestFleetService(
            ApplicationDbContext context,
            ILogger<PurchaseRequestFleetService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PurchaseRequestFleetListViewModel>> GetAllAsync(PurchaseRequestFleetFilterViewModel filter)
        {
            try
            {
                var query = _context.Set<PurchaseRequestsFleet>()
                    .Include(pr => pr.Details)
                    .AsQueryable();

                // Apply filters
                if (filter.PRNo.HasValue)
                    query = query.Where(pr => pr.PRNo == filter.PRNo);

                if (!string.IsNullOrEmpty(filter.CompanyCode))
                    query = query.Where(pr => pr.CompanyCode == filter.CompanyCode);

                if (!string.IsNullOrEmpty(filter.BranchCode))
                    query = query.Where(pr => pr.BranchCode == filter.BranchCode);

                if (!string.IsNullOrEmpty(filter.DepartmentCode))
                    query = query.Where(pr => pr.DepartmentCode == filter.DepartmentCode);

                if (filter.StateId.HasValue)
                    query = query.Where(pr => pr.StateId == filter.StateId);

                if (!string.IsNullOrEmpty(filter.Owner))
                    query = query.Where(pr => pr.Owner.Contains(filter.Owner));

                if (filter.RequiredFromDate.HasValue)
                    query = query.Where(pr => pr.RequiredBy >= filter.RequiredFromDate);

                if (filter.RequiredToDate.HasValue)
                    query = query.Where(pr => pr.RequiredBy <= filter.RequiredToDate);

                if (filter.CreatedFromDate.HasValue)
                    query = query.Where(pr => pr.CreatedOn >= filter.CreatedFromDate);

                if (filter.CreatedToDate.HasValue)
                    query = query.Where(pr => pr.CreatedOn <= filter.CreatedToDate);

                // Apply sorting
                query = filter.SortField?.ToLower() switch
                {
                    "prno" => filter.SortDirection == "desc" ? query.OrderByDescending(pr => pr.PRNo) : query.OrderBy(pr => pr.PRNo),
                    "owner" => filter.SortDirection == "desc" ? query.OrderByDescending(pr => pr.Owner) : query.OrderBy(pr => pr.Owner),
                    "requiredby" => filter.SortDirection == "desc" ? query.OrderByDescending(pr => pr.RequiredBy) : query.OrderBy(pr => pr.RequiredBy),
                    "statename" => filter.SortDirection == "desc" ? query.OrderByDescending(pr => pr.StateName) : query.OrderBy(pr => pr.StateName),
                    _ => filter.SortDirection == "desc" ? query.OrderByDescending(pr => pr.CreatedOn) : query.OrderBy(pr => pr.CreatedOn)
                };

                // Apply pagination
                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var result = items.Select(pr => new PurchaseRequestFleetListViewModel
                {
                    Id = pr.Id,
                    PRNo = pr.PRNo,
                    CompanyName = pr.CompanyName ?? pr.CompanyCode,
                    BranchName = pr.BranchName ?? pr.BranchCode,
                    DepartmentName = pr.DepartmentName ?? pr.DepartmentCode,
                    Owner = pr.Owner,
                    RequiredBy = pr.RequiredBy,
                    StateName = pr.StateName,
                    BudgetAmount = pr.BudgetAmount,
                    CreatedDate = pr.CreatedOn,
                    CreatedBy = pr.CreatedByUserId.ToString(),
                    StatusClass = GetStatusClass(pr.StateName),
                    StatusIcon = GetStatusIcon(pr.StateName),
                    CanEdit = CanEdit(pr.StateId),
                    CanDelete = CanDelete(pr.StateId),
                    CanApprove = CanApprove(pr.StateId),
                    CanReject = CanReject(pr.StateId)
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase request fleet list");
                throw;
            }
        }

        public async Task<DisplayPurchaseRequestFleetViewModel?> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _context.Set<PurchaseRequestsFleet>()
                    .Include(pr => pr.Details)
                    .Include(pr => pr.ProductNature) // Ensure these are properly loaded if needed for display
                    .Include(pr => pr.ServiceNature) // Ensure these are properly loaded if needed for display
                    .FirstOrDefaultAsync(pr => pr.Id == id);

                if (entity == null)
                    return null;

                // Manual mapping for DisplayPurchaseRequestFleetViewModel
                var viewModel = new DisplayPurchaseRequestFleetViewModel
                {
                    Id = entity.Id,
                    PRNo = entity.PRNo,
                    CompanyCode = entity.CompanyCode,
                    CompanyName = entity.CompanyName,
                    BranchCode = entity.BranchCode,
                    BranchName = entity.BranchName,
                    DepartmentCode = entity.DepartmentCode,
                    DepartmentName = entity.DepartmentName,
                    Owner = entity.Owner,
                    RequiredBy = entity.RequiredBy,
                    BudgetAmount = entity.BudgetAmount,
                    StateId = entity.StateId,
                    StateName = entity.StateName,
                    WorkFlowId = entity.WorkFlowId,
                    CurrentApprovalSequence = entity.CurrentApprovalSequence,
                    Approved = entity.Approved,
                    Rejected = entity.Rejected,
                    IsCompleted = entity.IsCompleted,
                    CreatedDate = entity.CreatedOn,
                    CreatedBy = entity.CreatedByUserId.ToString(), // Assuming CreatedByUserId is int
                    ModifiedDate = entity.UpdatedOn,
                    ModifiedBy = entity.UpdatedByUserId?.ToString(), // Assuming UpdatedByUserId is int?
                    ProductNatureName = entity.ProductNatureName,// Access via navigation property
                    ServiceNatureName = entity.ServiceNatureName, // Access via navigation property
                };

                // Map details
                viewModel.Details = entity.Details.Select(d => new DisplayPurchaseRequestDetailFleetViewModel
                {
                    Id = d.Id,
                    PRFleetId = d.PRFleetId,
                    DetailId = d.DetailId,
                    ProductId = d.ProductId,
                    ProductName = d.ProductName,
                    ServiceId = d.ServiceId,
                    ServiceName = d.ServiceName,
                    Quantity = d.Quantity,
                    UoMId = d.UoMId,
                    UoMName = d.UoMName,
                    Price = d.Price,
                    GSTRate = d.GSTRate,
                    GrossAmount = d.GrossAmount,
                    GSTAmount = d.GSTAmount,
                    NetAmount = d.NetAmount,
                    Remarks = d.Remarks,
                    CreatedDate = d.CreatedOn,
                    CreatedBy = d.CreatedByUserId.ToString(),
                    ModifiedDate = d.UpdatedOn,
                    ModifiedBy = d.UpdatedByUserId?.ToString()
                }).ToList();

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase request fleet with ID {Id}", id);
                throw;
            }
        }

        public async Task<CreatePurchaseRequestFleetViewModel> GetCreateViewModelAsync()
        {
            return new CreatePurchaseRequestFleetViewModel
            {
                RequiredBy = DateTime.Now.AddDays(7), // Default to one week from now
                Details = new List<CreatePurchaseRequestDetailFleetViewModel>()
            };
        }

        public async Task<EditPurchaseRequestFleetViewModel?> GetEditViewModelAsync(int id)
        {
            try
            {
                var entity = await _context.Set<PurchaseRequestsFleet>()
                    .Include(pr => pr.Details)
                    .FirstOrDefaultAsync(pr => pr.Id == id);

                if (entity == null)
                    return null;

                // Manual mapping for EditPurchaseRequestFleetViewModel
                var viewModel = new EditPurchaseRequestFleetViewModel
                {
                    Id = entity.Id,
                    PRNo = entity.PRNo,
                    CompanyCode = entity.CompanyCode,
                    CompanyName = entity.CompanyName,
                    BranchCode = entity.BranchCode,
                    BranchName = entity.BranchName,
                    DepartmentCode = entity.DepartmentCode,
                    DepartmentName = entity.DepartmentName,
                    Owner = entity.Owner,
                    RequiredBy = entity.RequiredBy,
                    BudgetAmount = entity.BudgetAmount,
                    StateId = entity.StateId,
                    ProductNatureName = entity.ProductNatureName,
                    ServiceNatureName = entity.ServiceNatureName,
                    // NewAttachments will remain null or empty as it's for new uploads, not existing
                };

                // Map details
                viewModel.Details = entity.Details.Select(d => new EditPurchaseRequestDetailFleetViewModel
                {
                    Id = d.Id,
                    PRFleetId = d.PRFleetId,
                    DetailId = d.DetailId,
                    ProductId = d.ProductId,
                    ProductName = d.ProductName,
                    ServiceId = d.ServiceId,
                    ServiceName = d.ServiceName,
                    Quantity = d.Quantity,
                    UoMId = d.UoMId,
                    UoMName = d.UoMName,
                    Price = d.Price,
                    GSTRate = d.GSTRate,
                    Remarks = d.Remarks,
                    CreatedDate = d.CreatedOn,
                    CreatedBy = d.CreatedByUserId.ToString(),
                    ModifiedDate = d.UpdatedOn,
                    ModifiedBy = d.UpdatedByUserId?.ToString()
                }).ToList();

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving edit view model for ID {Id}", id);
                throw;
            }
        }

        public async Task<int> CreateAsync(CreatePurchaseRequestFleetViewModel model, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Generate PR Number
                var prNumber = await GeneratePRNumberAsync();

                // Manual mapping for PurchaseRequestsFleet
                var entity = new PurchaseRequestsFleet
                {
                    CompanyCode = model.CompanyCode,
                    CompanyName = model.CompanyName,
                    BranchCode = model.BranchCode,
                    BranchName = model.BranchName,
                    DepartmentCode = model.DepartmentCode,
                    DepartmentName = model.DepartmentName,
                    Owner = model.Owner,
                    RequiredBy = model.RequiredBy,
                    BudgetAmount = model.BudgetAmount,
                    ProductNatureName = model.ProductNatureName,
                    ServiceNatureName = model.ServiceNatureName,
                    PRNo = prNumber,
                    CreatedByUserId = userId,
                    CreatedOn = DateTime.UtcNow,
                    StateId = (short)PurchaseRequestState.Saved,
                    // These will be set by the workflow or later actions
                    Approved = false,
                    Rejected = false,
                    IsCompleted = false,
                };

                // Set workflow
                entity.WorkFlowId = await GetWorkflowIdAsync(entity);

                _context.Set<PurchaseRequestsFleet>().Add(entity);
                await _context.SaveChangesAsync();

                // Add details
                if (model.Details?.Any() == true)
                {
                    foreach (var detailModel in model.Details)
                    {
                        // Manual mapping for PurchaseRequestDetailFleet
                        var detail = new PurchaseRequestDetailFleet
                        {
                            PRFleetId = entity.Id, // Link to the newly created PRFleet
                            DetailId = detailModel.DetailId,
                            ProductId = detailModel.ProductId,
                            ProductName = detailModel.ProductName,
                            ServiceId = detailModel.ServiceId,
                            ServiceName = detailModel.ServiceName,
                            Quantity = detailModel.Quantity,
                            UoMId = detailModel.UoMId,
                            UoMName = detailModel.UoMName,
                            Price = detailModel.Price,
                            GSTRate = detailModel.GSTRate,
                            GrossAmount = detailModel.GrossAmount,
                            GSTAmount = detailModel.GSTAmount,
                            NetAmount = detailModel.NetAmount,
                            Remarks = detailModel.Remarks,
                            CreatedByUserId = userId,
                            CreatedOn = DateTime.UtcNow,
                        };
                        _context.Set<PurchaseRequestDetailFleet>().Add(detail);
                    }
                    await _context.SaveChangesAsync();
                }

                // Handle attachments - simplified without attachment service
                if (model.Attachments?.Any() == true)
                {
                    _logger.LogInformation("Attachments uploaded for PR Fleet {Id}: {Count} files", entity.Id, model.Attachments.Count);
                    // TODO: Implement attachment handling logic here
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Purchase request fleet created with ID {Id} by user {UserId}", entity.Id, userId);

                return entity.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating purchase request fleet");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(int id, EditPurchaseRequestFleetViewModel model, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var entity = await _context.Set<PurchaseRequestsFleet>()
                    .Include(pr => pr.Details)
                    .FirstOrDefaultAsync(pr => pr.Id == id);

                if (entity == null)
                    return false;

                // Check if editable
                if (!((PurchaseRequestState)entity.StateId).IsEditable())
                {
                    throw new InvalidOperationException("Purchase request cannot be edited in current state.");
                }

                // Manual update of main entity properties
                entity.CompanyCode = model.CompanyCode;
                entity.CompanyName = model.CompanyName;
                entity.BranchCode = model.BranchCode;
                entity.BranchName = model.BranchName;
                entity.DepartmentCode = model.DepartmentCode;
                entity.DepartmentName = model.DepartmentName;
                entity.Owner = model.Owner;
                entity.RequiredBy = model.RequiredBy;
                entity.BudgetAmount = model.BudgetAmount;
                entity.ProductNatureName = model.ProductNatureName;
                entity.ServiceNatureName = model.ServiceNatureName;
                entity.UpdatedByUserId = userId;
                entity.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                // Update details
                await UpdateDetailsAsync(entity, model.Details, userId);

                // Handle new attachments - simplified
                if (model.NewAttachments?.Any() == true)
                {
                    _logger.LogInformation("New attachments uploaded for PR Fleet {Id}: {Count} files", entity.Id, model.NewAttachments.Count);
                    // TODO: Implement attachment handling logic here
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Purchase request fleet {Id} updated by user {UserId}", id, userId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating purchase request fleet with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            try
            {
                var entity = await _context.Set<PurchaseRequestsFleet>()
                    .Include(pr => pr.Details)
                    .FirstOrDefaultAsync(pr => pr.Id == id);

                if (entity == null)
                    return false;

                // Check if deletable
                if (!CanDelete(entity.StateId))
                {
                    throw new InvalidOperationException("Purchase request cannot be deleted in current state.");
                }

                // Remove details first
                _context.Set<PurchaseRequestDetailFleet>().RemoveRange(entity.Details);

                // Remove main entity
                _context.Set<PurchaseRequestsFleet>().Remove(entity);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Purchase request fleet {Id} deleted by user {UserId}", id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase request fleet with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> SubmitAsync(int id, int userId)
        {
            try
            {
                var entity = await _context.Set<PurchaseRequestsFleet>().FindAsync(id);
                if (entity == null)
                    return false;

                if (entity.StateId != (short)PurchaseRequestState.Saved)
                {
                    throw new InvalidOperationException("Only saved requests can be submitted.");
                }

                // Validate business rules
                if (!await ValidateBusinessRulesAsync(entity))
                {
                    throw new InvalidOperationException("Business rule validation failed.");
                }

                entity.StateId = (short)PurchaseRequestState.Submitted;
                entity.UpdatedByUserId = userId;
                entity.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                // Initialize workflow - simplified without workflow service
                entity.CurrentApprovalSequence = 1;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Purchase request fleet {Id} submitted by user {UserId}", id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting purchase request fleet with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> ApproveAsync(int id, int userId, string remarks = "")
        {
            try
            {
                var entity = await _context.Set<PurchaseRequestsFleet>().FindAsync(id);
                if (entity == null)
                    return false;

                if (entity.StateId != (short)PurchaseRequestState.Submitted)
                {
                    throw new InvalidOperationException("Only submitted requests can be approved.");
                }

                entity.StateId = (short)PurchaseRequestState.Approved;
                entity.Approved = true;
                entity.IsCompleted = true;
                entity.UpdatedByUserId = userId;
                entity.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                await _context.SaveChangesAsync();

                _logger.LogInformation("Purchase request fleet {Id} approved by user {UserId}", id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving purchase request fleet with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> RejectAsync(int id, int userId, string remarks)
        {
            try
            {
                var entity = await _context.Set<PurchaseRequestsFleet>().FindAsync(id);
                if (entity == null)
                    return false;

                if (entity.StateId != (short)PurchaseRequestState.Submitted)
                {
                    throw new InvalidOperationException("Only submitted requests can be rejected.");
                }

                entity.StateId = (short)PurchaseRequestState.Rejected;
                entity.Rejected = true;
                entity.IsCompleted = true;
                entity.UpdatedByUserId = userId;
                entity.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                await _context.SaveChangesAsync();

                _logger.LogInformation("Purchase request fleet {Id} rejected by user {UserId} with remarks: {Remarks}", id, userId, remarks);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting purchase request fleet with ID {Id}", id);
                throw;
            }
        }

        public async Task<PurchaseRequestFleetSummaryViewModel> GetSummaryAsync()
        {
            try
            {
                var summary = new PurchaseRequestFleetSummaryViewModel();

                var allRequests = await _context.Set<PurchaseRequestsFleet>().ToListAsync();

                summary.TotalRequests = allRequests.Count;
                summary.PendingApproval = allRequests.Count(pr => pr.StateId == (short)PurchaseRequestState.Submitted);
                summary.Approved = allRequests.Count(pr => pr.StateId == (short)PurchaseRequestState.Approved);
                summary.Rejected = allRequests.Count(pr => pr.StateId == (short)PurchaseRequestState.Rejected);
                summary.Draft = allRequests.Count(pr => pr.StateId == (short)PurchaseRequestState.Saved);

                summary.TotalBudgetAmount = allRequests.Sum(pr => pr.BudgetAmount ?? 0);
                summary.ApprovedBudgetAmount = allRequests
                    .Where(pr => pr.StateId == (short)PurchaseRequestState.Approved)
                    .Sum(pr => pr.BudgetAmount ?? 0);

                // Get recent requests
                var recent = await _context.Set<PurchaseRequestsFleet>()
                    .OrderByDescending(pr => pr.CreatedOn)
                    .Take(5)
                    .Select(pr => new PurchaseRequestFleetListViewModel
                    {
                        Id = pr.Id,
                        PRNo = pr.PRNo,
                        Owner = pr.Owner,
                        StateName = pr.StateName,
                        CreatedDate = pr.CreatedOn,
                        StatusClass = GetStatusClass(pr.StateName),
                        StatusIcon = GetStatusIcon(pr.StateName)
                    })
                    .ToListAsync();

                summary.RecentRequests = recent;

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary");
                throw;
            }
        }

        public async Task<PurchaseRequestDetailFleetSummaryViewModel> GetDetailSummaryAsync(int prFleetId)
        {
            try
            {
                var details = await _context.Set<PurchaseRequestDetailFleet>()
                    .Where(d => d.PRFleetId == prFleetId)
                    .ToListAsync();

                var summary = new PurchaseRequestDetailFleetSummaryViewModel
                {
                    TotalItems = details.Count,
                    TotalQuantity = details.Sum(d => d.Quantity),
                    TotalGrossAmount = details.Sum(d => d.GrossAmount),
                    TotalGSTAmount = details.Sum(d => d.GSTAmount),
                    TotalNetAmount = details.Sum(d => d.NetAmount),
                    AverageGSTRate = details.Any() ? details.Average(d => d.GSTRate) : 0,
                    Details = details.Select(d => new DisplayPurchaseRequestDetailFleetViewModel
                    {
                        Id = d.Id,
                        PRFleetId = d.PRFleetId,
                        ProductName = d.ProductName,
                        ServiceName = d.ServiceName,
                        Quantity = d.Quantity,
                        Price = d.Price,
                        GrossAmount = d.GrossAmount,
                        GSTAmount = d.GSTAmount,
                        NetAmount = d.NetAmount
                    }).ToList()
                };

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating detail summary for PR Fleet {Id}", prFleetId);
                throw;
            }
        }

        public async Task<bool> ValidateBusinessRulesAsync(PurchaseRequestsFleet entity)
        {
            // Load details if not already loaded
            if (!entity.Details.Any())
            {
                var details = await _context.Set<PurchaseRequestDetailFleet>()
                    .Where(d => d.PRFleetId == entity.Id)
                    .ToListAsync();

                if (!details.Any())
                {
                    return false; // Must have at least one detail line
                }
            }

            if (entity.BudgetAmount.HasValue && entity.BudgetAmount > 100000)
            {
                // High value requests might need special approval
                // Add logic here
            }

            return true;
        }

        public async Task<int?> GeneratePRNumberAsync()
        {
            try
            {
                var lastPR = await _context.Set<PurchaseRequestsFleet>()
                    .OrderByDescending(pr => pr.PRNo)
                    .FirstOrDefaultAsync();

                return (lastPR?.PRNo ?? 1000) + 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PR number");
                throw;
            }
        }

        #region Private Helper Methods

        private async Task UpdateDetailsAsync(PurchaseRequestsFleet entity, List<EditPurchaseRequestDetailFleetViewModel> modelDetails, int userId)
        {
            var existingDetails = entity.Details.ToList();
            var modelDetailIds = modelDetails.Where(d => d.Id > 0).Select(d => d.Id).ToList();

            // Remove deleted details
            var deletedDetails = existingDetails.Where(d => !modelDetailIds.Contains(d.Id)).ToList();
            _context.Set<PurchaseRequestDetailFleet>().RemoveRange(deletedDetails);

            // Update or add details
            foreach (var modelDetail in modelDetails)
            {
                if (modelDetail.Id > 0)
                {
                    // Update existing
                    var existingDetail = existingDetails.FirstOrDefault(d => d.Id == modelDetail.Id);
                    if (existingDetail != null)
                    {
                        // Manual mapping for existing detail
                        existingDetail.DetailId = modelDetail.DetailId;
                        existingDetail.ProductId = modelDetail.ProductId;
                        existingDetail.ProductName = modelDetail.ProductName;
                        existingDetail.ServiceId = modelDetail.ServiceId;
                        existingDetail.ServiceName = modelDetail.ServiceName;
                        existingDetail.Quantity = modelDetail.Quantity;
                        existingDetail.UoMId = modelDetail.UoMId;
                        existingDetail.UoMName = modelDetail.UoMName;
                        existingDetail.Price = modelDetail.Price;
                        existingDetail.GSTRate = modelDetail.GSTRate;
                        existingDetail.GrossAmount = modelDetail.GrossAmount; // Ensure these are calculated or passed
                        existingDetail.GSTAmount = modelDetail.GSTAmount;     // Ensure these are calculated or passed
                        existingDetail.NetAmount = modelDetail.NetAmount;       // Ensure these are calculated or passed
                        existingDetail.Remarks = modelDetail.Remarks;
                        existingDetail.UpdatedByUserId = userId;
                        existingDetail.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();
                    }
                }
                else
                {
                    // Add new
                    // Manual mapping for new detail
                    var newDetail = new PurchaseRequestDetailFleet
                    {
                        PRFleetId = entity.Id,
                        DetailId = modelDetail.DetailId,
                        ProductId = modelDetail.ProductId,
                        ProductName = modelDetail.ProductName,
                        ServiceId = modelDetail.ServiceId,
                        ServiceName = modelDetail.ServiceName,
                        Quantity = modelDetail.Quantity,
                        UoMId = modelDetail.UoMId,
                        UoMName = modelDetail.UoMName,
                        Price = modelDetail.Price,
                        GSTRate = modelDetail.GSTRate,
                        GrossAmount = modelDetail.GrossAmount, // Ensure these are calculated or passed
                        GSTAmount = modelDetail.GSTAmount,     // Ensure these are calculated or passed
                        NetAmount = modelDetail.NetAmount,       // Ensure these are calculated or passed
                        Remarks = modelDetail.Remarks,
                        CreatedByUserId = userId,
                        CreatedOn = DateTime.UtcNow,
                    };
                    _context.Set<PurchaseRequestDetailFleet>().Add(newDetail);
                }
            }
        }

        private async Task<short> GetWorkflowIdAsync(PurchaseRequestsFleet entity)
        {
            // Logic to determine appropriate workflow based on amount, department, etc.
            // For now, return a default workflow ID
            return 1; // Replace with actual workflow determination logic
        }

        private string GetStatusClass(string stateName)
        {
            return stateName?.ToLower() switch
            {
                "approved" => "success",
                "rejected" => "danger",
                "submitted" => "warning",
                "saved" => "secondary",
                _ => "primary"
            };
        }

        private string GetStatusIcon(string stateName)
        {
            return stateName?.ToLower() switch
            {
                "approved" => "fas fa-check-circle",
                "rejected" => "fas fa-times-circle",
                "submitted" => "fas fa-clock",
                "saved" => "fas fa-edit",
                _ => "fas fa-file"
            };
        }

        private bool CanEdit(short stateId)
        {
            return ((PurchaseRequestState)stateId).IsEditable();
        }

        private bool CanDelete(short stateId)
        {
            return stateId == (short)PurchaseRequestState.Saved;
        }

        private bool CanApprove(short stateId)
        {
            return stateId == (short)PurchaseRequestState.Submitted;
        }

        private bool CanReject(short stateId)
        {
            return stateId == (short)PurchaseRequestState.Submitted;
        }

        #endregion
    }
}