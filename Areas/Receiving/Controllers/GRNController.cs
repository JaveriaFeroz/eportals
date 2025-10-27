using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Common.Enums;
using ProcureToPay.Areas.Common.Services;
using ProcureToPay.Areas.Procurement.Models;
using ProcureToPay.Areas.Receiving.Models;
using ProcureToPay.Areas.Receiving.Models.DTOs;
using ProcureToPay.Areas.Receiving.Services;
using ProcureToPay.Areas.Receiving.ViewModels;
using ProcureToPay.Areas.UserManagement.Attributes;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Data;
using System.Security.Claims;
using static ProcureToPay.Areas.Procurement.Controllers.PurchaseOrderController;

namespace ProcureToPay.Areas.Receiving.Controllers
{
    [Area("Receiving")]
    [Authorize]
    public class GRNController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GRNController> _logger;
        private readonly IGRNService _grnService;
        private readonly IWorkflowService _workflowService;

        public GRNController(
            ApplicationDbContext context,
            ILogger<GRNController> logger,
            IGRNService grnService,
            IWorkflowService workflowService)
        {
            _context = context;
            _logger = logger;
            _grnService = grnService;
            _workflowService = workflowService;
        }

        public class WorkflowActionGRNResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public object? NextApprover { get; set; } // Can be User or null
        }

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
        public async Task<JsonResult> GetPreviousUser(int grnId)
        {
            _logger.LogInformation("🎯 GetPreviousUser called with ID: {GRNId}", grnId);

            try
            {
                if (grnId <= 0)
                {
                    _logger.LogWarning("❌ Invalid grnId: {GRNId}", grnId);
                    return Json(new { success = false, message = "Invalid GRN ID" });
                }

                var grn = await _context.GoodsReceiptNotes
                    .Include(pr => pr.CreatedByUser)
                    .FirstOrDefaultAsync(pr => pr.Id == grnId);

                if (grn == null)
                {
                    _logger.LogWarning("❌ GRN not found: {GRNId}", grnId);
                    return Json(new { success = false, message = "GRN not found." });
                }

                var currentUserId = GetCurrentUserId();
                _logger.LogInformation("🔍 Current user ID: {CurrentUserId}", currentUserId);

                // Get the most recent workflow history entry (excluding current user)
                var lastHistoryEntry = await _context.FormHistories
                    .Where(fh => fh.WorkFlowTypeId == grn.WorkFlowTypeId &&
                                 fh.FormId == grn.FormId &&
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
                if (grn.CreatedByUserId != currentUserId && grn.CreatedByUser != null)
                {
                    _logger.LogInformation("✅ Found original requester: {UserName}", grn.CreatedByUser.UserName);
                    return Json(new
                    {
                        success = true,
                        userId = grn.CreatedByUser.Id,
                        userName = grn.CreatedByUser.UserName,
                        email = grn.CreatedByUser.Email,
                        displayText = $"{grn.CreatedByUser.UserName} ({grn.CreatedByUser.Email}) - Original Requester"
                    });
                }

                _logger.LogInformation("⚠️ No previous user found for PR: {GRNId}", grn);
                return Json(new { success = false, message = "No previous user found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting previous user for grn {GRNId}", grnId);
                return Json(new { success = false, message = "An error occurred while retrieving the previous user.", error = ex.Message });
            }
        }

        [Route("Receiving/GRN/GetPreviousWorkflowUsers")]
        [HttpGet]
        public async Task<JsonResult> GetPreviousWorkflowUsers(int grnId)
        {
            System.Diagnostics.Debug.WriteLine($"Controller hit with ID: {grnId}");
            _logger.LogInformation("CONTROLLER HIT - Getting previous workflow users for grn {GRNId}", grnId);
            try
            {
                _logger.LogInformation("Getting previous workflow users for grn {GRNId}", grnId);

                var grn = await _context.GoodsReceiptNotes
                    .Include(pr => pr.CreatedByUser) // Include the requester
                    .FirstOrDefaultAsync(pr => pr.Id == grnId);

                if (grn == null)
                {
                    _logger.LogWarning("GRN {GRNId} not found", grnId);
                    return Json(new { success = false, message = "GRN not found." });
                }

                var currentUserId = GetCurrentUserId();
                _logger.LogInformation("Current user ID: {CurrentUserId}", currentUserId);

                // Get workflow history to find all unique users who have acted on this request
                var workflowHistory = await _context.FormHistories
                    .Where(fh => fh.WorkFlowTypeId == grn.WorkFlowTypeId &&
                                 fh.FormId == grn.FormId)
                    .Select(fh => new {
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
                if (grn.CreatedByUser != null && grn.CreatedByUserId != currentUserId)
                {
                    previousUsers.Add(new
                    {
                        value = grn.CreatedByUserId.ToString(),
                        text = $"{grn.CreatedByUser.UserName} ({grn.CreatedByUser.Email}) - Original Requester",
                        group = new { name = "Original Requester" }
                    });
                    _logger.LogInformation("Added original requester: {UserName}", grn.CreatedByUser.UserName);
                }

                // Add users from workflow history (excluding current user)
                var processedUserIds = new HashSet<int> { currentUserId };
                if (grn.CreatedByUserId != currentUserId)
                {
                    processedUserIds.Add(grn.CreatedByUserId);
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
                    .OrderBy(u => {
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
                _logger.LogError(ex, "Error getting previous workflow users for grn {GRNId}", grnId);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while retrieving previous users.",
                    error = ex.Message
                });
            }
        }


        // GET: Receiving/GRN
        [RequirePermission("GRN", "View")]
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            var currentUserName = GetCurrentUserName();
            var currentUserRoles = await GetUserRolesAsync(currentUserId);

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.CurrentUserName = currentUserName;
            ViewBag.CurrentUserRoles = currentUserRoles;

            // Step 1: Find all GRN IDs that the current user has created.
            var createdGrnIds = await _context.GoodsReceiptNotes
                .Where(grn => grn.CreatedByUserId == currentUserId)
                .Select(grn => grn.Id)
                .ToListAsync();

            // Step 2: Find all GRN IDs from FormHistory where the current user performed an action.
            var actedOnGrnIds = await _context.FormHistories
                .Where(fh => fh.WorkFlowTypeId == (short)WorkFlowType.GoodsReceiptNote && fh.ActionByUserId == currentUserId)
                .Select(fh => fh.FormId)
                .ToListAsync();

            // Step 3: Find all GRN IDs where the current user is the next approver (based on the last history entry).
            // This is the new part of the logic you requested.
            var assignedToUserGrnIds = await _context.FormHistories
                .Where(fh => fh.WorkFlowTypeId == (short)WorkFlowType.GoodsReceiptNote && fh.ToUserId == currentUserId)
                .Select(fh => fh.FormId)
                .ToListAsync();

            // Step 4: Combine all unique IDs from the three conditions.
            var relevantGrnIds = new HashSet<int>();
            foreach (var id in createdGrnIds)
            {
                relevantGrnIds.Add(id);
            }
            foreach (var id in actedOnGrnIds)
            {
                relevantGrnIds.Add(id);
            }
            foreach (var id in assignedToUserGrnIds)
            {
                relevantGrnIds.Add(id);
            }

            // Step 5: Query the GoodsReceiptNotes table using the consolidated list of IDs.
            var grns = await _context.GoodsReceiptNotes
                .Where(grn => relevantGrnIds.Contains(grn.Id))
                .Include(g => g.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .Include(g => g.CreatedByUser)
                .OrderByDescending(g => g.GRNNumber)
                .ToListAsync();

            var viewModel = new GRNIndexViewModel
            {
                GRNs = new List<GRNListItemViewModel>()
            };

            foreach (var grn in grns)
            {
                var currentStateName = await _workflowService.GetStateNameAsync(
                    grn.WorkFlowTypeId,
                    grn.StateId);

                viewModel.GRNs.Add(new GRNListItemViewModel
                {
                    Id = grn.Id,
                    POId = grn.PurchaseOrderId,
                    GRNNumber = grn.GRNNumber,
                    PONumber = grn.PurchaseOrder?.PONumber ?? "N/A",
                    SupplierName = grn.PurchaseOrder?.Supplier?.SupplierName ?? "N/A",
                    StateId = grn.StateId,
                    ReceiptDate = grn.ReceiptDate,
                    Owner = grn.Owner,
                    ReceivedByUserName = grn.ReceivedByUser?.UserName ?? "N/A",
                    TotalAmount = grn.TotalAmount,
                    ItemsCount = grn.Items?.Count ?? 0,
                    CreatedOn = grn.CreatedOn,
                    CurrentStateName = currentStateName
                });
            }

            return View(viewModel);
        }

        // GET: Receiving/GRN/Details/5
        [RequirePermission("GRN", "View")]
        public async Task<IActionResult> Details(int? id)
        {
            // 1. Validate the input ID
            if (id == null)
            {
                return NotFound();
            }

            // 2. Fetch the GoodsReceiptNote and all related data
            var grn = await _context.GoodsReceiptNotes
                .Include(g => g.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .Include(g => g.ReceivedByUser)
                .Include(g => g.CreatedByUser)
                .Include(g => g.Items)
                    .ThenInclude(grni => grni.PurchaseOrderItem)
                .FirstOrDefaultAsync(m => m.Id == id);

            // 3. Handle case where GRN is not found
            if (grn == null)
            {
                return NotFound();
            }

            // --- ADDED SECURITY CHECK ---
            // A. Get the current logged-in user's ID.
            var currentUserId = GetCurrentUserId();

            // B. Check if the current user is the creator or the current owner.
            // Assuming your GoodsReceiptNote model has a string 'Owner' that matches a user's `UserName`.
            //var ownerUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == grn.Owner);
            //var ownerId = ownerUser?.Id ?? 0;
            //bool isCreatorOrOwner = grn.CreatedByUserId == currentUserId || ownerId == currentUserId;

            //// C. Check if the user is in the workflow history.
            var historyEntities = await _workflowService.GetFormHistoryAsync(grn.WorkFlowTypeId, grn.FormId);

            //// Assuming the FormHistory model has 'ActionByUserId' and 'ToUserId'.
            //var isUserInHistory = historyEntities.Any(h => h.ActionByUserId == currentUserId || h.ToUserId == currentUserId);

            //// D. Deny access if the user is not authorized.
            //bool isAuthorizedToView = isCreatorOrOwner || isUserInHistory;

            //// E. Deny access if the user is not authorized.
            //if (!isAuthorizedToView) // <-- Check the simplified, comprehensive view permission
            //{
            //    TempData["Error"] = "You are not authorized to view this Goods Receipt Note.";
            //    return Forbid(); // Returns a 403 Forbidden status
            //}
            // --- END OF ADDED SECURITY CHECK ---


            // 4. Populate the ViewModel
            var viewModel = new GRNDetailsViewModel
            {
                Id = grn.Id,
                GRNNumber = grn.GRNNumber,
                ReceiptDate = grn.ReceiptDate,
                Remarks = grn.Remarks,
                TotalAmount = grn.Items?.Sum(item => item.TotalPrice) ?? 0,
                CreatedOn = grn.CreatedOn,
                CurrentStateName = await _workflowService.GetStateNameAsync(grn.WorkFlowTypeId, grn.StateId),
                StateId = grn.StateId,
                CreatedByUserId = grn.CreatedByUserId,
                CreatedByUser = grn.CreatedByUser?.UserName ?? "N/A",
                Owner = grn.Owner,
                ReceivedByUserName = grn.ReceivedByUser?.UserName ?? "N/A",
                ReceivedByUserEmail = grn.ReceivedByUser?.Email ?? "N/A",
                BillDCNo = grn.BillDCNo,
                BillDCDate = grn.BillDCDate,
                GRNVaryFromPO = grn.GRNVaryFromPO,
                GRNVaryRate = grn.GRNVaryRate,
                IsImported = grn.IsImported,
                CurrencyId = grn.CurrencyId,
                ExRate = grn.ExRate,
                PurchaseOrderId = grn.PurchaseOrderId,
                PONumber = grn.PurchaseOrder?.PONumber ?? "N/A",
                PODate = grn.PurchaseOrder?.PODate ?? DateTime.MinValue,
                POTotalAmount = grn.PurchaseOrder?.TotalAmount ?? 0,
                SupplierName = grn.PurchaseOrder?.Supplier?.SupplierName ?? "N/A",
                SupplierCode = grn.PurchaseOrder?.Supplier?.NTN ?? string.Empty,
                SupplierContactPerson = grn.PurchaseOrder?.Supplier?.SupplierName ?? string.Empty,
                SupplierPhone = grn.PurchaseOrder?.Supplier?.PhoneNo ?? string.Empty,
                SupplierEmail = grn.PurchaseOrder?.Supplier?.Email ?? string.Empty,
                WorkflowHistory = historyEntities.Select(h => new WorkflowHistoryViewModel
                {
                    ActionName = h.Action,
                    FromStateName = h.FromStateName,
                    ToStateName = h.ToStateName,
                    Comments = h.Comments,
                    ActionByUserName = h.ActionByUserName,
                    ToUserName = h.ToUserName,
                    ActionDate = h.ActionDate
                }).ToList(),
                Items = new List<GRNItemDetailsViewModel>()
            };

            // 5. Populate the list of items
            foreach (var grnItem in grn.Items)
            {
                if (grnItem.PurchaseOrderItem == null)
                {
                    _logger.LogWarning("PurchaseOrderItem for GRNItem Id {GRNItemId} was null. Skipping.", grnItem.Id);
                    continue;
                }

                var totalReceivedForPoItem = await _grnService.GetReceivedQuantityForPOItemAsync(grnItem.PurchaseOrderItem.Id);
                decimal remainingQuantityForPoItem = grnItem.PurchaseOrderItem.Quantity - totalReceivedForPoItem;
                if (remainingQuantityForPoItem < 0) remainingQuantityForPoItem = 0;

                viewModel.Items.Add(new GRNItemDetailsViewModel
                {
                    Id = grnItem.Id,
                    ItemName = grnItem.ItemName,
                    UnitOfMeasure = grnItem.PurchaseOrderItem.Unit ?? string.Empty,
                    Quantity = grnItem.ReceivedQuantity,
                    UnitPrice = grnItem.UnitPrice,
                    TotalPrice = grnItem.TotalPrice,
                    Remarks = grnItem.Remarks,
                    RetailPrice = grnItem.RetailPrice,
                    GSTonRP = grnItem.GSTonRP,
                    GSTRate = grnItem.GSTRate,
                    GSTAmount = grnItem.GSTAmount,
                    DiscRate = grnItem.DiscRate,
                    Narration = grnItem.Narration
                });
            }

            var currentUserName = GetCurrentUserName();
            var currentUserRoles = await GetUserRolesAsync(currentUserId);

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.CurrentUserName = currentUserName;
            ViewBag.CurrentUserRoles = currentUserRoles;

            return View(viewModel);
        }// GET: Receiving/GRN/CreateFromPO
        [RequirePermission("GRN", "Add")]
        public async Task<IActionResult> CreateFromPO()
        {
            var currentUserId = GetCurrentUserId();
            var eligiblePOs = await _grnService.GetEligiblePurchaseOrdersForGRNAsync(currentUserId);

            var viewModel = new CreateGRNFromPOViewModel
            {
                EligiblePOs = new List<EligiblePOViewModel>()
            };

            foreach (var po in eligiblePOs)
            {
                if (po.Items == null || !po.Items.Any())
                {
                    await _context.Entry(po).Collection(p => p.Items).LoadAsync();
                }

                var pendingItemsCount = 0;
                decimal pendingAmount = 0;

                foreach (var item in po.Items)
                {
                    var receivedSoFar = await _grnService.GetReceivedQuantityForPOItemAsync(item.Id);
                    if (item.Quantity > receivedSoFar)
                    {
                        pendingItemsCount++;
                        pendingAmount += (item.Quantity - receivedSoFar) * item.UnitPrice;
                    }
                }

                // Find the associated Purchase Request Number
                // PurchaseOrder model must have a navigation property or foreign key to PurchaseRequest
                // This example assumes a many-to-many relationship via PurchaseRequestOrderMapping
                var prMapping = await _context.PurchaseRequestOrderMappings
                    .FirstOrDefaultAsync(m => m.PurchaseOrderId == po.Id);

                string prNumber = "N/A";
                if (prMapping != null)
                {
                    var purchaseRequest = await _context.PurchaseRequests
                        .FirstOrDefaultAsync(pr => pr.Id == prMapping.PurchaseRequestId);

                    if (purchaseRequest != null)
                    {
                        prNumber = purchaseRequest.RequestNumber;
                    }
                }

                var poCurrentStateName = await _workflowService.GetStateNameAsync(
                    (short)WorkFlowType.PurchaseOrder,
                    po.StateId);

                viewModel.EligiblePOs.Add(new EligiblePOViewModel
                {
                    Id = po.Id,
                    prId = po.Items.FirstOrDefault()?.SourcePurchaseRequestItem?.PurchaseRequestId ?? 0,
                    PONumber = po.PONumber,
                    PRNumber = prNumber,
                    PODate = po.PODate,
                    Owner = po.Owner,
                    SupplierName = po.Supplier?.SupplierName ?? "N/A",
                    TotalAmount = po.TotalAmount,
                    CurrentStateName = poCurrentStateName,
                    PendingItemsCount = pendingItemsCount,
                    DepartmentName = po.DepartmentCode,
                    CreatedOn = po.CreatedOn,
                    Items = po.Items // Pass the items collection
                });
            }

            return View(viewModel);
        }

        // GET: Receiving/GRN/Create?poId=5
        [RequirePermission("GRN", "Add")]
        public async Task<IActionResult> Create(int? poId)
        {
            if (poId == null)
            {
                TempData["Error"] = "Please select a Purchase Order to create a Goods Receipt Note.";
                return RedirectToAction(nameof(CreateFromPO));
            }

            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Items)
                    .ThenInclude(poi => poi.SourcePurchaseRequestItem)
                .Include(po => po.Supplier)
                .FirstOrDefaultAsync(po => po.Id == poId);

            if (purchaseOrder == null)
            {
                TempData["Error"] = "Selected Purchase Order not found.";
                return RedirectToAction(nameof(CreateFromPO));
            }

            var createGRNViewModel = new CreateGRNViewModel
            {
                PurchaseOrderId = purchaseOrder.Id,
                PONumber = purchaseOrder.PONumber,
                SupplierName = purchaseOrder.Supplier?.SupplierName ?? "N/A",
                PODate = purchaseOrder.PODate,
                POTotalAmount = purchaseOrder.TotalAmount,
                ReceiptDate = DateTime.Now,
                ReceivedByUserId = GetCurrentUserId(),

                BillDCNo = null,
                BillDCDate = null,
                IsImported = false,
                CurrencyId = 1,
                ExRate = 1.0
            };

            foreach (var poItem in purchaseOrder.Items)
            {
               
                    createGRNViewModel.ReceivedItems.Add(new CreateGRNItemViewModel
                    {
                        PurchaseOrderItemId = poItem.Id,
                        ItemName = poItem.ItemName,
                        UnitOfMeasure = poItem.Unit,
                        POQuantity = poItem.Quantity,
                        UnitPrice = poItem.UnitPrice,
                        RemainingQuantity = poItem.UpdatedQuantity,
                        ReceivedQuantity = poItem.UpdatedQuantity,
                        RetailPrice = 0,
                        GSTonRP = false,
                        GSTRate = (double)poItem.GSTRate,
                        GSTAmount = poItem.GSTAmount,
                        Narration = poItem.Remarks,
                    });
                
            }

            var currentUser = await _context.Users.FindAsync(GetCurrentUserId());
            createGRNViewModel.ReceivedByUserName = currentUser?.UserName ?? "Unknown User";

            return View(createGRNViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("GRN", "Add")]
        public async Task<IActionResult> Create(CreateGRNRequest request)
        {
            var currentUserId = GetCurrentUserId();
            request.ReceivedByUserId = currentUserId;

            // Filter out items with a quantity of 0 or less from the request
            request.ReceivedItems = request.ReceivedItems.Where(item => item.Quantity > 0).ToList();

            if (!request.ReceivedItems.Any())
            {
                ModelState.AddModelError("", "You must receive at least one item with a quantity greater than 0.");
            }

            // Server-side validation loop
            foreach (var item in request.ReceivedItems)
            {
                var poItem = await _context.PurchaseOrderItems.FindAsync(item.PurchaseOrderItemId);
                if (poItem == null)
                {
                    ModelState.AddModelError("", $"Purchase Order Item with ID {item.PurchaseOrderItemId} not found.");
                    continue;
                }

                var remainingQuantity = poItem.UpdatedQuantity;

                if (item.Quantity > remainingQuantity)
                {
                    ModelState.AddModelError($"ReceivedItems[{item.PurchaseOrderItemId}].Quantity",
                        $"The received quantity for {poItem.ItemName} ({item.Quantity}) cannot exceed the remaining quantity of {remainingQuantity}.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var newGRN = await _grnService.CreateGRNAsync(request);

                    // Update the status and remaining quantity of each Purchase Order Item
                    foreach (var receivedItem in request.ReceivedItems)
                    {
                        var poItem = await _context.PurchaseOrderItems
                            .FirstOrDefaultAsync(poi => poi.Id == receivedItem.PurchaseOrderItemId);

                        if (poItem != null)
                        {
                            // 🎯 NEW LOGIC: Subtract the received quantity directly from the updated quantity
                            poItem.UpdatedQuantity -= receivedItem.Quantity;

                            // Check if the PO item is now fully completed
                            if (poItem.UpdatedQuantity <= 0)
                            {
                                poItem.IsCompleted = true;
                            }

                            _context.PurchaseOrderItems.Update(poItem);
                        }
                    }

                    // Check if the entire Purchase Order is completed
                    var purchaseOrder = await _context.PurchaseOrders
                        .Include(po => po.Items)
                        .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderId);

                    if (purchaseOrder != null && purchaseOrder.Items.All(item => item.IsCompleted))
                    {
                        purchaseOrder.IsCompleted = true;
                        _context.PurchaseOrders.Update(purchaseOrder);
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Goods Receipt Note {newGRN.GRNNumber} created successfully.";
                    return RedirectToAction(nameof(Details), new { id = newGRN.Id });
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Error"] = ex.Message;
                    _logger.LogWarning(ex, "Business rule violation during GRN creation for user {UserId}", currentUserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating GRN for user {UserId}", currentUserId);
                    TempData["Error"] = "An unexpected error occurred while creating the Goods Receipt Note. Please try again.";
                }
            }

            // If ModelState is not valid, re-populate the ViewModel with the request data
            // This part ensures that user input and validation errors are preserved.
            var poFromDb = await _context.PurchaseOrders
                .Include(po => po.Items)
                .Include(po => po.Supplier)
                .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderId);

            var viewModel = new CreateGRNViewModel
            {
                PurchaseOrderId = request.PurchaseOrderId,
                PONumber = poFromDb?.PONumber ?? "N/A",
                SupplierName = poFromDb?.Supplier?.SupplierName ?? "N/A",
                PODate = poFromDb?.PODate ?? DateTime.MinValue,
                POTotalAmount = poFromDb?.TotalAmount ?? 0,
                ReceiptDate = request.ReceiptDate,
                ReceivedByUserId = currentUserId,
                ReceivedItems = new List<CreateGRNItemViewModel>(),
                BillDCNo = request.BillDCNo,
                BillDCDate = request.BillDCDate,
                IsImported = request.IsImported,
                CurrencyId = request.CurrencyId,
                ExRate = request.ExRate
            };

            var currentUser = await _context.Users.FindAsync(GetCurrentUserId());
            viewModel.ReceivedByUserName = currentUser?.UserName ?? "Unknown User";

            if (poFromDb != null)
            {
                foreach (var poItem in poFromDb.Items)
                {
                    var previouslyReceivedQuantity = await _grnService.GetReceivedQuantityForPOItemAsync(poItem.Id);
                    var remainingQuantity = poItem.Quantity - previouslyReceivedQuantity;

                    if (remainingQuantity > 0)
                    {
                        var reqItem = request.ReceivedItems.FirstOrDefault(ri => ri.PurchaseOrderItemId == poItem.Id);

                        viewModel.ReceivedItems.Add(new CreateGRNItemViewModel
                        {
                            PurchaseOrderItemId = poItem.Id,
                            ItemName = poItem.ItemName,
                            UnitOfMeasure = poItem.Unit,
                            POQuantity = poItem.Quantity,
                            RemainingQuantity = remainingQuantity,
                            UnitPrice = poItem.UnitPrice,
                            ReceivedQuantity = reqItem?.Quantity ?? 0,
                            RetailPrice = reqItem?.RetailPrice ?? 0,
                            GSTonRP = reqItem?.GSTonRP ?? false,
                            GSTRate = reqItem?.GSTRate ?? 0,
                            GSTAmount = reqItem?.GSTAmount ?? 0,
                            DiscRate = reqItem?.DiscRate ?? 0,
                            Narration = reqItem?.Narration
                        });
                    }
                }
            }
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WorkflowActionGRN(int id, string action, string comments = "", int? returnToUserId = null, bool returnToPrevious = false, int? submitToUserId = null, int? approveToUserId = null)
        {
            var grn = await _context.GoodsReceiptNotes
                .Include(g => g.CreatedByUser)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grn == null) return NotFound();

            var currentUserId = GetCurrentUserId();

            bool canPerform = await _workflowService.CanUserPerformActionAsync(
                currentUserId, grn, action);

            if (!canPerform)
            {
                TempData["Error"] = $"You are not authorized to {action} this GRN.";
                return RedirectToAction(nameof(Details), new { id });
            }

            WorkflowActionGRNResult result;

            if (action.Equals("submit", StringComparison.OrdinalIgnoreCase))
            {
                if (!submitToUserId.HasValue)
                {
                    TempData["Error"] = "Please select a user to submit the GRN to.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var workflowResult = await _workflowService.ProcessWorkflowActionAsync(
                    grn, action, comments, currentUserId, submitToUserId);

                result = new WorkflowActionGRNResult
                {
                    Success = workflowResult.Success,
                    Message = workflowResult.Message,
                    NextApprover = workflowResult.NextApprover
                };
            }
            else if (action.Equals("approve", StringComparison.OrdinalIgnoreCase))
            {
                const int GM_ROLE_ID = 26;
                var currentUserRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == currentUserId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                bool isCurrentUserCEO = currentUserRoles.Contains(GM_ROLE_ID);

                if (!isCurrentUserCEO && !approveToUserId.HasValue)
                {
                    TempData["Error"] = "Please select a user to send the approval to or indicate that this is the final approval.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var workflowResult = await _workflowService.ProcessWorkflowActionAsync(
                    grn, action, comments, currentUserId, approveToUserId);

                result = new WorkflowActionGRNResult
                {
                    Success = workflowResult.Success,
                    Message = workflowResult.Message,
                    NextApprover = workflowResult.NextApprover
                };
            }
            else if (action.Equals("return", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(comments))
                {
                    TempData["Error"] = "Comments are required when returning a request.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                int targetUserId;

                if (returnToPrevious)
                {
                    var previousUserResult = await GetPreviousUserForReturn(grn);
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

                result = await ProcessReturnToSpecificUser(grn, targetUserId, comments, currentUserId);
            }
            else
            {
                var workflowResult = await _workflowService.ProcessWorkflowActionAsync(
                    grn, action, comments, currentUserId);

                result = new WorkflowActionGRNResult
                {
                    Success = workflowResult.Success,
                    Message = workflowResult.Message,
                    NextApprover = workflowResult.NextApprover
                };
            }

            if (result.Success)
            {
                _context.Update(grn);
                await _context.SaveChangesAsync();

                TempData["Success"] = result.Message;

                if (result.NextApprover != null)
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
        private async Task<PreviousUserResult> GetPreviousUserForReturn(GoodsReceiptNote grn)
        {
            var currentUserId = GetCurrentUserId();

            try
            {
                // First, try to get the most recent user from workflow history (excluding current user)
                var lastHistoryEntry = await _context.FormHistories
                    .Where(fh => fh.WorkFlowTypeId == grn.WorkFlowTypeId &&
                                 fh.FormId == grn.FormId &&
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
                if (grn.CreatedByUserId != currentUserId)
                {
                    var requester = await _context.Users.FindAsync(grn.CreatedByUserId);
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
                _logger.LogError(ex, "Error finding previous user for return on grn {GRNId}", grn.Id);
                return new PreviousUserResult
                {
                    Success = false,
                    Message = "An error occurred while finding the previous user."
                };
            }
        }
        private async Task<WorkflowActionGRNResult> ProcessReturnToSpecificUser(
            GoodsReceiptNote grn,
            int returnToUserId,
            string comments,
            int currentUserId)
        {
            var returnToUser = await _context.Users.FindAsync(returnToUserId);
            if (returnToUser == null)
            {
                return new WorkflowActionGRNResult
                {
                    Success = false,
                    Message = "Selected user not found."
                };
            }

            // Update purchase request state
            short fromStateId = grn.StateId;
            grn.StateId = WorkflowService.STATE_RETURNED;
            grn.Owner = returnToUser.UserName ?? returnToUser.Email;
            grn.SyncStatusWithState();
            grn.CurrentApprovalSequence = 0; // Reset approval sequence for returned items
            grn.Approved = false;
            grn.Rejected = false;
            grn.IsCompleted = false;

            // Add workflow history entry. Pass returnToUserId as toUserId.
            await _workflowService.AddFormHistoryAsync(
                grn.WorkFlowTypeId,
                grn.FormId,
                fromStateId, // The state *before* returning
                WorkflowService.STATE_RETURNED,
                "Returned", // More descriptive action for history
                comments,
                currentUserId,
                returnToUserId); // Explicitly pass the user ID to whom it's being returned

            return new WorkflowActionGRNResult
            {
                Success = true,
                Message = $"Purchase Request returned to {returnToUser.UserName} successfully.",
                NextApprover = returnToUser
            };
        }

        private bool PurchaseOrderExists(int id)
        {
            return _context.PurchaseOrders.Any(e => e.Id == id);
        }
    }
}
