using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Procurement.Models;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Procurement.Controllers
{
    [Area("Procurement")]
    [Authorize] // Require authentication for all actions
    public class PurchaseRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<PurchaseRequestController> _logger;

        public PurchaseRequestController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            ILogger<PurchaseRequestController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: PurchaseRequest
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<PurchaseRequest> query = _context.PurchaseRequests
                .Include(p => p.Items)
                .Include(p => p.RequestedByUser)
                .Include(p => p.Department)
                .Include(p => p.Branch)
                .Include(p => p.ApprovedByUser);

            // Role-based filtering
            if (userRoles.Contains("Admin") || userRoles.Contains("Finance Manager"))
            {
                // Admins and Finance Managers can see all requests
                query = query.OrderByDescending(p => p.CreatedAt);
            }
            else if (userRoles.Contains("Department Manager"))
            {
                // Department Managers can see requests from their department
                query = query.Where(p => p.DepartmentId == currentUser.DepartmentId ||
                                        p.RequestedByUserId == currentUser.Id)
                            .OrderByDescending(p => p.CreatedAt);
            }
            else
            {
                // Regular users can only see their own requests
                query = query.Where(p => p.RequestedByUserId == currentUser.Id)
                            .OrderByDescending(p => p.CreatedAt);
            }

            var requests = await query.ToListAsync();
            return View(requests);
        }

        // GET: PurchaseRequest/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // First, update the total to make sure it's current
            await UpdateTotalAmount(id.Value);
            await _context.SaveChangesAsync();

            var purchaseRequest = await _context.PurchaseRequests
                .Include(p => p.Items)
                .Include(p => p.RequestedByUser)
                .Include(p => p.Department)
                .Include(p => p.Branch)
                .Include(p => p.ApprovedByUser)
                .Include(p => p.Approvals)
                    .ThenInclude(a => a.Approver)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (purchaseRequest == null)
            {
                return NotFound();
            }

            // Check if user has permission to view this request
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            if (!await CanViewRequest(purchaseRequest, currentUser, userRoles))
            {
                return Forbid();
            }

            return View(purchaseRequest);
        }
        // GET: PurchaseRequest/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Populate dropdowns for departments and branches
            ViewBag.Departments = await _context.Departments
                .Where(d => d.IsActive)
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName,
                    Selected = d.DepartmentId == currentUser.DepartmentId
                })
                .ToListAsync();

            ViewBag.Branches = await _context.Branches
                .Where(b => b.IsActive)
                .Select(b => new SelectListItem
                {
                    Value = b.BranchId.ToString(),
                    Text = b.BranchName,
                    Selected = b.BranchId == currentUser.BranchId
                })
                .ToListAsync();

            var model = new PurchaseRequest
            {
                RequestDate = DateTime.Now,
                RequiredDate = DateTime.Now.AddDays(7), // Default to 7 days from now
                Status = RequestStatus.Draft,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                RequestedByUserId = currentUser.Id,
                DepartmentId = currentUser.DepartmentId,
                BranchId = currentUser.BranchId
            };

            return View(model);
        }

        // Updated POST: PurchaseRequest/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Purpose,RequiredDate,DepartmentId,BranchId")] PurchaseRequest purchaseRequest)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                // Remove validation errors for ALL system-generated fields
                ModelState.Remove("RequestNumber");
                ModelState.Remove("TotalAmount");
                ModelState.Remove("Status");
                ModelState.Remove("RequestedByUserId");
                ModelState.Remove("RequestDate");
                ModelState.Remove("CreatedAt");
                ModelState.Remove("UpdatedAt");
                ModelState.Remove("RequestedByUser");
                ModelState.Remove("ApprovedByUser");
                ModelState.Remove("ApprovedByUserId");
                ModelState.Remove("ApprovalComments");
                ModelState.Remove("ApprovedAt");
                ModelState.Remove("Department");
                ModelState.Remove("Branch");
                ModelState.Remove("Items");
                ModelState.Remove("Approvals");

                // Validate that department and branch are provided
                if (!purchaseRequest.DepartmentId.HasValue || purchaseRequest.DepartmentId.Value == 0)
                {
                    ModelState.AddModelError("DepartmentId", "Department is required.");
                }

                if (!purchaseRequest.BranchId.HasValue || purchaseRequest.BranchId.Value == 0)
                {
                    ModelState.AddModelError("BranchId", "Branch is required.");
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(purchaseRequest.Title))
                {
                    ModelState.AddModelError("Title", "Title is required.");
                }

                if (string.IsNullOrWhiteSpace(purchaseRequest.Purpose))
                {
                    ModelState.AddModelError("Purpose", "Purpose is required.");
                }

                if (ModelState.IsValid)
                {
                    // Generate request number based on existing count
                    var existingCount = await _context.PurchaseRequests.CountAsync();
                    var requestNumber = $"PR{DateTime.Now:yyyyMM}{existingCount + 1:D4}";

                    // Set system-generated fields
                    purchaseRequest.RequestNumber = requestNumber;
                    purchaseRequest.Status = RequestStatus.Draft;
                    purchaseRequest.TotalAmount = 0;
                    purchaseRequest.RequestDate = DateTime.Now;
                    purchaseRequest.CreatedAt = DateTime.Now;
                    purchaseRequest.UpdatedAt = DateTime.Now;

                    // Set user context
                    purchaseRequest.RequestedByUserId = currentUser.Id;

                    // Clear navigation properties to avoid validation issues
                    purchaseRequest.RequestedByUser = null;
                    purchaseRequest.ApprovedByUser = null;
                    purchaseRequest.Department = null;
                    purchaseRequest.Branch = null;
                    purchaseRequest.Items = null;
                    purchaseRequest.Approvals = null;

                    _context.Add(purchaseRequest);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Purchase request {RequestNumber} created by user {UserId}",
                        purchaseRequest.RequestNumber, currentUser.Id);

                    return RedirectToAction(nameof(Details), new { id = purchaseRequest.Id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase request");
                ModelState.AddModelError("", $"An error occurred while creating the request: {ex.Message}");
            }

            // If we got this far, something failed, redisplay form
            // Repopulate dropdowns
            var user = await _userManager.GetUserAsync(User);

            ViewBag.Departments = await _context.Departments
                .Where(d => d.IsActive)
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName,
                    Selected = d.DepartmentId == purchaseRequest.DepartmentId
                })
                .ToListAsync();

            ViewBag.Branches = await _context.Branches
                .Where(b => b.IsActive)
                .Select(b => new SelectListItem
                {
                    Value = b.BranchId.ToString(),
                    Text = b.BranchName,
                    Selected = b.BranchId == purchaseRequest.BranchId
                })
                .ToListAsync();

            return View(purchaseRequest);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int purchaseRequestId, string itemName, string description, int quantity, decimal unitPrice, string unit = "pcs")
        {
            try
            {
                var purchaseRequest = await _context.PurchaseRequests.FindAsync(purchaseRequestId);
                if (purchaseRequest == null)
                {
                    return Json(new { success = false, message = "Request not found" });
                }

                // Check if user can modify this request
                var currentUser = await _userManager.GetUserAsync(User);
                if (!await CanModifyRequest(purchaseRequest, currentUser))
                {
                    return Json(new { success = false, message = "You don't have permission to modify this request" });
                }

                if (purchaseRequest.Status != RequestStatus.Draft)
                {
                    return Json(new { success = false, message = "Only draft requests can be modified" });
                }

                var item = new PurchaseRequestItem
                {
                    PurchaseRequestId = purchaseRequestId,
                    ItemName = itemName,
                    Description = description,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    Unit = unit
                };

                _context.PurchaseRequestItems.Add(item);

                // Update total amount
                await UpdateTotalAmount(purchaseRequestId);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Item added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to purchase request {RequestId}", purchaseRequestId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            try
            {
                var item = await _context.PurchaseRequestItems
                    .Include(i => i.PurchaseRequest)
                    .FirstOrDefaultAsync(i => i.ItemId == itemId);

                if (item == null)
                {
                    return Json(new { success = false, message = "Item not found" });
                }

                // Check if user can modify this request
                var currentUser = await _userManager.GetUserAsync(User);
                if (!await CanModifyRequest(item.PurchaseRequest, currentUser))
                {
                    return Json(new { success = false, message = "You don't have permission to modify this request" });
                }

                var purchaseRequestId = item.PurchaseRequestId;
                _context.PurchaseRequestItems.Remove(item);

                // Update total amount
                await UpdateTotalAmount(purchaseRequestId);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Item removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item {ItemId}", itemId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForApproval(int id)
        {
            try
            {
                var purchaseRequest = await _context.PurchaseRequests
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (purchaseRequest == null)
                {
                    return Json(new { success = false, message = "Purchase request not found" });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (!await CanModifyRequest(purchaseRequest, currentUser))
                {
                    return Json(new { success = false, message = "You don't have permission to submit this request" });
                }

                if (purchaseRequest.Status != RequestStatus.Draft)
                {
                    return Json(new { success = false, message = "Only draft requests can be submitted for approval" });
                }

                if (!purchaseRequest.Items.Any())
                {
                    return Json(new { success = false, message = "Cannot submit request without items" });
                }

                // CRITICAL: Update the total amount BEFORE creating workflow
                await UpdateTotalAmount(purchaseRequest.Id);

                // Save the updated total first
                await _context.SaveChangesAsync();

                // Refresh the purchase request to get the updated total
                await _context.Entry(purchaseRequest).ReloadAsync();

                _logger.LogInformation("Request {RequestId} total before workflow creation: {Total}",
                    purchaseRequest.Id, purchaseRequest.TotalAmount);

                purchaseRequest.Status = RequestStatus.Submitted;
                purchaseRequest.UpdatedAt = DateTime.Now;

                // Create approval workflow based on the UPDATED amount
                await CreateApprovalWorkflow(purchaseRequest);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Purchase request {RequestNumber} submitted for approval by user {UserId} with total {Total}",
                    purchaseRequest.RequestNumber, currentUser.Id, purchaseRequest.TotalAmount);

                return Json(new
                {
                    success = true,
                    message = "Request submitted for approval successfully",
                    totalAmount = purchaseRequest.TotalAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting request {RequestId} for approval. Error: {Error}", id, ex.Message);

                var innerException = ex.InnerException?.Message ?? "No inner exception";
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}",
                    details = innerException
                });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Department Manager,Finance Manager,CEO")]
        public async Task<IActionResult> Approve(int id, string? approvalComments)
        {
            try
            {
                var purchaseRequest = await _context.PurchaseRequests
                    .Include(p => p.Approvals.OrderBy(a => a.Order))
                        .ThenInclude(a => a.Approver)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (purchaseRequest == null)
                {
                    return Json(new { success = false, message = "Purchase request not found" });
                }

                var currentUser = await _userManager.GetUserAsync(User);

                // DEBUG: Log the current user and their roles
                var userRoles = await _userManager.GetRolesAsync(currentUser);
                _logger.LogInformation("User {UserId} with roles [{Roles}] attempting to approve request {RequestId}",
                    currentUser.Id, string.Join(", ", userRoles), id);

                // Find the CURRENT pending approval for this user
                var pendingApproval = purchaseRequest.Approvals
                    .Where(a => a.ApproverId == currentUser.Id && a.Status == ApprovalStatus.Pending)
                    .OrderBy(a => a.Order)
                    .FirstOrDefault();

                if (pendingApproval == null)
                {
                    // DEBUG: Log all approvals for this request
                    var allApprovals = purchaseRequest.Approvals.Select(a => new {
                        ApproverId = a.ApproverId,
                        ApproverName = $"{a.Approver?.FirstName} {a.Approver?.LastName}",
                        Status = a.Status.ToString(),
                        Order = a.Order
                    }).ToList();

                    _logger.LogWarning("No pending approval found for user {UserId} on request {RequestId}. All approvals: {@Approvals}",
                        currentUser.Id, id, allApprovals);

                    return Json(new
                    {
                        success = false,
                        message = "No pending approval found for you",
                        debug = $"Your ID: {currentUser.Id}, Available approvals: {string.Join(", ", allApprovals.Select(a => $"Approver {a.ApproverId} ({a.ApproverName}) - {a.Status}"))}"
                    });
                }

                // Check if this is the next approval in sequence
                var previousApprovals = purchaseRequest.Approvals
                    .Where(a => a.Order < pendingApproval.Order)
                    .ToList();

                if (previousApprovals.Any() && previousApprovals.Any(a => a.Status == ApprovalStatus.Pending))
                {
                    return Json(new { success = false, message = "Previous approvals are still pending" });
                }

                // Update the current approval
                pendingApproval.Status = ApprovalStatus.Approved;
                pendingApproval.Comments = approvalComments;
                pendingApproval.ActionDate = DateTime.Now;

                // Check if this is the final approval
                var nextPendingApproval = purchaseRequest.Approvals
                    .Where(a => a.Order > pendingApproval.Order && a.Status == ApprovalStatus.Pending)
                    .OrderBy(a => a.Order)
                    .FirstOrDefault();

                if (nextPendingApproval == null)
                {
                    // This was the final approval
                    purchaseRequest.Status = RequestStatus.Approved;
                    purchaseRequest.ApprovedByUserId = currentUser.Id;
                    purchaseRequest.ApprovedAt = DateTime.Now;
                    purchaseRequest.ApprovalComments = approvalComments;
                }
                else
                {
                    // More approvals are needed
                    purchaseRequest.Status = RequestStatus.UnderReview;
                }

                purchaseRequest.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Purchase request {RequestNumber} approved by user {UserId}",
                    purchaseRequest.RequestNumber, currentUser.Id);

                var message = nextPendingApproval == null
                    ? "Request fully approved successfully"
                    : $"Request approved. Next approver: {nextPendingApproval.Approver?.FirstName} {nextPendingApproval.Approver?.LastName}";

                return Json(new
                {
                    success = true,
                    message = message,
                    nextApprover = nextPendingApproval?.Approver?.FirstName + " " + nextPendingApproval?.Approver?.LastName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving request {RequestId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Department Manager,Finance Manager")]
        public async Task<IActionResult> Reject(int id, string? rejectionComments)
        {
            try
            {
                var purchaseRequest = await _context.PurchaseRequests
                    .Include(p => p.Approvals)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (purchaseRequest == null)
                {
                    return Json(new { success = false, message = "Purchase request not found" });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (!await CanApproveRequest(purchaseRequest, currentUser))
                {
                    return Json(new { success = false, message = "You don't have permission to reject this request" });
                }

                // Find the pending approval for this user
                var pendingApproval = purchaseRequest.Approvals
                    .FirstOrDefault(a => a.ApproverId == currentUser.Id && a.Status == ApprovalStatus.Pending);

                if (pendingApproval != null)
                {
                    pendingApproval.Status = ApprovalStatus.Rejected;
                    pendingApproval.Comments = rejectionComments;
                    pendingApproval.ActionDate = DateTime.Now;
                }

                purchaseRequest.Status = RequestStatus.Rejected;
                purchaseRequest.ApprovalComments = rejectionComments;
                purchaseRequest.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Purchase request {RequestNumber} rejected by user {UserId}",
                    purchaseRequest.RequestNumber, currentUser.Id);

                return Json(new { success = true, message = "Request rejected successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting request {RequestId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var purchaseRequest = await _context.PurchaseRequests.FindAsync(id);

                if (purchaseRequest == null)
                {
                    return Json(new { success = false, message = "Purchase request not found" });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (!await CanModifyRequest(purchaseRequest, currentUser))
                {
                    return Json(new { success = false, message = "You don't have permission to cancel this request" });
                }

                if (purchaseRequest.Status == RequestStatus.Approved)
                {
                    return Json(new { success = false, message = "Approved requests cannot be cancelled" });
                }

                purchaseRequest.Status = RequestStatus.Cancelled;
                purchaseRequest.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Purchase request {RequestNumber} cancelled by user {UserId}",
                    purchaseRequest.RequestNumber, currentUser.Id);

                return Json(new { success = true, message = "Request cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling request {RequestId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Get approval status information
        public async Task<IActionResult> GetApprovalStatus(int id)
        {
            var purchaseRequest = await _context.PurchaseRequests
                .Include(p => p.Approvals.OrderBy(a => a.Order))
                    .ThenInclude(a => a.Approver)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchaseRequest == null)
            {
                return Json(new { success = false, message = "Request not found" });
            }

            var approvalStatus = purchaseRequest.Approvals.Select(a => new
            {
                ApproverName = $"{a.Approver.FirstName} {a.Approver.LastName}",
                Role = a.ApprovalLevel?.Name ?? "Unknown",
                Status = a.Status.ToString(),
                Comments = a.Comments,
                ActionDate = a.ActionDate?.ToString("yyyy-MM-dd HH:mm"),
                Order = a.Order,
                IsCurrent = a.Status == ApprovalStatus.Pending &&
                           !purchaseRequest.Approvals.Any(prev => prev.Order < a.Order && prev.Status == ApprovalStatus.Pending)
            }).ToList();

            return Json(new { success = true, approvals = approvalStatus });
        }

        #region Helper Methods

        private async Task UpdateTotalAmount(int purchaseRequestId)
        {
            try
            {
                // Get items directly from database to avoid navigation property issues
                var totalAmount = await _context.PurchaseRequestItems
                    .Where(i => i.PurchaseRequestId == purchaseRequestId)
                    .SumAsync(i => i.Quantity * i.UnitPrice);

                // Get the purchase request
                var purchaseRequest = await _context.PurchaseRequests
                    .FirstOrDefaultAsync(p => p.Id == purchaseRequestId);

                if (purchaseRequest != null)
                {
                    purchaseRequest.TotalAmount = totalAmount;
                    purchaseRequest.UpdatedAt = DateTime.Now;

                    _logger.LogInformation("Updated total for Request {RequestId}: {Total}",
                        purchaseRequestId, totalAmount);
                }
                else
                {
                    _logger.LogWarning("Purchase request {RequestId} not found when updating total", purchaseRequestId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating total amount for request {RequestId}", purchaseRequestId);
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FixAllTotals()
        {
            try
            {
                var allRequests = await _context.PurchaseRequests.ToListAsync();
                int updatedCount = 0;

                foreach (var request in allRequests)
                {
                    var correctTotal = await _context.PurchaseRequestItems
                        .Where(i => i.PurchaseRequestId == request.Id)
                        .SumAsync(i => i.Quantity * i.UnitPrice);

                    if (request.TotalAmount != correctTotal)
                    {
                        request.TotalAmount = correctTotal;
                        request.UpdatedAt = DateTime.Now;
                        updatedCount++;

                        _logger.LogInformation("Fixed total for Request {RequestId}: {OldTotal} -> {NewTotal}",
                            request.Id, request.TotalAmount, correctTotal);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Fixed {updatedCount} request totals",
                    updatedCount = updatedCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing request totals");
                return Json(new { success = false, message = ex.Message });
            }
        }
        private async Task<bool> CanViewRequest(PurchaseRequest request, User currentUser, IList<string> userRoles)
        {
            // Admins can view all
            if (userRoles.Contains("Admin"))
                return true;

            // Finance managers can view all
            if (userRoles.Contains("Finance Manager"))
                return true;

            // Department managers can view requests from their department
            if (userRoles.Contains("Department Manager") && request.DepartmentId == currentUser.DepartmentId)
                return true;

            // Users can view their own requests
            if (request.RequestedByUserId == currentUser.Id)
                return true;

            // Check if user is an approver for this request
            var isApprover = await _context.RequestApprovals
                .AnyAsync(a => a.PurchaseRequestId == request.Id && a.ApproverId == currentUser.Id);

            return isApprover;
        }

        private async Task<bool> CanModifyRequest(PurchaseRequest request, User currentUser)
        {
            // Only the requester can modify their own draft requests
            return request.RequestedByUserId == currentUser.Id && request.Status == RequestStatus.Draft;
        }

        private async Task<bool> CanApproveRequest(PurchaseRequest request, User currentUser)
        {
            // Check if user has a pending approval for this request
            return await _context.RequestApprovals
                .AnyAsync(a => a.PurchaseRequestId == request.Id &&
                              a.ApproverId == currentUser.Id &&
                              a.Status == ApprovalStatus.Pending);
        }

        private async Task CreateApprovalWorkflow(PurchaseRequest request)
        {
            try
            {
                var approvals = new List<RequestApproval>();
                var currentOrder = 1;

                _logger.LogInformation("Attempting to create approval workflow for RequestId: {RequestId}, TotalAmount: {TotalAmount}, DepartmentId: {DepartmentId}, BranchId: {BranchId}",
                    request.Id, request.TotalAmount, request.DepartmentId, request.BranchId);

                // Step 1: Department Manager (if not the requester)
                if (request.DepartmentId.HasValue)
                {
                    var departmentManager = await GetDepartmentManager(request.DepartmentId.Value, request.RequestedByUserId);
                    if (departmentManager != null)
                    {
                        var deptApprovalLevelId = await GetApprovalLevelId("Department Manager");
                        if (deptApprovalLevelId > 0)
                        {
                            approvals.Add(new RequestApproval
                            {
                                PurchaseRequestId = request.Id,
                                ApproverId = departmentManager.Id,
                                Status = ApprovalStatus.Pending,
                                CreatedAt = DateTime.Now,
                                Order = currentOrder++,
                                ApprovalLevelId = deptApprovalLevelId
                            });
                            _logger.LogInformation("Added Department Manager approval for RequestId: {RequestId}, Approver: {ApproverId} ({ApproverName}), Order: {Order}",
                                request.Id, departmentManager.Id, $"{departmentManager.FirstName} {departmentManager.LastName}", approvals.Last().Order);
                        }
                        else
                        {
                            _logger.LogWarning("Approval Level 'Department Manager' not found or inactive. Cannot add this approval step for RequestId: {RequestId}.", request.Id);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No active 'Department Manager' found for DepartmentId: {DepartmentId} (excluding requester {RequesterId}) for RequestId: {RequestId}. This step will be skipped.",
                            request.DepartmentId, request.RequestedByUserId, request.Id);
                    }
                }
                else
                {
                    _logger.LogWarning("Request {RequestId} has no DepartmentId. Skipping Department Manager approval step.", request.Id);
                }


                // Step 2: Finance Manager (for amounts > threshold, e.g., $1000)
                if (request.TotalAmount > 1000 && request.BranchId.HasValue)
                {
                    var financeManager = await GetFinanceManager(request.BranchId.Value);
                    if (financeManager != null)
                    {
                        var financeApprovalLevelId = await GetApprovalLevelId("Finance Manager");
                        if (financeApprovalLevelId > 0)
                        {
                            approvals.Add(new RequestApproval
                            {
                                PurchaseRequestId = request.Id,
                                ApproverId = financeManager.Id,
                                Status = ApprovalStatus.Pending,
                                CreatedAt = DateTime.Now,
                                Order = currentOrder++,
                                ApprovalLevelId = financeApprovalLevelId
                            });
                            _logger.LogInformation("Added Finance Manager approval for RequestId: {RequestId}, Approver: {ApproverId} ({ApproverName}), Order: {Order}",
                                request.Id, financeManager.Id, $"{financeManager.FirstName} {financeManager.LastName}", approvals.Last().Order);
                        }
                        else
                        {
                            _logger.LogWarning("Approval Level 'Finance Manager' not found or inactive. Cannot add this approval step for RequestId: {RequestId}.", request.Id);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No active 'Finance Manager' found for BranchId: {BranchId} for RequestId: {RequestId}. This step will be skipped.",
                            request.BranchId, request.Id);
                    }
                }
                else if (request.TotalAmount > 1000 && !request.BranchId.HasValue)
                {
                    _logger.LogWarning("Request {RequestId} has total amount > 1000 but no BranchId. Skipping Finance Manager approval step.", request.Id);
                }


                // Step 3: CEO (for high amounts, e.g., > $5000)
                if (request.TotalAmount > 5000)
                {
                    var ceo = await GetCEO();
                    if (ceo != null)
                    {
                        var ceoApprovalLevelId = await GetApprovalLevelId("CEO");
                        if (ceoApprovalLevelId > 0)
                        {
                            approvals.Add(new RequestApproval
                            {
                                PurchaseRequestId = request.Id,
                                ApproverId = ceo.Id,
                                Status = ApprovalStatus.Pending,
                                CreatedAt = DateTime.Now,
                                Order = currentOrder++,
                                ApprovalLevelId = ceoApprovalLevelId
                            });
                            _logger.LogInformation("Added CEO approval for RequestId: {RequestId}, Approver: {ApproverId} ({ApproverName}), Order: {Order}",
                                request.Id, ceo.Id, $"{ceo.FirstName} {ceo.LastName}", approvals.Last().Order);
                        }
                        else
                        {
                            _logger.LogWarning("Approval Level 'CEO' not found or inactive. Cannot add this approval step for RequestId: {RequestId}.", request.Id);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No active 'CEO' found for RequestId: {RequestId}. This step will be skipped.", request.Id);
                    }
                }

                // Add all approvals to context
                if (approvals.Any())
                {
                    _context.RequestApprovals.AddRange(approvals);
                    _logger.LogInformation("Created {Count} approvals for purchase request {RequestId}. First approver: {FirstApproverId}",
                        approvals.Count, request.Id, approvals.OrderBy(a => a.Order).First().ApproverId);
                }
                else
                {
                    _logger.LogWarning("No approvals created for purchase request {RequestId}. Total amount: {Amount}",
                        request.Id, request.TotalAmount);

                    // For requests with no required approvals (e.g., very small amounts), auto-approve
                    if (request.TotalAmount <= 1000) // This condition might need adjustment based on your actual approval level thresholds
                    {
                        _logger.LogInformation("Auto-approving request {RequestId} due to amount {Amount} being below all approval thresholds.",
                            request.Id, request.TotalAmount);
                        request.Status = RequestStatus.Approved;
                        request.ApprovedAt = DateTime.Now;
                        request.ApprovalComments = "Auto-approved - amount below approval thresholds.";
                    }
                    else
                    {
                        _logger.LogError("Request {RequestId} with TotalAmount {TotalAmount} has no approvals generated and is not auto-approved. It will be stuck.", request.Id, request.TotalAmount);
                        // Optionally, you might want to change the status to "Rejected" or "Error" here if no workflow could be generated
                        // request.Status = RequestStatus.Rejected;
                        // request.ApprovalComments = "No suitable approval workflow could be generated.";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FATAL ERROR during approval workflow creation for request {RequestId}: {ErrorMessage}", request.Id, ex.Message);
                throw; // Re-throw to handle in calling method (SubmitForApproval)
            }
        }
        private async Task<User> GetDepartmentManager(int departmentId, int requesterId)
        {
            // Get Department Manager from the same department, excluding the requester
            var departmentManagers = await _userManager.GetUsersInRoleAsync("Department Manager");

            return departmentManagers
                .Where(u => u.IsActive &&
                           u.DepartmentId == departmentId &&
                           u.Id != requesterId)
                .FirstOrDefault();
        }

        private async Task<User> GetFinanceManager(int branchId)
        {
            // Get Finance Manager from the same branch
            var financeManagers = await _userManager.GetUsersInRoleAsync("Finance Manager");

            return financeManagers
                .Where(u => u.IsActive && u.BranchId == branchId)
                .FirstOrDefault();
        }

        private async Task<User> GetCEO()
        {
            // Get the CEO (assuming there's only one)
            var ceos = await _userManager.GetUsersInRoleAsync("CEO");

            return ceos
                .Where(u => u.IsActive)
                .FirstOrDefault();
        }

        private async Task<int> GetApprovalLevelId(string roleName)
        {
            try
            {
                var approvalLevel = await _context.ApprovalLevels
                    .FirstOrDefaultAsync(al => al.RequiredRole == roleName && al.IsActive);

                if (approvalLevel == null)
                {
                    _logger.LogWarning("No active approval level found for role {RoleName}. Available levels: {@Levels}",
                        roleName,
                        await _context.ApprovalLevels.Select(al => new { al.RequiredRole, al.IsActive }).ToListAsync());
                    return 0;
                }

                return approvalLevel.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approval level ID for role {RoleName}", roleName);
                return 0;
            }
        }
        public async Task<IActionResult> DebugApprovals(int id)
        {
            var purchaseRequest = await _context.PurchaseRequests
                .Include(p => p.Approvals)
                    .ThenInclude(a => a.Approver)
                .Include(p => p.Approvals)
                    .ThenInclude(a => a.ApprovalLevel)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchaseRequest == null)
            {
                return Json(new { success = false, message = "Request not found" });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            var debugInfo = new
            {
                RequestId = id,
                CurrentUserId = currentUser.Id,
                CurrentUserName = $"{currentUser.FirstName} {currentUser.LastName}",
                CurrentUserRoles = userRoles,
                RequestStatus = purchaseRequest.Status.ToString(),
                TotalAmount = purchaseRequest.TotalAmount,
                Approvals = purchaseRequest.Approvals.Select(a => new
                {
                    Id = a.Id,
                    ApproverId = a.ApproverId,
                    ApproverName = $"{a.Approver?.FirstName} {a.Approver?.LastName}",
                    Status = a.Status.ToString(),
                    Order = a.Order,
                    ApprovalLevel = a.ApprovalLevel?.Name,
                    RequiredRole = a.ApprovalLevel?.RequiredRole,
                    IsCurrentUsersPendingApproval = a.ApproverId == currentUser.Id && a.Status == ApprovalStatus.Pending
                }).OrderBy(a => a.Order).ToList()
            };

            return Json(new { success = true, debug = debugInfo });
        }

        // 5. Add method to seed approval levels if they don't exist
        public async Task<IActionResult> SeedApprovalLevels()
        {
            try
            {
                var approvalLevels = new[]
                {
            new ApprovalLevel
            {
                Name = "Department Manager",
                Description = "Department level approval",
                MinAmount = 0,
                MaxAmount = 10000,
                RequiredRole = "Department Manager",
                Order = 1,
                IsActive = true
            },
            new ApprovalLevel
            {
                Name = "Finance Manager",
                Description = "Finance department approval",
                MinAmount = 1000,
                MaxAmount = 50000,
                RequiredRole = "Finance Manager",
                Order = 2,
                IsActive = true
            },
            new ApprovalLevel
            {
                Name = "CEO",
                Description = "Executive level approval",
                MinAmount = 5000,
                MaxAmount = null,
                RequiredRole = "CEO",
                Order = 3,
                IsActive = true
            }
        };

                foreach (var level in approvalLevels)
                {
                    var exists = await _context.ApprovalLevels
                        .AnyAsync(al => al.RequiredRole == level.RequiredRole);

                    if (!exists)
                    {
                        _context.ApprovalLevels.Add(level);
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Approval levels seeded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding approval levels");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 1. Add this method to get available approvers for a request
        [HttpGet]
        public async Task<IActionResult> GetAvailableApprovers(int requestId)
        {
            try
            {
                var request = await _context.PurchaseRequests
                    .Include(p => p.RequestedByUser)
                    .FirstOrDefaultAsync(p => p.Id == requestId);

                if (request == null)
                {
                    return Json(new { success = false, message = "Request not found" });
                }

                var approvalLevels = new List<object>();

                // Level 1: Department Manager (if not the requester)
                if (request.DepartmentId.HasValue)
                {
                    var departmentManagers = await _userManager.GetUsersInRoleAsync("Department Manager");
                    var availableDeptManagers = departmentManagers
                        .Where(u => u.IsActive &&
                                   u.DepartmentId == request.DepartmentId &&
                                   u.Id != request.RequestedByUserId)
                        .Select(u => new
                        {
                            Id = u.Id,
                            Name = $"{u.FirstName} {u.LastName}",
                            Title = "Department Manager",
                            Department = u.Department?.DepartmentName
                        })
                        .ToList();

                    if (availableDeptManagers.Any())
                    {
                        approvalLevels.Add(new
                        {
                            Level = "Department Manager",
                            Description = "Department level approval",
                            Required = true,
                            Approvers = availableDeptManagers
                        });
                    }
                }

                // Level 2: Finance Manager (for amounts > $1000)
                if (request.TotalAmount > 1000 && request.BranchId.HasValue)
                {
                    var financeManagers = await _userManager.GetUsersInRoleAsync("Finance Manager");
                    var availableFinanceManagers = financeManagers
                        .Where(u => u.IsActive && u.BranchId == request.BranchId)
                        .Select(u => new
                        {
                            Id = u.Id,
                            Name = $"{u.FirstName} {u.LastName}",
                            Title = "Finance Manager",
                            Branch = u.Branch?.BranchName
                        })
                        .ToList();

                    if (availableFinanceManagers.Any())
                    {
                        approvalLevels.Add(new
                        {
                            Level = "Finance Manager",
                            Description = $"Financial approval required for amounts over $1,000",
                            Required = true,
                            Approvers = availableFinanceManagers
                        });
                    }
                }

                // Level 3: CEO (for amounts > $5000)
                if (request.TotalAmount > 5000)
                {
                    var ceos = await _userManager.GetUsersInRoleAsync("CEO");
                    var availableCEOs = ceos
                        .Where(u => u.IsActive)
                        .Select(u => new
                        {
                            Id = u.Id,
                            Name = $"{u.FirstName} {u.LastName}",
                            Title = "CEO",
                            Branch = u.Branch?.BranchName
                        })
                        .ToList();

                    if (availableCEOs.Any())
                    {
                        approvalLevels.Add(new
                        {
                            Level = "CEO",
                            Description = $"Executive approval required for amounts over $5,000",
                            Required = true,
                            Approvers = availableCEOs
                        });
                    }
                }

                return Json(new
                {
                    success = true,
                    requestId = requestId,
                    totalAmount = request.TotalAmount,
                    approvalLevels = approvalLevels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available approvers for request {RequestId}", requestId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 2. Modified SubmitForApproval method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForApprovalWithSelection(int id, string selectedApprovers)
        {
            try
            {
                var purchaseRequest = await _context.PurchaseRequests
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (purchaseRequest == null)
                {
                    return Json(new { success = false, message = "Purchase request not found" });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (!await CanModifyRequest(purchaseRequest, currentUser))
                {
                    return Json(new { success = false, message = "You don't have permission to submit this request" });
                }

                if (purchaseRequest.Status != RequestStatus.Draft)
                {
                    return Json(new { success = false, message = "Only draft requests can be submitted for approval" });
                }

                if (!purchaseRequest.Items.Any())
                {
                    return Json(new { success = false, message = "Cannot submit request without items" });
                }

                // Parse selected approvers (JSON format: [{"level":"Department Manager","approverId":2}, ...])
                var approverSelections = System.Text.Json.JsonSerializer.Deserialize<List<ApproverSelection>>(selectedApprovers);

                // Update total amount
                await UpdateTotalAmount(purchaseRequest.Id);
                await _context.SaveChangesAsync();
                await _context.Entry(purchaseRequest).ReloadAsync();

                purchaseRequest.Status = RequestStatus.Submitted;
                purchaseRequest.UpdatedAt = DateTime.Now;

                // Create approval workflow with selected approvers
                await CreateApprovalWorkflowWithSelection(purchaseRequest, approverSelections);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Purchase request {RequestNumber} submitted for approval by user {UserId} with selected approvers",
                    purchaseRequest.RequestNumber, currentUser.Id);

                return Json(new
                {
                    success = true,
                    message = "Request submitted for approval successfully",
                    totalAmount = purchaseRequest.TotalAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting request {RequestId} for approval with selection", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 3. Helper class for approver selection
        public class ApproverSelection
        {
            public string Level { get; set; }
            public int ApproverId { get; set; }
        }

        // 4. Create workflow with selected approvers
        private async Task CreateApprovalWorkflowWithSelection(PurchaseRequest request, List<ApproverSelection> selections)
        {
            try
            {
                var approvals = new List<RequestApproval>();
                var currentOrder = 1;

                // Sort selections by predefined order
                var orderedSelections = selections.OrderBy(s => GetLevelOrder(s.Level)).ToList();

                foreach (var selection in orderedSelections)
                {
                    var approvalLevelId = await GetApprovalLevelId(selection.Level);
                    if (approvalLevelId > 0)
                    {
                        // Verify the selected approver is valid for this level
                        if (await IsValidApproverForLevel(selection.ApproverId, selection.Level, request))
                        {
                            approvals.Add(new RequestApproval
                            {
                                PurchaseRequestId = request.Id,
                                ApproverId = selection.ApproverId,
                                Status = ApprovalStatus.Pending,
                                CreatedAt = DateTime.Now,
                                Order = currentOrder++,
                                ApprovalLevelId = approvalLevelId
                            });

                            var approver = await _userManager.FindByIdAsync(selection.ApproverId.ToString());
                            _logger.LogInformation("Added {Level} approval for RequestId: {RequestId}, Approver: {ApproverId} ({ApproverName}), Order: {Order}",
                                selection.Level, request.Id, selection.ApproverId, $"{approver?.FirstName} {approver?.LastName}", currentOrder - 1);
                        }
                        else
                        {
                            _logger.LogWarning("Invalid approver {ApproverId} selected for level {Level} on request {RequestId}",
                                selection.ApproverId, selection.Level, request.Id);
                        }
                    }
                }

                if (approvals.Any())
                {
                    _context.RequestApprovals.AddRange(approvals);
                    _logger.LogInformation("Created {Count} approvals for purchase request {RequestId}",
                        approvals.Count, request.Id);
                }
                else
                {
                    // Auto-approve if no approvals needed
                    request.Status = RequestStatus.Approved;
                    request.ApprovedAt = DateTime.Now;
                    request.ApprovalComments = "Auto-approved - no approval levels required.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating approval workflow with selection for request {RequestId}", request.Id);
                throw;
            }
        }

        // 5. Helper methods
        private int GetLevelOrder(string level)
        {
            return level switch
            {
                "Department Manager" => 1,
                "Finance Manager" => 2,
                "CEO" => 3,
                _ => 999
            };
        }

        private async Task<bool> IsValidApproverForLevel(int approverId, string level, PurchaseRequest request)
        {
            var user = await _userManager.FindByIdAsync(approverId.ToString());
            if (user == null || !user.IsActive) return false;

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Contains(level)) return false;

            // Additional validation based on level
            return level switch
            {
                "Department Manager" => user.DepartmentId == request.DepartmentId && user.Id != request.RequestedByUserId,
                "Finance Manager" => user.BranchId == request.BranchId,
                "CEO" => true,
                _ => false
            };
        }
        private async Task<List<User>> GetApproversForLevel(ApprovalLevel level, PurchaseRequest request)
        {
            // Get users with the required role in the same department/branch
            var usersInRole = await _userManager.GetUsersInRoleAsync(level.RequiredRole);

            return usersInRole.Where(u => u.IsActive &&
                                         (level.RequiredRole == "Admin" || // Admins can approve any request
                                          u.DepartmentId == request.DepartmentId || // Same department
                                          u.BranchId == request.BranchId)) // Same branch
                             .ToList();
        }

        #endregion
    }
}