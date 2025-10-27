using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Common.Enums; // For WorkFlowType enum
using ProcureToPay.Areas.Common.Services; // For WorkflowService
using ProcureToPay.Areas.Procurement.Enums;
using ProcureToPay.Areas.Procurement.Models;
using ProcureToPay.Areas.Procurement.Models.DTOs; // For CreatePOFromPRRequest DTO
using ProcureToPay.Areas.Procurement.Services; // For IPurchaseOrderService
using ProcureToPay.Areas.UserManagement.Attributes;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Data;
using ProcureToPay.Helpers;
using System.Security.Claims;
using System.Xml;
using static ProcureToPay.Areas.Procurement.Controllers.PurchaseRequestController;
using static ProcureToPay.Areas.Procurement.Models.PurchaseOrderItem;

namespace ProcureToPay.Areas.Procurement.Controllers
{
    [Area("Procurement")]
    [Authorize]
    public class PurchaseOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PurchaseOrderController> _logger;
        private readonly IWorkflowService _workflowService;
        private readonly IPurchaseOrderService _poService; // Inject PurchaseOrderService

        public PurchaseOrderController(
            ApplicationDbContext context,
            ILogger<PurchaseOrderController> logger,
            IWorkflowService workflowService,
            IPurchaseOrderService poService) // Inject IPurchaseOrderService
        {
            _context = context;
            _logger = logger;
            _workflowService = workflowService;
            _poService = poService; // Assign
        }

        public class WorkflowActionPOResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public object? NextApprover { get; set; } // Can be User or null
        }
        // Helper method to get current user ID
        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private string GetCurrentUserName()
        {
            // Returns the username (e.g., email or unique name) of the logged-in user
            return User.Identity.Name ?? string.Empty;
        }

        private async Task<List<int>> GetUserRolesAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<JsonResult> GetApprovers(int purchaseOrderId)
        {
            try
            {
                var purchaseOrder = await _context.PurchaseOrders.FindAsync(purchaseOrderId);
                if (purchaseOrder == null)
                {
                    return Json(new { success = false, message = "Purchase Order not found." });
                }

                // Get the ID of the current user who is initiating the action
                var currentUserId = GetCurrentUserId();

                // Initialize the next approval sequence
                int nextApprovalSeq;

                // 1. Get all roles for the current user.
                var currentUserRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == currentUserId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                if (currentUserRoles.Any())
                {
                    // 2. Find the highest approval sequence covered by the current user's roles.
                    var highestApprovalSequence = await _context.WorkFlowApprovalSequences
                        .Where(ws => ws.WorkFlowTypeId == purchaseOrder.WorkFlowTypeId &&
                                     ws.IsActive &&
                                     currentUserRoles.Contains(ws.RoleID))
                        .OrderByDescending(ws => ws.ApprovalSeq)
                        .Select(ws => (int?)ws.ApprovalSeq)
                        .FirstOrDefaultAsync();

                    if (highestApprovalSequence.HasValue)
                    {
                        // Set the next approval sequence to one level above the current user's highest role.
                        nextApprovalSeq = highestApprovalSequence.Value + 1;
                    }
                    else
                    {
                        // If the user's role is not found in the hierarchy, default to the first step.
                        nextApprovalSeq = 1;
                    }
                }
                else
                {
                    // If the user has no roles, default to the first step.
                    nextApprovalSeq = 1;
                }

                // Use the newly determined `nextApprovalSeq` to fetch the approvers
                var users = await _workflowService.GetUsersForApprovalSequenceAsync(purchaseOrder, nextApprovalSeq);

                if (!users.Any())
                {
                    return Json(new { success = false, message = "No approvers configured for the next step." });
                }

                var userList = users.Select(u => new
                {
                    value = u.Id,
                    text = $"{u.UserName} ({u.Email})"
                }).ToList();

                return Json(new { success = true, users = userList });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes.
                return Json(new { success = false, message = "An error occurred while fetching approvers." });
            }
        }


        [HttpGet]
        public async Task<JsonResult> GetPreviousUser(int purchaseOrderId)
        {
            _logger.LogInformation("🎯 GetPreviousUser called with ID: {PurchaseOrderId}", purchaseOrderId);

            try
            {
                if (purchaseOrderId <= 0)
                {
                    _logger.LogWarning("❌ Invalid purchaseOrderId: {PurchaseOrderId}", purchaseOrderId);
                    return Json(new { success = false, message = "Invalid Purchase Order ID" });
                }

                var purchaseOrder = await _context.PurchaseOrders
                    .Include(pr => pr.CreatedByUser)
                    .FirstOrDefaultAsync(pr => pr.Id == purchaseOrderId);

                if (purchaseOrder == null)
                {
                    _logger.LogWarning("❌ Purchase order not found: {PurchaseOrderId}", purchaseOrderId);
                    return Json(new { success = false, message = "Purchase order not found." });
                }

                var currentUserId = GetCurrentUserId();
                _logger.LogInformation("🔍 Current user ID: {CurrentUserId}", currentUserId);

                // Get the most recent workflow history entry (excluding current user)
                var lastHistoryEntry = await _context.FormHistories
                    .Where(fh => fh.WorkFlowTypeId == purchaseOrder.WorkFlowTypeId &&
                                 fh.FormId == purchaseOrder.FormId &&
                                 fh.ActionByUserId != currentUserId)
                    .OrderByDescending(fh => fh.ActionDate)
                    .FirstOrDefaultAsync();

                if (lastHistoryEntry != null)
                {
                    var previousUser = await _context.Users.FindAsync(lastHistoryEntry.ActionByUserId);
                    if (previousUser != null)
                    {
                        _logger.LogInformation("✅ Found previous user from history: {UserName}", previousUser.UserName);
                        return Json(new
                        {
                            success = true,
                            userId = previousUser.Id,
                            userName = previousUser.UserName,
                            email = previousUser.Email,
                            displayText = $"{previousUser.UserName} ({previousUser.Email}) - Last Action: {lastHistoryEntry.Action}"
                        });
                    }
                }

                // If no workflow history, fall back to the original requester (if not current user)
                if (purchaseOrder.CreatedByUserId != currentUserId && purchaseOrder.CreatedByUser != null)
                {
                    _logger.LogInformation("✅ Found original requester: {UserName}", purchaseOrder.CreatedByUser.UserName);
                    return Json(new
                    {
                        success = true,
                        userId = purchaseOrder.CreatedByUser.Id,
                        userName = purchaseOrder.CreatedByUser.UserName,
                        email = purchaseOrder.CreatedByUser.Email,
                        displayText = $"{purchaseOrder.CreatedByUser.UserName} ({purchaseOrder.CreatedByUser.Email}) - Original Requester"
                    });
                }

                _logger.LogInformation("⚠️ No previous user found for PR: {PurchaseOrderId}", purchaseOrderId);
                return Json(new { success = false, message = "No previous user found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting previous user for purchase order {PurchaseOrderId}", purchaseOrderId);
                return Json(new { success = false, message = "An error occurred while retrieving the previous user.", error = ex.Message });
            }
        }

        [Route("Procurement/PurchaseOrder/GetPreviousWorkflowUsers")]
        [HttpGet]
        public async Task<JsonResult> GetPreviousWorkflowUsers(int purchaseOrderId)
        {
            System.Diagnostics.Debug.WriteLine($"Controller hit with ID: {purchaseOrderId}");
            _logger.LogInformation("CONTROLLER HIT - Getting previous workflow users for purchase order {PurchaseOrderId}", purchaseOrderId);
            try
            {
                _logger.LogInformation("Getting previous workflow users for purchase order {PurchaseOrderId}", purchaseOrderId);

                var purchaseOrder = await _context.PurchaseOrders
                    .Include(pr => pr.CreatedByUser) // Include the requester
                    .FirstOrDefaultAsync(pr => pr.Id == purchaseOrderId);

                if (purchaseOrder == null)
                {
                    _logger.LogWarning("Purchase request {PurchaseOrderId} not found", purchaseOrderId);
                    return Json(new { success = false, message = "Purchase order not found." });
                }

                var currentUserId = GetCurrentUserId();
                _logger.LogInformation("Current user ID: {CurrentUserId}", currentUserId);

                // Get workflow history to find all unique users who have acted on this request
                var workflowHistory = await _context.FormHistories
                    .Where(fh => fh.WorkFlowTypeId == purchaseOrder.WorkFlowTypeId &&
                                 fh.FormId == purchaseOrder.FormId)
                    .Select(fh => new
                    {
                        fh.ActionByUserId,
                        fh.ActionByUserName,
                        fh.Action,
                        fh.ActionDate,
                        fh.ToUserId
                    })
                    .OrderBy(fh => fh.ActionDate)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} workflow history entries", workflowHistory.Count);

                // Create the response structure that matches what the JavaScript expects
                var previousUsers = new List<object>();

                // Start with the original requester (if not current user)
                if (purchaseOrder.CreatedByUser != null && purchaseOrder.CreatedByUserId != currentUserId)
                {
                    previousUsers.Add(new
                    {
                        value = purchaseOrder.CreatedByUserId.ToString(),
                        text = $"{purchaseOrder.CreatedByUser.UserName} ({purchaseOrder.CreatedByUser.Email}) - Original Requester",
                        group = new { name = "Original Requester" }
                    });
                    _logger.LogInformation("Added original requester: {UserName}", purchaseOrder.CreatedByUser.UserName);
                }

                // Add users from workflow history (excluding current user)
                var processedUserIds = new HashSet<int> { currentUserId };
                if (purchaseOrder.CreatedByUserId != currentUserId)
                {
                    processedUserIds.Add(purchaseOrder.CreatedByUserId);
                }

                foreach (var historyEntry in workflowHistory)
                {
                    // Add the ActionByUser if not already processed
                    if (historyEntry.ActionByUserId != currentUserId &&
                        !processedUserIds.Contains(historyEntry.ActionByUserId))
                    {
                        // Get full user details for better display
                        var actionUser = await _context.Users.FindAsync(historyEntry.ActionByUserId);
                        if (actionUser != null)
                        {
                            previousUsers.Add(new
                            {
                                value = historyEntry.ActionByUserId.ToString(),
                                text = $"{actionUser.UserName} ({actionUser.Email}) - {historyEntry.Action} on {historyEntry.ActionDate:dd/MM/yyyy}",
                                group = new { name = "Workflow History" }
                            });
                            processedUserIds.Add(historyEntry.ActionByUserId);
                            _logger.LogInformation("Added workflow user: {UserName} - {Action}", actionUser.UserName, historyEntry.Action);
                        }
                    }

                    // If there's a ToUserId (someone the request was sent to), add them too
                    if (historyEntry.ToUserId.HasValue &&
                        historyEntry.ToUserId.Value != currentUserId &&
                        !processedUserIds.Contains(historyEntry.ToUserId.Value))
                    {
                        var toUser = await _context.Users.FindAsync(historyEntry.ToUserId.Value);
                        if (toUser != null)
                        {
                            previousUsers.Add(new
                            {
                                value = toUser.Id.ToString(),
                                text = $"{toUser.UserName} ({toUser.Email}) - Previous Recipient",
                                group = new { name = "Previous Recipients" }
                            });
                            processedUserIds.Add(historyEntry.ToUserId.Value);
                            _logger.LogInformation("Added previous recipient: {UserName}", toUser.UserName);
                        }
                    }
                }

                // Sort users by group name, then by text
                var sortedUsers = previousUsers
                    .OrderBy(u =>
                    {
                        var group = u.GetType().GetProperty("group")?.GetValue(u);
                        return group?.GetType().GetProperty("name")?.GetValue(group)?.ToString() ?? "ZZZ";
                    })
                    .ThenBy(u => u.GetType().GetProperty("text")?.GetValue(u)?.ToString())
                    .ToList();

                _logger.LogInformation("Returning {Count} previous users for selection", sortedUsers.Count);

                return Json(new
                {
                    success = true,
                    users = sortedUsers,
                    message = $"Found {sortedUsers.Count} previous users"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting previous workflow users for purchase request {PurchaseOrderId}", purchaseOrderId);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while retrieving previous users.",
                    error = ex.Message
                });
            }
        }

        private async Task PopulatePODropdowns()
        {
            ViewBag.Suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.SupplierName
                }).ToListAsync();

            // Populate Branches
            ViewBag.Branches = await _context.Branches
                .Select(b => new SelectListItem
                {
                    Value = b.BranchId.ToString(),
                    Text = b.BranchName
                }).ToListAsync();

            // Populate Departments
            ViewBag.Departments = await _context.Departments
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName
                }).ToListAsync();
        }

        // Removed private GeneratePONumberAsync() as it's now in the service

        // GET: Procurement/PurchaseOrder (List all Purchase Orders)
        [RequirePermission("Purchase Order", "View")]
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            var currentUserName = GetCurrentUserName();
            var currentUserRoles = await GetUserRolesAsync(currentUserId);


            ViewBag.CurrentUserId = currentUserId;
            ViewBag.CurrentUserName = currentUserName;
            ViewBag.CurrentUserRoles = currentUserRoles;

            // Step 1: Find all PO IDs created by the current user.
            var createdPoIds = await _context.PurchaseOrders
                .Where(po => po.CreatedByUserId == currentUserId)
                .Select(po => po.Id)
                .ToListAsync();

            // Step 2: Find all PO IDs where the current user performed a workflow action.
            var actedOnPoIds = await _context.FormHistories
                .Where(fh => fh.WorkFlowTypeId == (short)WorkFlowType.PurchaseOrder && fh.ActionByUserId == currentUserId)
                .Select(fh => fh.FormId)
                .ToListAsync();

            // Step 3: Find all PO IDs where the current user is the next designated approver (ToUserId).
            var assignedToUserPoIds = await _context.FormHistories
                .Where(fh => fh.WorkFlowTypeId == (short)WorkFlowType.PurchaseOrder && fh.ToUserId == currentUserId)
                .Select(fh => fh.FormId)
                .ToListAsync();

            // Step 4: Combine all unique IDs from the three lists.
            var relevantPoIds = new HashSet<int>();
            foreach (var id in createdPoIds)
            {
                relevantPoIds.Add(id);
            }
            foreach (var id in actedOnPoIds)
            {
                relevantPoIds.Add(id);
            }
            foreach (var id in assignedToUserPoIds)
            {
                relevantPoIds.Add(id);
            }

            // Step 5: Query the PurchaseOrders table using the consolidated set of IDs.
            var purchaseOrders = await _context.PurchaseOrders
              .Where(po => relevantPoIds.Contains(po.Id))
              .Include(po => po.Items)
                  .ThenInclude(poi => poi.SourcePurchaseRequestItem)
                      .ThenInclude(pri => pri.PurchaseRequest) // Correctly include the PurchaseRequest
              .Include(po => po.Supplier)
              .Include(po => po.CreatedByUser)
              .OrderByDescending(po => po.PODate)
              .ToListAsync();

            return View(purchaseOrders);
        }

        [RequirePermission("Purchase Order", "View")]
        // GET: Procurement/PurchaseOrder/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.CreatedByUser)
                .Include(po => po.Department)
                .Include(po => po.Branch)
                .Include(po => po.Items)
                .ThenInclude(poi => poi.SourcePurchaseRequestItem)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (purchaseOrder == null) return NotFound();

            var currentUserId = GetCurrentUserId();

            // C. Check if the user is in the workflow history.
            var history = await _workflowService.GetFormHistoryAsync(
                purchaseOrder.WorkFlowTypeId,
                purchaseOrder.FormId);

            var purchaseRequestId = purchaseOrder.Items
                .FirstOrDefault(item => item.SourcePurchaseRequestItem != null)?
                .SourcePurchaseRequestItem?.PurchaseRequestId;

            // Use this ID to get the full PurchaseRequest for the view, if it exists.
            PurchaseRequest? originalPRForView = null;
            if (purchaseRequestId.HasValue)
            {
                originalPRForView = await _context.PurchaseRequests
                    .Include(pr => pr.Items)
                    .ThenInclude(item => item.UoM)
                    .FirstOrDefaultAsync(pr => pr.Id == purchaseRequestId.Value);
            }

            // Keep PR context for the view.
            ViewBag.PRRequestNumber = originalPRForView?.RequestNumber;
            ViewBag.PurchaseRequestId = originalPRForView?.Id; // Pass the ID for the link in the view

            ViewBag.WorkflowHistory = history;
            ViewBag.CurrentStateName = await _workflowService.GetStateNameAsync(
                purchaseOrder.WorkFlowTypeId,
                purchaseOrder.StateId);

            var currentUserName = GetCurrentUserName();
            var currentUserRoles = await GetUserRolesAsync(currentUserId);

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.CurrentUserName = currentUserName;
            ViewBag.CurrentUserRoles = currentUserRoles;

            return View(purchaseOrder);
        }

        // GET: Procurement/PurchaseOrder/CreateFromPR (Select PRs to create PO)
        // This is the starting point for creating a PO from an approved PR.
        [RequirePermission("Purchase Order", "Add")]
        public async Task<IActionResult> CreateFromPR()
        {
            var currentUserId = GetCurrentUserId();
            // Use the service to get eligible PRs
            var approvedPRs = await _poService.GetEligiblePurchaseRequestsAsync(currentUserId);

            if (approvedPRs != null)
            {
                approvedPRs = approvedPRs.OrderBy(pr => pr.RequestDate).ToList();
            }

            return View(approvedPRs);
        }

        // GET: Procurement/PurchaseOrder/Create?prId=5 (Actual PO creation form)
        [RequirePermission("Purchase Order", "Add")]
        public async Task<IActionResult> Create(int? prId)
        {
            if (prId == null)
            {
                TempData["Error"] = "Please select a Purchase Request to create a Purchase Order.";
                return RedirectToAction(nameof(CreateFromPR));
            }

            var purchaseRequest = await _context.PurchaseRequests
                .Include(pr => pr.Items)
            .ThenInclude(item => item.UoM)
                .Include(pr => pr.Department)
                .Include(pr => pr.Branch)

                .FirstOrDefaultAsync(pr => pr.Id == prId && pr.StateId == WorkflowService.STATE_APPROVED);

            if (purchaseRequest == null)
            {
                TempData["Error"] = "Selected Purchase Request is not found or not in an 'Approved' state.";
                return RedirectToAction(nameof(CreateFromPR));
            }

            await PopulatePODropdowns();

            ViewBag.PRRequestNumber = purchaseRequest.RequestNumber;
            ViewBag.SelectedPRId = prId;

            // Create the view model properly
            var newPOModel = new PurchaseOrder
            {
                PODate = DateTime.UtcNow,
                ExpectedDeliveryDate = DateTime.UtcNow.AddDays(14),
                Title = $"PO for PR {purchaseRequest.RequestNumber}",
                Description = purchaseRequest.Description,
                CreatedByUserId = GetCurrentUserId(),
                PurchaseNature = purchaseRequest.RequestNatureId.HasValue ?
                    (PurchaseNatureType)purchaseRequest.RequestNatureId.Value : PurchaseNatureType.Opex,
                PurchaseType = purchaseRequest.RequestTypeId.HasValue ?
                    (PurchaseItemType)purchaseRequest.RequestTypeId.Value : PurchaseItemType.Goods,
                DepartmentId = purchaseRequest.DepartmentId,
                BranchId = purchaseRequest.BranchId,
                Items = new List<PurchaseOrderItem>()
            };

            decimal initialTotalAmount = 0;

            // Add items with remaining quantities
            foreach (var prItem in purchaseRequest.Items)
            {
                var poItem = new PurchaseOrderItem
                {
                    SourcePurchaseRequestItemId = prItem.DetailId,
                    ItemName = prItem.ItemName,
                    Description = prItem.Narration,
                    Quantity = prItem.Quantity,
                    UnitPrice = prItem.UnitPrice,
                    TotalPrice = prItem.Quantity * prItem.UnitPrice,
                    Status = POItemStatus.Open,
                    Unit = prItem.UoM?.UoMName, // Null-conditional operator is important
                    UoMId = prItem.UoMId, // Assign the UoMId
                    TaxRate = 0,
                    TaxAmount = 0
                };

                newPOModel.Items.Add(poItem);
                initialTotalAmount += poItem.TotalPrice;
            }

            newPOModel.TotalAmount = initialTotalAmount;
            newPOModel.GrandTotal = initialTotalAmount; // Will be updated by JavaScript

            return View(newPOModel);
        }


        // POST: Procurement/PurchaseOrder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Purchase Order", "Add")]
        public async Task<IActionResult> Create(CreatePOFromPRRequest request)
        {

            var currentUserId = GetCurrentUserId();
            request.CreatedByUserId = currentUserId;

            request.SelectedItems = request.SelectedItems.Where(item => item.Quantity > 0).ToList();

            // Check if any items remain after filtering.
            // If there are no items left, the form is invalid.
            if (!request.SelectedItems.Any())
            {
                ModelState.AddModelError("", "You must order at least one item with a quantity greater than 0.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Call the service to handle the creation and all related logic.
                    // This is the core of your "successful creation logic."
                    var newPurchaseOrder = await _poService.CreatePOFromPRAsync(request);

                    // Add an entry to the workflow history for the PO creation.
                    // This ensures a record of the initial state change.
                    await _workflowService.AddFormHistoryAsync(
                        newPurchaseOrder.WorkFlowTypeId,
                        newPurchaseOrder.FormId,
                        0, // Assuming 0 is the previous state before creation.
                        WorkflowService.STATE_SAVED,
                        "Created",
                        "Purchase Order created from Purchase Request.",
                        currentUserId,
                        null
                    );

                    // Save the changes to the history.
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Purchase Order {newPurchaseOrder.PONumber} created successfully. It is now in Draft state and requires submission for approval.";
                    return RedirectToAction(nameof(Details), new { id = newPurchaseOrder.Id });
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Error"] = ex.Message;
                    _logger.LogWarning(ex, "Business rule violation during PO creation for user {UserId}", currentUserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating purchase order for user {UserId}", currentUserId);
                    TempData["Error"] = "An unexpected error occurred while creating the purchase order. Please try again.";
                }
            }

            // If we reach here, there was an error - repopulate dropdowns and return view.
            await PopulatePODropdowns();

            // CORRECTED QUERY: Eagerly load both Items and the nested UoM.
            var originalPRForView = await _context.PurchaseRequests
                .Include(pr => pr.Items)
                .ThenInclude(item => item.UoM) // This is the crucial line for the Unit of Measure fix.
                .FirstOrDefaultAsync(pr => pr.Id == request.PurchaseRequestId);

            // Keep PR context for the view.
            ViewBag.SelectedPRId = request.PurchaseRequestId;
            ViewBag.PRRequestNumber = originalPRForView?.RequestNumber;

            // Re-map the DTO back to a PurchaseOrder for the view.
            var poModelForView = new PurchaseOrder
            {
                Title = request.Title ?? $"PO from PR: {originalPRForView?.RequestNumber}",
                Description = request.Description,
                ExpectedDeliveryDate = request.ExpectedDeliveryDate,
                VendorId = (short)request.VendorId,
                SupplierAddress = request.VendorAddress,
                SupplierContact = request.VendorContact,
                PurchaseNature = request.PurchaseNature,
                PurchaseType = request.PurchaseType,
                TaxAmount = request.TaxAmount,
                Terms = request.Terms,
                PaymentDays = request.PaymentDays,
                Items = request.SelectedItems.Select(dtoItem => new PurchaseOrderItem
                {
                    SourcePurchaseRequestItemId = dtoItem.PurchaseRequestItemId,
                    ItemName = originalPRForView?.Items.FirstOrDefault(pi => pi.DetailId == dtoItem.PurchaseRequestItemId)?.ItemName ?? "Unknown",
                    Description = originalPRForView?.Items.FirstOrDefault(pi => pi.DetailId == dtoItem.PurchaseRequestItemId)?.Narration,
                    Quantity = dtoItem.Quantity,
                    UpdatedQuantity = dtoItem.Quantity,
                    UnitPrice = dtoItem.UnitPrice,
                    DiscRate = dtoItem.DiscRate,
                    DiscAmount = dtoItem.DiscAmount,
                    GSTRate = dtoItem.GSTRate,
                    GSTAmount = dtoItem.GSTAmount,
                    TotalPrice = dtoItem.Quantity * dtoItem.UnitPrice,
                    Status = POItemStatus.Open,
                    Unit = dtoItem.UoM?.UoMName, // Null-conditional operator is important
                    UoMId = dtoItem.UoMId, // Assign the UoMId
                }).ToList(),
                TotalAmount = request.SelectedItems.Sum(i => i.Quantity * i.UnitPrice),
                GrandTotal = request.SelectedItems.Sum(i => i.Quantity * i.UnitPrice) + request.TaxAmount
            };

            return View(poModelForView);
        }

        [HttpGet]
        public async Task<JsonResult> GetSupplierDetails(int supplierId)
        {
            var supplierDetails = await _context.Suppliers
                .Where(s => s.SupplierId == supplierId)
                .Select(s => new
                {
                    Address = s.Address,
                    Contact = s.MobileNo
                })
                .FirstOrDefaultAsync();

            if (supplierDetails == null)
            {
                return Json(new { success = false });
            }

            return Json(new
            {
                success = true,
                address = supplierDetails.Address,
                contact = supplierDetails.Contact
            });
        }
        [RequirePermission("Purchase Order", "Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Items)
                    .ThenInclude(item => item.UoM)
                .Include(po => po.Supplier)
                .Include(po => po.Department)
                .Include(po => po.Branch)
                .Include(po => po.PurchaseRequest) // Assuming a navigation property exists
                .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder == null) return NotFound();

            // Check if the PO is in an editable state (e.g., Saved or Returned).
            if (purchaseOrder.StateId != WorkflowService.STATE_SAVED &&
                purchaseOrder.StateId != WorkflowService.STATE_RETURNED)
            {
                TempData["Error"] = "Purchase Order cannot be edited in its current state.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var currentUserId = GetCurrentUserId();
            // Security check: Only the creator of the PO can edit it.
            if (purchaseOrder.CreatedByUserId != currentUserId)
            {
                TempData["Error"] = "You can only edit your own Purchase Orders.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var suppliers = await _context.Suppliers.ToListAsync();
            ViewBag.Suppliers = new SelectList(suppliers, "Id", "SupplierName", purchaseOrder.SupplierId);

            await PopulatePODropdowns();

            // Return a partial view for AJAX calls or the full view for regular requests.
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(purchaseOrder);
            }

            return View(purchaseOrder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Purchase Order", "Edit")]
        public async Task<IActionResult> Edit(int id, PurchaseOrder purchaseOrder)
        {
            if (id != purchaseOrder.Id)
            {
                return Json(new { success = false, message = "Invalid request ID." });
            }

            // Explicitly remove navigation properties from ModelState to prevent validation errors.
            ModelState.Remove("CreatedByUser");
            ModelState.Remove("UpdatedByUser");
            ModelState.Remove("Supplier");
            ModelState.Remove("Department");
            ModelState.Remove("Branch");
            ModelState.Remove("PurchaseRequest");
            ModelState.Remove("Title");

            if (purchaseOrder.Items != null)
            {
                for (int i = 0; i < purchaseOrder.Items.Count; i++)
                {
                    ModelState.Remove($"Items[{i}].PurchaseOrder");
                    ModelState.Remove($"Items[{i}].UoM");
                    ModelState.Remove($"Items[{i}].Unit");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPO = await _context.PurchaseOrders
                        .Include(po => po.Items)
                        .FirstOrDefaultAsync(po => po.Id == id);

                    if (existingPO == null)
                    {
                        return Json(new { success = false, message = "Purchase order not found." });
                    }

                    // Security check
                    if (existingPO.CreatedByUserId != GetCurrentUserId())
                    {
                        return Json(new { success = false, message = "You can only edit your own Purchase Orders." });
                    }

                    // Update main properties from the submitted model
                    existingPO.PODate = purchaseOrder.PODate;
                    existingPO.Remarks = purchaseOrder.Remarks;
                    existingPO.SupplierId = purchaseOrder.SupplierId;
                    existingPO.UpdatedByUserId = GetCurrentUserId();
                    existingPO.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                    // Efficiently update nested collection (PurchaseOrderItems)
                    var submittedItemIds = purchaseOrder.Items?.Select(i => i.Id).ToList() ?? new List<int>();
                    var existingItems = existingPO.Items.ToList();
                    var existingItemIds = existingItems.Select(i => i.Id).ToList();

                    // 1. Find and remove deleted items
                    var deletedItems = existingItems.Where(i => !submittedItemIds.Contains(i.Id)).ToList();
                    _context.PurchaseOrderItems.RemoveRange(deletedItems);

                    // 2. Find and update existing items, and add new items
                    if (purchaseOrder.Items != null)
                    {
                        foreach (var submittedItem in purchaseOrder.Items)
                        {
                            var existingItem = existingItems.FirstOrDefault(i => i.Id == submittedItem.Id);
                            if (existingItem != null)
                            {
                                // Item exists, update its properties
                                existingItem.ItemName = submittedItem.ItemName;
                                existingItem.Description = submittedItem.Description;
                                existingItem.Quantity = submittedItem.Quantity;
                                existingItem.UpdatedQuantity = submittedItem.Quantity;
                                existingItem.UnitPrice = submittedItem.UnitPrice;
                                existingItem.TotalPrice = submittedItem.TotalPrice;
                                existingItem.DiscRate = submittedItem.DiscRate;
                                existingItem.DiscAmount = submittedItem.DiscAmount;
                                existingItem.GSTRate = submittedItem.GSTRate;
                                existingItem.GSTAmount = submittedItem.GSTAmount;
                                existingItem.Unit = submittedItem.Unit;
                                _context.PurchaseOrderItems.Update(existingItem);
                            }
                            else
                            {
                                // New item, add it to the collection
                                existingPO.Items.Add(submittedItem);
                            }
                        }
                    }

                    existingPO.SyncStatusWithState();

                    // Recalculate totals
                    existingPO.TotalAmount = existingPO.Items?.Sum(i => i.TotalPrice) ?? 0;
                    existingPO.GrandTotal = existingPO.TotalAmount + existingPO.TaxAmount;

                    _context.Update(existingPO);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Purchase Order updated successfully.";
                    var redirectUrl = Url.Action(nameof(Details), new { id = purchaseOrder.Id });
                    return Json(new { success = true, message = "Purchase Order updated successfully.", redirectUrl });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return Json(new { success = false, message = "A concurrency error occurred. Please try again.", error = ex.Message });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "An error occurred while updating the purchase order.", error = ex.Message });
                }
            }

            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
            );

            return Json(new { success = false, message = "Validation failed. Please correct the errors.", errors });
        }
        // Action for handling PO workflow
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WorkflowActionPO(int id, string action, string comments = "", int? returnToUserId = null, bool returnToPrevious = false, int? submitToUserId = null, int? approveToUserId = null)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.CreatedByUser)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder == null) return NotFound();

            var currentUserId = GetCurrentUserId();

            bool canPerform = await _workflowService.CanUserPerformActionAsync(
                currentUserId, purchaseOrder, action);

            if (!canPerform)
            {
                TempData["Error"] = $"You are not authorized to {action} this Purchase Order.";
                return RedirectToAction(nameof(Details), new { id });
            }

            WorkflowActionPOResult result;

            if (action.Equals("submit", StringComparison.OrdinalIgnoreCase))
            {
                // This block remains correct. It handles validation and calls the service.
                if (!submitToUserId.HasValue)
                {
                    TempData["Error"] = "Please select a user to submit the request to.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var workflowResult = await _workflowService.ProcessWorkflowActionAsync(
                    purchaseOrder, action, comments, currentUserId, submitToUserId);

                result = new WorkflowActionPOResult
                {
                    Success = workflowResult.Success,
                    Message = workflowResult.Message,
                    NextApprover = workflowResult.NextApprover
                };
            }
            else if (action.Equals("approve", StringComparison.OrdinalIgnoreCase))
            {
                // REMOVED: Hardcoded GM_ROLE_ID check.
                // Instead of pre-validating here, let the service handle the next step logic.

                // Let the service determine if a next approver exists or if this is the final approval.
                var workflowResult = await _workflowService.ProcessWorkflowActionAsync(
                    purchaseOrder, action, comments, currentUserId, approveToUserId);

                result = new WorkflowActionPOResult
                {
                    Success = workflowResult.Success,
                    Message = workflowResult.Message,
                    NextApprover = workflowResult.NextApprover
                };
            }
            else if (action.Equals("return", StringComparison.OrdinalIgnoreCase))
            {
                // This block handles return logic, which is already correct.
                if (string.IsNullOrWhiteSpace(comments))
                {
                    TempData["Error"] = "Comments are required when returning a request.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                int targetUserId;

                if (returnToPrevious)
                {
                    var previousUserResult = await GetPreviousUserForReturn(purchaseOrder);
                    if (!previousUserResult.Success)
                    {
                        TempData["Error"] = previousUserResult.Message;
                        return RedirectToAction(nameof(Details), new { id });
                    }
                    targetUserId = previousUserResult.UserId;
                }
                else if (returnToUserId.HasValue)
                {
                    targetUserId = returnToUserId.Value;
                }
                else
                {
                    TempData["Error"] = "Please select a user to return the request to or use 'Return to Previous'.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                result = await ProcessReturnToSpecificUser(purchaseOrder, targetUserId, comments, currentUserId);
            }
            else
            {
                // This block handles other actions like 'cancel' or 'issue'.
                var workflowResult = await _workflowService.ProcessWorkflowActionAsync(
                    purchaseOrder, action, comments, currentUserId);

                result = new WorkflowActionPOResult
                {
                    Success = workflowResult.Success,
                    Message = workflowResult.Message,
                    NextApprover = workflowResult.NextApprover
                };
            }

            if (result.Success)
            {
                _context.Update(purchaseOrder);
                await _context.SaveChangesAsync();

                TempData["Success"] = result.Message;

                if (!string.IsNullOrEmpty(purchaseOrder.Owner) && purchaseOrder.Owner != "System" && purchaseOrder.Owner != (purchaseOrder.CreatedByUser?.UserName ?? "Unknown Creator"))
                {
                    TempData["Info"] = $"Next assignee: {purchaseOrder.Owner}";
                }
                else if (result.NextApprover != null)
                {
                    var nextApproverName = (result.NextApprover as User)?.UserName;
                    if (!string.IsNullOrEmpty(nextApproverName))
                    {
                        TempData["Info"] = $"Next assignee: {nextApproverName}";
                    }
                }
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private class PreviousUserResult
        {
            public bool Success { get; set; }
            public int UserId { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        /// <summary>
        /// Gets the most appropriate previous user for return action.
        /// </summary>
        private async Task<PreviousUserResult> GetPreviousUserForReturn(PurchaseOrder purchaseOrder)
        {
            var currentUserId = GetCurrentUserId();

            try
            {
                // First, try to get the most recent user from workflow history (excluding current user)
                var lastHistoryEntry = await _context.FormHistories
                    .Where(fh => fh.WorkFlowTypeId == purchaseOrder.WorkFlowTypeId &&
                                 fh.FormId == purchaseOrder.FormId &&
                                 fh.ActionByUserId != currentUserId)
                    .OrderByDescending(fh => fh.ActionDate)
                    .FirstOrDefaultAsync();

                if (lastHistoryEntry != null)
                {
                    var previousUser = await _context.Users.FindAsync(lastHistoryEntry.ActionByUserId);
                    if (previousUser != null)
                    {
                        return new PreviousUserResult
                        {
                            Success = true,
                            UserId = previousUser.Id,
                            Message = $"Returning to {previousUser.UserName} (last action: {lastHistoryEntry.Action})"
                        };
                    }
                }

                // If no workflow history, fall back to the original requester (if not current user)
                if (purchaseOrder.CreatedByUserId != currentUserId)
                {
                    var requester = await _context.Users.FindAsync(purchaseOrder.CreatedByUserId);
                    if (requester != null)
                    {
                        return new PreviousUserResult
                        {
                            Success = true,
                            UserId = requester.Id,
                            Message = $"Returning to original requester: {requester.UserName}"
                        };
                    }
                }

                return new PreviousUserResult
                {
                    Success = false,
                    Message = "No suitable previous user found for return."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding previous user for return on purchase request {PurchaseRequestId}", purchaseOrder.Id);
                return new PreviousUserResult
                {
                    Success = false,
                    Message = "An error occurred while finding the previous user."
                };
            }
        }
        private async Task<WorkflowActionPOResult> ProcessReturnToSpecificUser(
      PurchaseOrder purchaseOrder, // Model is PurchaseOrder
      int returnToUserId,
      string comments,
      int currentUserId)
        {
            var returnToUser = await _context.Users.FindAsync(returnToUserId);
            if (returnToUser == null)
            {
                return new WorkflowActionPOResult
                {
                    Success = false,
                    Message = "Selected user not found."
                };
            }

            // 1. Get the return-to user's roles
            var targetUserRoles = await _context.UserRoles
                .Where(ur => ur.UserId == returnToUserId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            // 2. Find the highest Approval Sequence the target user holds in this workflow
            var targetUserHighestApprovalSequence = await _context.WorkFlowApprovalSequences
                .Where(ws => ws.WorkFlowTypeId == purchaseOrder.WorkFlowTypeId &&
                             ws.IsActive &&
                             targetUserRoles.Contains(ws.RoleID))
                .OrderByDescending(ws => ws.ApprovalSeq)
                .Select(ws => (int?)ws.ApprovalSeq)
                .FirstOrDefaultAsync();

            // 3. Determine the sequence the PO should be returned to.
            // If roles are found, set it to the highest sequence. If not, set to 0 for a full restart.
            int returnedToSequence = targetUserHighestApprovalSequence.HasValue
                ? targetUserHighestApprovalSequence.Value
                : 0;

            // Update PO state
            short fromStateId = purchaseOrder.StateId;
            purchaseOrder.StateId = WorkflowService.STATE_RETURNED;
            purchaseOrder.Owner = returnToUser.UserName ?? returnToUser.Email;

            // ⭐ FIX: Set CurrentApprovalSequence based on the target user's role hierarchy
            purchaseOrder.CurrentApprovalSequence = returnedToSequence;

            purchaseOrder.SyncStatusWithState();
            purchaseOrder.Approved = false;
            purchaseOrder.Rejected = false;
            purchaseOrder.IsCompleted = false;

            // Add workflow history entry.
            await _workflowService.AddFormHistoryAsync(
                purchaseOrder.WorkFlowTypeId,
                purchaseOrder.FormId,
                fromStateId,
                WorkflowService.STATE_RETURNED,
                "Returned",
                comments,
                currentUserId,
                returnToUserId);

            return new WorkflowActionPOResult
            {
                Success = true,
                Message = $"Purchase Order returned to {returnToUser.UserName} successfully. Approval sequence reset to {returnedToSequence}.",
                NextApprover = returnToUser
            };
        }

        private bool PurchaseOrderExists(int id)
        {
            return _context.PurchaseOrders.Any(e => e.Id == id);
        }


        // GET: Procurement/PurchaseOrder/Reorder/5
        [RequirePermission("Purchase Order", "Add")]
        public async Task<IActionResult> Reorder(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Invalid Purchase Order ID.";
                return RedirectToAction(nameof(Index));
            }

            var originalPO = await _context.PurchaseOrders
                .Include(po => po.Items)
                    .ThenInclude(item => item.SourcePurchaseRequestItem)
                        .ThenInclude(pri => pri.UoM)
                // Ensure SourcePurchaseRequestItem and its nested PurchaseRequest is included
                .Include(po => po.Items)
                    .ThenInclude(item => item.SourcePurchaseRequestItem)
                        .ThenInclude(pri => pri.PurchaseRequest) // <-- PR navigation property included

                .Include(po => po.Supplier)
                .Include(po => po.Department)
                .Include(po => po.Branch)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (originalPO == null)
            {
                TempData["Error"] = "Purchase Order not found.";
                return RedirectToAction(nameof(Index));
            }

            // Security check - only allow reordering of approved/issued/completed POs
            if (originalPO.StateId != WorkflowService.STATE_APPROVED &&
                originalPO.StateId != WorkflowService.STATE_ISSUED &&
                originalPO.StateId != WorkflowService.STATE_COMPLETED)
            {
                TempData["Error"] = "Only approved, issued, or completed Purchase Orders can be reordered.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await PopulatePODropdowns();

            // --- PR NUMBER EXTRACTION LOGIC ---
            var originalPRNumber = originalPO.Items
                .FirstOrDefault(item => item.SourcePurchaseRequestItem?.PurchaseRequest?.RequestNumber != null)?
                .SourcePurchaseRequestItem?.PurchaseRequest?.RequestNumber;

            // Set ViewBag for PR Number
            ViewBag.PRRequestNumber = originalPRNumber;
            // ------------------------------------
            decimal initialTotalBaseAmount = 0;
            decimal initialTotalGSTAmount = 0;
            // Create a new PO model based on the original
            var reorderPOModel = new PurchaseOrder
            {
                // New Fields
                PODate = DateTime.UtcNow,
                ExpectedDeliveryDate = DateTime.UtcNow.AddDays(14),
                CreatedByUserId = GetCurrentUserId(),

                // Copied Fields
                Title = $"Reorder of {originalPO.PONumber}",
                Description = $"Reorder from PO: {originalPO.PONumber}. Original Description: {originalPO.Description}",
                PurchaseNature = originalPO.PurchaseNature,
                PurchaseType = originalPO.PurchaseType,
                DepartmentId = originalPO.DepartmentId,
                BranchId = originalPO.BranchId,
                VendorId = originalPO.VendorId,
                SupplierId = originalPO.SupplierId,
                SupplierAddress = originalPO.SupplierAddress,
                SupplierContact = originalPO.SupplierContact,
                Terms = originalPO.Terms,
                PaymentDays = originalPO.PaymentDays,
                Items = new List<PurchaseOrderItem>()
            };

            foreach (var originalItem in originalPO.Items)
            {
                // Base Calculation
                decimal basePrice = originalItem.Quantity * originalItem.UnitPrice;
                decimal discAmount = originalItem.DiscAmount ?? 0;
                decimal gstAmount = originalItem.GSTAmount ?? 0;

                // Item TotalPrice (Base - Disc + GST)
                decimal itemFinalPrice = basePrice - discAmount + gstAmount;

                var reorderItem = new PurchaseOrderItem
                {
                    ItemName = originalItem.ItemName,
                    Description = originalItem.Description,
                    Quantity = originalItem.Quantity,
                    UpdatedQuantity = originalItem.Quantity,
                    UnitPrice = originalItem.UnitPrice,

                    // Item TotalPrice: Yahan Final Price Set karen
                    TotalPrice = itemFinalPrice,

                    Status = POItemStatus.Open,
                    Unit = originalItem.Unit,
                    UoMId = originalItem.UoMId,
                    TaxRate = originalItem.TaxRate,
                    TaxAmount = originalItem.TaxAmount, // Keep this if used for other Item-level tax
                    DiscRate = originalItem.DiscRate,
                    DiscAmount = discAmount,
                    GSTRate = originalItem.GSTRate,
                    GSTAmount = gstAmount,
                    Remarks = originalItem.Remarks,
                    SourcePurchaseRequestItemId = originalItem.SourcePurchaseRequestItemId,
                };

                reorderPOModel.Items.Add(reorderItem);

                // Accumulate Totals
                initialTotalBaseAmount += (basePrice - discAmount);
                initialTotalGSTAmount += gstAmount;
            }

            reorderPOModel.TotalAmount = initialTotalBaseAmount;
            reorderPOModel.GrandTotal = initialTotalBaseAmount + initialTotalGSTAmount;

            ViewBag.OriginalPONumber = originalPO.PONumber;
            ViewBag.OriginalPOId = originalPO.Id;
            ViewBag.IsReorder = true;

            TempData["Info"] = $"Reordering from {originalPO.PONumber}. Review and adjust quantities as needed.";

            return View("CreateReorder", reorderPOModel);
        }

        // POST: Procurement/PurchaseOrder/CreateReorder
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Purchase Order", "Add")]
        public async Task<IActionResult> CreateReorder(
    PurchaseOrder purchaseOrder,
    int originalPOId,
    string supplierAddress,
    string SupplierContact,
    string description,
    int? departmentId,
    int? branchId)
        {
            var currentUserId = GetCurrentUserId();
            purchaseOrder.CreatedByUserId = currentUserId;

            // 💡 FIX: Manually map the captured data back to the object.
            purchaseOrder.Description = description;
            purchaseOrder.DepartmentId = departmentId;
            purchaseOrder.BranchId = branchId;

            // Remove items with 0 quantity
            purchaseOrder.Items = purchaseOrder.Items.Where(item => item.Quantity > 0).ToList();

            if (!purchaseOrder.Items.Any())
            {
                ModelState.AddModelError("", "You must order at least one item with a quantity greater than 0.");
            }

            // Remove only necessary navigation properties 
            ModelState.Remove("CreatedByUser");
            ModelState.Remove("UpdatedByUser");

            if (purchaseOrder.Items != null)
            {
                for (int i = 0; i < purchaseOrder.Items.Count; i++)
                {
                    ModelState.Remove($"Items[{i}].PurchaseOrder");
                    ModelState.Remove($"Items[{i}].UoM");
                    ModelState.Remove($"Items[{i}].Unit");
                }
            }

            // IMPORTANT: VendorId ko SupplierId mein map karein (Foreign Key consistency)
            purchaseOrder.SupplierId = purchaseOrder.VendorId;

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Load Original PO Items to get Source PR IDs (Needed if items were removed/re-indexed in the view)
                    var originalPOWithItems = await _context.PurchaseOrders
                        .Include(po => po.Items)
                        .FirstOrDefaultAsync(po => po.Id == originalPOId);

                    // 2. Map Source PR IDs from Original PO Items to New PO Items
                    if (originalPOWithItems != null)
                    {
                        foreach (var newItem in purchaseOrder.Items)
                        {
                            // Find the matching item in the original PO (assuming ItemName & UoM is the key)
                            var matchingOriginalItem = originalPOWithItems.Items
                                .FirstOrDefault(oi => oi.ItemName == newItem.ItemName && oi.UoMId == newItem.UoMId);

                            if (matchingOriginalItem != null)
                            {
                                // Copy the Source PR Item ID to the new PO Item
                                newItem.SourcePurchaseRequestItemId = matchingOriginalItem.SourcePurchaseRequestItemId;
                            }

                            newItem.UpdatedQuantity = newItem.Quantity;
                        }
                    }


                    // 3. Complete PO setup and Save
                    purchaseOrder.PONumber = await _poService.GenerateOrderNumberAsync();
                    purchaseOrder.PODate = DateTimeHelper.GetPakistanStandardTime();
                    purchaseOrder.CreatedOn = DateTimeHelper.GetPakistanStandardTime();

                    // Set workflow properties
                    purchaseOrder.StateId = WorkflowService.STATE_SAVED;
                    purchaseOrder.Status = PurchaseOrderStatus.Draft;
                    purchaseOrder.WorkFlowTypeId = (short)ProcureToPay.Areas.Common.Enums.WorkFlowType.PurchaseOrder;

                    var currentUser = await _context.Users.FindAsync(currentUserId);
                    purchaseOrder.Owner = currentUser?.UserName ?? "System";

                    // Set department and branch codes
                    if (purchaseOrder.DepartmentId.HasValue)
                    {
                        var dept = await _context.Departments.FindAsync(purchaseOrder.DepartmentId.Value);
                        purchaseOrder.DepartmentCode = dept?.DepartmentCode ?? "";
                    }
                    if (purchaseOrder.BranchId.HasValue)
                    {
                        var branch = await _context.Branches.FindAsync(purchaseOrder.BranchId.Value);
                        purchaseOrder.BranchCode = branch?.BranchCode ?? "";
                    }

                    // Recalculate totals
                    purchaseOrder.TotalAmount = purchaseOrder.Items.Sum(i => (i.Quantity * i.UnitPrice) - (i.DiscAmount ?? 0));

                    // Step B: Calculate Total Item GST
                    decimal totalItemGST = purchaseOrder.Items.Sum(i => i.GSTAmount ?? 0);

                    // Step C: GrandTotal (Final amount with all taxes/fees)
                    // GrandTotal = TotalAmount (Net Base) + Total Item GST + PO Level Tax (purchaseOrder.TaxAmount)
                    purchaseOrder.GrandTotal = purchaseOrder.TotalAmount + totalItemGST;

                    _context.PurchaseOrders.Add(purchaseOrder);
                    await _context.SaveChangesAsync();

                    // Add workflow history
                    await _workflowService.AddFormHistoryAsync(
                        purchaseOrder.WorkFlowTypeId,
                        purchaseOrder.FormId,
                        0,
                        WorkflowService.STATE_SAVED,
                        "Created",
                        "Purchase Order created as a reorder.",
                        currentUserId,
                        null
                    );

                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Purchase Order {purchaseOrder.PONumber} created successfully from reorder.";
                    return RedirectToAction(nameof(Details), new { id = purchaseOrder.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating reorder PO for user {UserId}", currentUserId);
                    TempData["Error"] = "An unexpected error occurred while creating the purchase order. Please try again.";
                }
            }

            // 🛑 Error Handling / Model Invalid (Fallback logic)
            await PopulatePODropdowns();
            ViewBag.IsReorder = true;

            // FIX: Fetch Original PO Number and PR Number to keep the banner intact
            var originalPO = await _context.PurchaseOrders
                .Include(po => po.Items).ThenInclude(i => i.SourcePurchaseRequestItem).ThenInclude(pr => pr.PurchaseRequest)
                .FirstOrDefaultAsync(po => po.Id == originalPOId);

            ViewBag.OriginalPONumber = originalPO?.PONumber ?? "N/A";

            var originalPRNumber = originalPO?.Items
                .FirstOrDefault(item => item.SourcePurchaseRequestItem?.PurchaseRequest?.RequestNumber != null)?
                .SourcePurchaseRequestItem?.PurchaseRequest?.RequestNumber;
            ViewBag.PRRequestNumber = originalPRNumber;

            return View("CreateReorder", purchaseOrder);
        }
    }
}