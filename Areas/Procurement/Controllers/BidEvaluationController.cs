using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Common.Enums;
using ProcureToPay.Areas.Common.Services;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Procurement.Models;
using ProcureToPay.Areas.Procurement.Services;
using ProcureToPay.Areas.Procurement.ViewModels;
using ProcureToPay.Areas.UserManagement.Attributes;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Data;
using ProcureToPay.Helpers;
using System.Security.Claims;

namespace ProcureToPay.Areas.Procurement.Controllers
{
    [Area("Procurement")]
    // [Authorize]
    public class BidEvaluationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowService _workflowService;
        private readonly IProductService _productService;
        private readonly IPurchaseOrderService _poService;
        private readonly ILogger<BidEvaluationController> _logger;

        public BidEvaluationController(
            ApplicationDbContext context,
            IWorkflowService workflowService,
            IProductService productService,
            IPurchaseOrderService poService,
        ILogger<BidEvaluationController> logger)
        {
            _context = context;
            _workflowService = workflowService;
            _productService = productService;
            _poService = poService;
            _logger = logger;
        }

        public class WorkflowActionBEResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public object? NextApprover { get; set; } // Can be User or null
        }
        private int GetCurrentUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            return 0;
        }

        private string GetCurrentUserName()
        {
            return User.Identity?.Name ?? string.Empty;
        }

        private async Task<List<int>> GetUserRolesAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<JsonResult> GetApprovers(int bidEvaluationId)
        {
            try
            {
                // 1. Rename variable and find the Bid Evaluation instead of Purchase Order
                var bidEvaluation = await _context.BidEvaluations.FindAsync(bidEvaluationId);
                if (bidEvaluation == null)
                {
                    // 2. Update error message
                    return Json(new { success = false, message = "Bid Evaluation not found." });
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
                        // 3. Use the WorkFlowTypeId from the Bid Evaluation object
                        .Where(ws => ws.WorkFlowTypeId == bidEvaluation.WorkFlowTypeId &&
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
                // 4. Pass the Bid Evaluation object to the service method
                var users = await _workflowService.GetUsersForApprovalSequenceAsync(bidEvaluation, nextApprovalSeq);

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
        public async Task<JsonResult> GetPreviousUser(int bidEvaluationId)
        {
            _logger.LogInformation("🎯 GetPreviousUser called with ID: {BidEvaluationId}", bidEvaluationId);

            try
            {
                if (bidEvaluationId <= 0)
                {
                    _logger.LogWarning("❌ Invalid bidEvaluationId: {BidEvaluationId}", bidEvaluationId);
                    return Json(new { success = false, message = "Invalid Bid Evaluation ID" });
                }

                var bidEvaluation = await _context.BidEvaluations
                    .Include(be => be.CreatedByUser)
                    .FirstOrDefaultAsync(be => be.BidNo == bidEvaluationId);

                if (bidEvaluation == null)
                {
                    _logger.LogWarning("❌ Bid evaluation not found: {BidEvaluationId}", bidEvaluationId);
                    return Json(new { success = false, message = "Bid evaluation not found." });
                }

                var currentUserId = GetCurrentUserId();
                _logger.LogInformation("🔍 Current user ID: {CurrentUserId}", currentUserId);

                // Get the most recent workflow history entry (excluding current user)
                var lastHistoryEntry = await _context.FormHistories
                    .Where(fh => fh.WorkFlowTypeId == bidEvaluation.WorkFlowTypeId &&
                                 fh.FormId == bidEvaluation.FormId &&
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
                if (bidEvaluation.CreatedByUserId != currentUserId && bidEvaluation.CreatedByUser != null)
                {
                    _logger.LogInformation("✅ Found original requester: {UserName}", bidEvaluation.CreatedByUser.UserName);
                    return Json(new
                    {
                        success = true,
                        userId = bidEvaluation.CreatedByUser.Id,
                        userName = bidEvaluation.CreatedByUser.UserName,
                        email = bidEvaluation.CreatedByUser.Email,
                        displayText = $"{bidEvaluation.CreatedByUser.UserName} ({bidEvaluation.CreatedByUser.Email}) - Original Requester"
                    });
                }

                _logger.LogInformation("⚠️ No previous user found for Bid Evaluation: {BidEvaluationId}", bidEvaluationId);
                return Json(new { success = false, message = "No previous user found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting previous user for bid evaluation {BidEvaluationId}", bidEvaluationId);
                return Json(new { success = false, message = "An error occurred while retrieving the previous user.", error = ex.Message });
            }
        }


        [Route("Procurement/BidEvaluation/GetPreviousWorkflowUsers")]
        [HttpGet]
        public async Task<JsonResult> GetPreviousWorkflowUsers(int bidEvaluationId)
        {
            // Renamed parameter, route, and debug messages
            System.Diagnostics.Debug.WriteLine($"Controller hit with ID: {bidEvaluationId}");
            _logger.LogInformation("CONTROLLER HIT - Getting previous workflow users for bid evaluation {BidEvaluationId}", bidEvaluationId);
            try
            {
                _logger.LogInformation("Getting previous workflow users for bid evaluation {BidEvaluationId}", bidEvaluationId);

                // Renamed variable and entity access
                var bidEvaluation = await _context.BidEvaluations
                    .Include(be => be.CreatedByUser) // Include the requester
                    .FirstOrDefaultAsync(be => be.BidNo == bidEvaluationId);

                if (bidEvaluation == null)
                {
                    // Renamed log message and user message
                    _logger.LogWarning("Bid evaluation {BidEvaluationId} not found", bidEvaluationId);
                    return Json(new { success = false, message = "Bid evaluation not found." });
                }

                var currentUserId = GetCurrentUserId();
                _logger.LogInformation("Current user ID: {CurrentUserId}", currentUserId);

                // Get workflow history to find all unique users who have acted on this request
                var workflowHistory = await _context.FormHistories
                    // Linked to the new bidEvaluation object's workflow properties
                    .Where(fh => fh.WorkFlowTypeId == bidEvaluation.WorkFlowTypeId &&
                                 fh.FormId == bidEvaluation.FormId)
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
                if (bidEvaluation.CreatedByUser != null && bidEvaluation.CreatedByUserId != currentUserId)
                {
                    // Used bidEvaluation properties
                    previousUsers.Add(new
                    {
                        value = bidEvaluation.CreatedByUserId.ToString(),
                        text = $"{bidEvaluation.CreatedByUser.UserName} ({bidEvaluation.CreatedByUser.Email}) - Original Requester",
                        group = new { name = "Original Requester" }
                    });
                    _logger.LogInformation("Added original requester: {UserName}", bidEvaluation.CreatedByUser.UserName);
                }

                // Add users from workflow history (excluding current user)
                var processedUserIds = new HashSet<int> { currentUserId };
                if (bidEvaluation.CreatedByUserId != currentUserId)
                {
                    processedUserIds.Add(bidEvaluation.CreatedByUserId);
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
                // Renamed log message
                _logger.LogError(ex, "Error getting previous workflow users for bid evaluation {BidEvaluationId}", bidEvaluationId);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while retrieving previous users.",
                    error = ex.Message
                });
            }
        }

        // GET: Procurement/BidEvaluation
        [RequirePermission("Bid Evaluation", "View")]
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            var currentUserName = GetCurrentUserName();

            var bidEvaluations = await _context.BidEvaluations
                .Include(be => be.PurchaseRequest)
                .Include(be => be.CreatedByUser)
                .Include(be => be.Bids)
                .Include(be => be.SelectedBid)
                    .ThenInclude(b => b.Supplier)
                .Where(be => be.CreatedByUserId == currentUserId || be.Owner == currentUserName)
                .OrderByDescending(be => be.CreatedOn)
                .ToListAsync();

            foreach (var be in bidEvaluations)
            {
                var bidCount = await _context.Bids.CountAsync(b => b.BidNo == be.BidNo);
                _logger.LogInformation($"BidEvaluation {be.BidEvaluationNumber}: Bids.Count = {be.Bids.Count}, DB Count = {bidCount}");
            }

            return View(bidEvaluations);
        }

        [RequirePermission("Bid Evaluation", "Add")]
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
        // GET: Procurement/BidEvaluation/Create?prNo=1098
        [RequirePermission("Bid Evaluation", "Add")]
        public async Task<IActionResult> Create(int prNo)
        {
            var purchaseRequest = await _context.PurchaseRequests
                .Include(pr => pr.Items)
                    .ThenInclude(i => i.UoM)
                .Include(pr => pr.Department)
                .Include(pr => pr.Branch)
                .FirstOrDefaultAsync(pr => pr.Id == prNo);

            if (purchaseRequest == null)
            {
                TempData["Error"] = "Purchase Request not found.";
                return RedirectToAction("Index", "PurchaseRequest");
            }

            // Check if PR is approved
            if (purchaseRequest.StateId != 3)
            {
                TempData["Error"] = "Bid Evaluation can only be created for approved Purchase Requests.";
                return RedirectToAction("Details", "PurchaseRequest", new { id = prNo });
            }

            // Check if bid evaluation already exists
            var existingBidEvaluation = await _context.BidEvaluations
                .AnyAsync(be => be.PRNo == prNo);

            if (existingBidEvaluation)
            {
                TempData["Error"] = "A Bid Evaluation already exists for this Purchase Request.";
                return RedirectToAction("Details", "PurchaseRequest", new { id = prNo });
            }

            var currentUserId = GetCurrentUserId();
            var user = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            var viewModel = new BidEvaluationCreateViewModel
            {
                PRNo = prNo,
                PurchaseRequestNumber = purchaseRequest.RequestNumber,
                Title = $"Bid Evaluation for {purchaseRequest.RequestNumber}",
                EvaluationDate = DateTime.Now,
                SubmissionDeadline = DateTime.Now.AddDays(7),
                EstimatedAmount = purchaseRequest.Items.Sum(i => i.TotalAmount),
                PurchaseRequestItems = purchaseRequest.Items.Select(i => new PurchaseRequestItemDto
                {
                    ItemId = i.DetailId,  // Changed from ItemId to DetailId
                    ItemName = i.ItemName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalAmount = i.TotalAmount,
                    UoMName = i.UoM?.UoMName ?? "N/A"
                }).ToList()
            };

            await PopulateDropdowns();
            return View(viewModel);
        }

        // POST: Procurement/BidEvaluation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Bid Evaluation", "Add")]
        public async Task<IActionResult> Create(BidEvaluationCreateViewModel viewModel)
        {
            ModelState.Remove("PurchaseRequestItems");
            ModelState.Remove("PurchaseRequestNumber");

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserId = GetCurrentUserId();
                    var user = await _context.Users
                        .Include(u => u.Department)
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == currentUserId);

                    var bidEvaluation = new BidEvaluation
                    {
                        BidEvaluationNumber = await GenerateBidEvaluationNumberAsync(),
                        PRNo = viewModel.PRNo,
                        Title = viewModel.Title,
                        Justification = viewModel.Justification,
                        EvaluationDate = viewModel.EvaluationDate,
                        SubmissionDeadline = viewModel.SubmissionDeadline,
                        EstimatedAmount = viewModel.EstimatedAmount,
                        StateId = WorkflowService.STATE_SAVED,
                        Owner = GetCurrentUserName(),
                        DepartmentCode = user?.Department?.DepartmentCode ?? "",
                        BranchCode = user?.Branch?.BranchCode ?? "",
                        CreatedByUserId = currentUserId,
                        CreatedOn = DateTimeHelper.GetPakistanStandardTime()
                    };

                    _context.BidEvaluations.Add(bidEvaluation);
                    await _context.SaveChangesAsync();

                    // Add workflow history
                    await _workflowService.AddFormHistoryAsync(
                        bidEvaluation.WorkFlowTypeId,
                        bidEvaluation.FormId,
                        0,
                        WorkflowService.STATE_SAVED,
                        "Created",
                        "Bid Evaluation created",
                        currentUserId,
                        null);

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Bid Evaluation created successfully.";
                    return RedirectToAction(nameof(Details), new { id = bidEvaluation.BidNo });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating bid evaluation");
                    TempData["Error"] = "An error occurred while creating the Bid Evaluation.";
                }
            }

            // Repopulate dropdown and PR items if validation fails
            await PopulateDropdowns();
            var purchaseRequest = await _context.PurchaseRequests
                .Include(pr => pr.Items)
                    .ThenInclude(i => i.UoM)
                .FirstOrDefaultAsync(pr => pr.Id == viewModel.PRNo);

            if (purchaseRequest != null)
            {
                viewModel.PurchaseRequestNumber = purchaseRequest.RequestNumber;
                viewModel.PurchaseRequestItems = purchaseRequest.Items.Select(i => new PurchaseRequestItemDto
                {
                    ItemId = i.DetailId,  // Changed from ItemId to DetailId
                    ItemName = i.ItemName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalAmount = i.TotalAmount,
                    UoMName = i.UoM?.UoMName ?? "N/A"
                }).ToList();
            }

            return View(viewModel);
        }

        // GET: Procurement/BidEvaluation/Details/5
        [RequirePermission("Bid Evaluation", "View")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var bidEvaluation = await _context.BidEvaluations
                .Include(be => be.PurchaseRequest)
                    .ThenInclude(pr => pr.Items)
                        .ThenInclude(i => i.UoM)
                .Include(be => be.Bids)
                    .ThenInclude(b => b.Supplier)
                .Include(be => be.Bids)
                    .ThenInclude(b => b.BidItems)
                .Include(be => be.SelectedBid)
                    .ThenInclude(b => b.Supplier)
                .Include(be => be.CreatedByUser)
                .FirstOrDefaultAsync(be => be.BidNo == id);

            if (bidEvaluation == null) return NotFound();

            // Security check
            var currentUserId = GetCurrentUserId();
            var currentUserName = GetCurrentUserName();
            var ownerUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == bidEvaluation.Owner);
            var ownerId = ownerUser?.Id ?? 0;

            bool isAuthorized = bidEvaluation.CreatedByUserId == currentUserId || ownerId == currentUserId;

            var history = await _workflowService.GetFormHistoryAsync(
                bidEvaluation.WorkFlowTypeId,
                bidEvaluation.FormId);

            var isUserInHistory = history.Any(h => h.ActionByUserId == currentUserId || h.ToUserId == currentUserId);

            if (!isAuthorized && !isUserInHistory)
            {
                TempData["Error"] = "You are not authorized to view this Bid Evaluation.";
                return Forbid();
            }

            ViewBag.WorkflowHistory = history;
            ViewBag.CurrentStateName = await _workflowService.GetStateNameAsync(
                bidEvaluation.WorkFlowTypeId,
                bidEvaluation.StateId);

            var currentUserRoles = await GetUserRolesAsync(currentUserId);
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.CurrentUserName = currentUserName;
            ViewBag.CurrentUserRoles = currentUserRoles;

            return View(bidEvaluation);
        }

        // GET: Procurement/BidEvaluation/AddBid/5
        [RequirePermission("Bid Evaluation", "Add")]
        public async Task<IActionResult> AddBid(int bidEvaluationId)
        {
            var bidEvaluation = await _context.BidEvaluations
                .Include(be => be.PurchaseRequest)
                    .ThenInclude(pr => pr.Items)
                        .ThenInclude(i => i.UoM)
                .FirstOrDefaultAsync(be => be.BidNo == bidEvaluationId);

            if (bidEvaluation == null)
            {
                TempData["Error"] = "Bid Evaluation not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new BidCreateViewModel
            {
                BidNo = bidEvaluationId,
                BidEvaluationNumber = bidEvaluation.BidEvaluationNumber,
                SubmissionDate = DateTime.Now,
                DeliveryDays = 30,
                PaymentTerms = "Net 30",
                ValidityPeriod = "30 Days",
                MeetsRequirements = true,
                IsResponsive = true,
                BidItems = bidEvaluation.PurchaseRequest.Items.Select(prItem => new BidItemViewModel
                {
                    PurchaseRequestItemId = prItem.DetailId,  // Changed from ItemId to DetailId
                    ItemName = prItem.ItemName,
                    Specifications = prItem.Narration,  // Changed from Specifications to Narration
                    Quantity = prItem.Quantity,
                    UOM = prItem.UoM?.UoMName ?? "N/A",
                    UoMId = prItem.UoMId,
                    UnitPrice = 0,
                    MeetsSpecifications = true
                }).ToList()
            };

            await PopulateDropdowns();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBid(BidCreateViewModel viewModel, IFormFile quotationDocument)
        {
            if (viewModel.BidItems != null)
            {
                for (int i = 0; i < viewModel.BidItems.Count; i++)
                {
                    ModelState.Remove($"BidItems[{i}].UOM");
                }
            }

            const int MaxFileSizeMB = 5;
            const int MaxFileSizeBytes = MaxFileSizeMB * 1024 * 1024;
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };

            if (quotationDocument == null || quotationDocument.Length == 0)
            {
                // If the document is optional, remove any default 'required' error that might be present
                ModelState.Remove("quotationDocument");
            }
            else
            {
                // Validation for optional file if it IS provided (size and extension)
                var fileExtension = Path.GetExtension(quotationDocument.FileName).ToLowerInvariant();

                if (quotationDocument.Length > MaxFileSizeBytes)
                {
                    ModelState.AddModelError("quotationDocument", $"File size exceeds the limit of {MaxFileSizeMB} MB.");
                }
                else if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("quotationDocument", "Invalid file type. Only PDF, DOCX, JPG, and PNG are allowed.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserId = GetCurrentUserId();

                    // Calculate totals
                    var totalAmount = viewModel.BidItems?.Sum(bi => bi.TotalPrice) ?? 0;
                    var grandTotal = totalAmount + viewModel.TaxAmount;

                    var bid = new Bid
                    {
                        BidNo = viewModel.BidNo,
                        SupplierId = viewModel.SupplierId,
                        QuotationNumber = viewModel.QuotationNumber,
                        SubmissionDate = viewModel.SubmissionDate,
                        TotalAmount = totalAmount,
                        TaxAmount = viewModel.TaxAmount,
                        GrandTotal = grandTotal,
                        DeliveryDays = viewModel.DeliveryDays,
                        PaymentTerms = viewModel.PaymentTerms,
                        ValidityPeriod = viewModel.ValidityPeriod,
                        MeetsRequirements = viewModel.MeetsRequirements,
                        IsResponsive = viewModel.IsResponsive,
                        Remarks = viewModel.Remarks,
                        CreatedOn = DateTime.Now,
                        CreatedByUserId = currentUserId,
                        BidItems = new List<BidItem>()  // ⭐ IMPORTANT: Initialize the collection
                    };

                    // Handle quotation document upload
                    if (quotationDocument != null && quotationDocument.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "quotations");
                        Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = $"{Guid.NewGuid()}_{quotationDocument.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await quotationDocument.CopyToAsync(fileStream);
                        }

                        bid.QuotationDocumentPath = $"/uploads/quotations/{uniqueFileName}";
                    }

                    // Add bid items - Make sure this happens BEFORE adding to context
                    if (viewModel.BidItems != null && viewModel.BidItems.Any())
                    {
                        _logger.LogInformation($"Adding {viewModel.BidItems.Count} bid items");

                        foreach (var itemVm in viewModel.BidItems)
                        {
                            var bidItem = new BidItem
                            {
                                PurchaseRequestItemId = itemVm.PurchaseRequestItemId,
                                ItemName = itemVm.ItemName,
                                Specifications = itemVm.Specifications,
                                Quantity = itemVm.Quantity,
                                UoMId = itemVm.UoMId,
                                UnitPrice = itemVm.UnitPrice,
                                TotalPrice = itemVm.TotalPrice,
                                Remarks = itemVm.Remarks,
                                MeetsSpecifications = itemVm.MeetsSpecifications
                            };

                            bid.BidItems.Add(bidItem);

                            _logger.LogInformation($"Added item: {bidItem.ItemName}, Qty: {bidItem.Quantity}, Price: {bidItem.UnitPrice}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No bid items to add!");
                    }

                    // Add to context and save
                    _context.Bids.Add(bid);

                    _logger.LogInformation($"Saving bid with {bid.BidItems.Count} items");

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Bid saved successfully. BidId: {bid.BidId}");

                    // Verify the save
                    var savedBidItemsCount = await _context.Set<BidItem>()
                        .CountAsync(bi => bi.BidId == bid.BidId);

                    _logger.LogInformation($"Verification: Found {savedBidItemsCount} bid items in database for BidId: {bid.BidId}");

                    TempData["Success"] = $"Bid added successfully with {bid.BidItems.Count} items.";
                    return RedirectToAction(nameof(Details), new { id = viewModel.BidNo });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding bid. Inner exception: {InnerException}", ex.InnerException?.Message);
                    TempData["Error"] = $"An error occurred while adding the bid: {ex.Message}. {ex.InnerException?.Message}";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning($"ModelState invalid. Errors: {string.Join(", ", errors)}");
                TempData["Error"] = "Please fix the validation errors.";
            }

            await PopulateDropdowns();
            var bidEvaluation = await _context.BidEvaluations
                .FirstOrDefaultAsync(be => be.BidNo == viewModel.BidNo);
            if (bidEvaluation != null)
            {
                viewModel.BidEvaluationNumber = bidEvaluation.BidEvaluationNumber;
            }

            return View(viewModel);
        }


        // GET: Procurement/BidEvaluation/CompareBids/2
        public async Task<IActionResult> CompareBids(int bidEvaluationId)
        {
            var bidEvaluation = await _context.BidEvaluations
                .Include(be => be.Bids)
                    .ThenInclude(b => b.Supplier)
                .Include(be => be.Bids)
                    .ThenInclude(b => b.BidItems)
                .Include(be => be.PurchaseRequest)
                    .ThenInclude(pr => pr.Items)
                .FirstOrDefaultAsync(be => be.BidNo == bidEvaluationId);

            if (bidEvaluation == null)
            {
                TempData["Error"] = "Bid Evaluation not found.";
                return RedirectToAction(nameof(Index));
            }

            // Get all unique item names from purchase request
            var itemNames = bidEvaluation.PurchaseRequest.Items
                .Select(i => i.ItemName)
                .Distinct()
                .ToList();

            // Rank bids by grand total (lowest to highest)
            var rankedBids = bidEvaluation.Bids
                .OrderBy(b => b.GrandTotal)
                .Select((bid, index) => new
                {
                    Bid = bid,
                    Rank = index + 1
                })
                .ToList();

            var viewModel = new BidComparisonViewModel
            {
                BidEvaluationId = bidEvaluation.BidNo,
                BidEvaluationNumber = bidEvaluation.BidEvaluationNumber,
                SelectedBidId = bidEvaluation.SelectedBidId,
                SelectionJustification = bidEvaluation.SelectionJustification,
                ItemNames = itemNames,
                Bids = rankedBids.Select(rb => new BidComparisonViewModel.BidSummary
                {
                    BidId = rb.Bid.BidId,
                    SupplierName = rb.Bid.Supplier?.SupplierName ?? "Unknown",
                    QuotationNumber = rb.Bid.QuotationNumber,
                    TotalAmount = rb.Bid.TotalAmount,
                    TaxAmount = rb.Bid.TaxAmount,
                    GrandTotal = rb.Bid.GrandTotal,
                    DeliveryDays = rb.Bid.DeliveryDays,
                    MeetsRequirements = rb.Bid.MeetsRequirements,
                    IsResponsive = rb.Bid.IsResponsive,
                    Rank = rb.Rank,
                    Items = rb.Bid.BidItems.Select(bi => new BidComparisonViewModel.BidItemDetail
                    {
                        ItemName = bi.ItemName,
                        Quantity = bi.Quantity,
                        UnitPrice = bi.UnitPrice,
                        TotalPrice = bi.TotalPrice,
                        MeetsSpecifications = bi.MeetsSpecifications
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }
        // POST: Procurement/BidEvaluation/SelectWinner
        [HttpPost]
        [ValidateAntiForgeryToken]

        [HttpPost]
        public async Task<JsonResult> SelectWinnerAction(int bidEvaluationId, int selectedBidId)
        {
            var bidEvaluation = await _context.BidEvaluations
                .Include(be => be.Bids)
                .FirstOrDefaultAsync(be => be.BidNo == bidEvaluationId);

            if (bidEvaluation == null)
            {
                return Json(new { success = false, message = "Bid Evaluation not found." });
            }

            var selectedBid = bidEvaluation.Bids.FirstOrDefault(b => b.BidId == selectedBidId);
            if (selectedBid == null)
            {
                return Json(new { success = false, message = "Selected bid not found." });
            }

            try
            {
                var currentUserId = GetCurrentUserId();

                // Update bid evaluation fields only
                bidEvaluation.SelectedBidId = selectedBidId;
                bidEvaluation.UpdatedByUserId = currentUserId;
                bidEvaluation.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                // Save the selection
                await _context.SaveChangesAsync();

                string successMessage = $"Winning bid set to {selectedBid.Supplier?.SupplierName ?? "selected supplier"}. Please approve the evaluation to finalize.";

                _logger.LogInformation("Bid winner selected successfully. BidEvaluation: {BidEvaluationId}, SelectedBid: {SelectedBidId}", bidEvaluationId, selectedBidId);

                // Return JSON success response, prompting client-side page reload
                return Json(new
                {
                    success = true,
                    message = successMessage,
                    redirectUrl = Url.Action(nameof(Details), new { id = bidEvaluationId })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting winner for bid evaluation {BidEvaluationId}", bidEvaluationId);
                return Json(new { success = false, message = "An error occurred while saving the winner selection." });
            }
        }

        // POST: Procurement/BidEvaluation/WorkflowAction
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 1. Rename method
        public async Task<IActionResult> WorkflowActionBE(int id, string action, string comments = "", int? returnToUserId = null, bool returnToPrevious = false, int? submitToUserId = null, int? approveToUserId = null)
        {
            // 2. Rename variable and entity access
            var bidEvaluation = await _context.BidEvaluations
                .Include(be => be.CreatedByUser)
                .FirstOrDefaultAsync(be => be.BidNo == id);

            if (bidEvaluation == null) return NotFound();

            var currentUserId = GetCurrentUserId();

            // 3. Use bidEvaluation object for service call
            bool canPerform = await _workflowService.CanUserPerformActionAsync(
                currentUserId, bidEvaluation, action);

            if (!canPerform)
            {
                // 4. Update message
                TempData["Error"] = $"You are not authorized to {action} this Bid Evaluation.";
                return RedirectToAction(nameof(Details), new { id });
            }

            WorkflowActionBEResult result;

            if (action.Equals("submit", StringComparison.OrdinalIgnoreCase))
            {
                if (!submitToUserId.HasValue)
                {
                    TempData["Error"] = "Please select a user to submit the request to.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // 6. Use bidEvaluation object for service call
                var workflowResult = await _workflowService.ProcessWorkflowActionAsync(
                    bidEvaluation, action, comments, currentUserId, submitToUserId);

                result = new WorkflowActionBEResult
                {
                    Success = workflowResult.Success,
                    Message = workflowResult.Message,
                    NextApprover = workflowResult.NextApprover
                };
            }
            else if (action.Equals("approve", StringComparison.OrdinalIgnoreCase))
            {
                // 8. Use bidEvaluation object for service call
                var workflowResult = await _workflowService.ProcessWorkflowActionAsync(
                    bidEvaluation, action, comments, currentUserId, approveToUserId);

                result = new WorkflowActionBEResult
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
                    // 10. Use bidEvaluation object for return logic
                    var previousUserResult = await GetPreviousUserForReturn(bidEvaluation);
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

                // 11. Use bidEvaluation object for return logic
                result = await ProcessReturnToSpecificUser(bidEvaluation, targetUserId, comments, currentUserId);
            }
            else
            {
                // 12. Use bidEvaluation object for service call
                var workflowResult = await _workflowService.ProcessWorkflowActionAsync(
                    bidEvaluation, action, comments, currentUserId);

                // 13. Rename WorkflowActionPOResult (assuming this model is also renamed)
                result = new WorkflowActionBEResult
                {
                    Success = workflowResult.Success,
                    Message = workflowResult.Message,
                    NextApprover = workflowResult.NextApprover
                };
            }

            if (result.Success)
            {
                // 14. Update context with bidEvaluation
                _context.Update(bidEvaluation);
                await _context.SaveChangesAsync();

                TempData["Success"] = result.Message;

                // 15. Use bidEvaluation properties for display
                if (!string.IsNullOrEmpty(bidEvaluation.Owner) && bidEvaluation.Owner != "System" && bidEvaluation.Owner != (bidEvaluation.CreatedByUser?.UserName ?? "Unknown Creator"))
                {
                    TempData["Info"] = $"Next assignee: {bidEvaluation.Owner}";
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

        private async Task<PreviousUserResult> GetPreviousUserForReturn(BidEvaluation bidEvaluation)
        {
            var currentUserId = GetCurrentUserId();

            try
            {
                // First, try to get the most recent user from workflow history (excluding current user)
                var lastHistoryEntry = await _context.FormHistories
                    // Use bidEvaluation's workflow properties
                    .Where(fh => fh.WorkFlowTypeId == bidEvaluation.WorkFlowTypeId &&
                                 fh.FormId == bidEvaluation.FormId &&
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
                // Use bidEvaluation's CreatedByUserId
                if (bidEvaluation.CreatedByUserId != currentUserId)
                {
                    var requester = await _context.Users.FindAsync(bidEvaluation.CreatedByUserId);
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
                // Update log message to reference bid evaluation
                _logger.LogError(ex, "Error finding previous user for return on bid evaluation {BidEvaluationId}", bidEvaluation.BidNo);
                return new PreviousUserResult
                {
                    Success = false,
                    Message = "An error occurred while finding the previous user."
                };
            }
        }

        private async Task<WorkflowActionBEResult> ProcessReturnToSpecificUser(
    BidEvaluation bidEvaluation, // Model is BidEvaluation
    int returnToUserId,
    string comments,
    int currentUserId)
        {
            var returnToUser = await _context.Users.FindAsync(returnToUserId);
            if (returnToUser == null)
            {
                // 1. Rename result object
                return new WorkflowActionBEResult
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
                // 2. Use bidEvaluation object
                .Where(ws => ws.WorkFlowTypeId == bidEvaluation.WorkFlowTypeId &&
                             ws.IsActive &&
                             targetUserRoles.Contains(ws.RoleID))
                .OrderByDescending(ws => ws.ApprovalSeq)
                .Select(ws => (int?)ws.ApprovalSeq)
                .FirstOrDefaultAsync();

            // 3. Determine the sequence the BE should be returned to.
            // If roles are found, set it to the highest sequence. If not, set to 0 for a full restart.
            int returnedToSequence = targetUserHighestApprovalSequence.HasValue
                ? targetUserHighestApprovalSequence.Value
                : 0;

            // Update BE state
            short fromStateId = bidEvaluation.StateId;
            bidEvaluation.StateId = WorkflowService.STATE_RETURNED;
            // 3. Use bidEvaluation object
            bidEvaluation.Owner = returnToUser.UserName ?? returnToUser.Email;

            // ⭐ FIX: Set CurrentApprovalSequence based on the target user's role hierarchy
            // 4. Use bidEvaluation object
            bidEvaluation.CurrentApprovalSequence = returnedToSequence;

            // 5. Use bidEvaluation object
            bidEvaluation.SyncStatusWithState();
            bidEvaluation.Approved = false;
            bidEvaluation.Rejected = false;
            bidEvaluation.IsCompleted = false;

            // Add workflow history entry.
            // 6. Use bidEvaluation object
            await _workflowService.AddFormHistoryAsync(
                bidEvaluation.WorkFlowTypeId,
                bidEvaluation.FormId,
                fromStateId,
                WorkflowService.STATE_RETURNED,
                "Returned",
                comments,
                currentUserId,
                returnToUserId);

            // 7. Rename result object and update message
            return new WorkflowActionBEResult
            {
                Success = true,
                Message = $"Bid Evaluation returned to {returnToUser.UserName} successfully. Approval sequence reset to {returnedToSequence}.",
                NextApprover = returnToUser
            };
        }
        private async Task<string> GenerateBidEvaluationNumberAsync()
        {
            var date = DateTime.Now;
            var prefix = $"BE{date:yyyyMMdd}";

            var lastBidEvaluation = await _context.BidEvaluations
                .Where(be => be.BidEvaluationNumber.StartsWith(prefix))
                .OrderByDescending(be => be.BidEvaluationNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastBidEvaluation != null)
            {
                var lastSequence = lastBidEvaluation.BidEvaluationNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }
            return $"{prefix}{sequence:D3}";
        }

        private async Task PopulateDropdowns()
        {
            ViewBag.Suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.SupplierName
                })
                .ToListAsync();

            ViewBag.UoMs = await _context.UoMs
                .Where(u => u.IsActive)
                .Select(u => new SelectListItem
                {
                    Value = u.UoMId.ToString(),
                    Text = u.UoMName
                })
                .ToListAsync();
        }

        // GET: Procurement/BidEvaluation/DownloadQuotationDocument/123
        [HttpGet]
        [RequirePermission("Bid Evaluation", "View")] // Use View permission or create a specific Download permission
        public async Task<IActionResult> DownloadQuotationDocument(int bidId)
        {
            // 1. Find the Bid record
            var bid = await _context.Bids.FindAsync(bidId);

            if (bid == null)
            {
                _logger.LogWarning("Attempted to download document for non-existent Bid ID: {BidId}", bidId);
                return NotFound();
            }

            // 2. Check if a document path exists
            if (string.IsNullOrEmpty(bid.QuotationDocumentPath))
            {
                _logger.LogWarning("Bid ID {BidId} does not have a quotation document path.", bidId);
                return NotFound("No document attached to this bid.");
            }

            // 3. Construct the full physical file path
            // The path in the database is relative (e.g., /uploads/quotations/unique.pdf)
            var relativePath = bid.QuotationDocumentPath.TrimStart('/');
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

            // 4. Check if the file exists on the disk
            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogError("Quotation document not found on disk at path: {Path}", fullPath);
                return NotFound("The file was not found on the server.");
            }

            try
            {
                // 5. Determine the MIME type and file name for the download
                var fileName = Path.GetFileName(fullPath);

                // You might need a utility function (like the one below) to determine the content type
                var contentType = GetContentType(fileName);

                // 6. Read the file into a byte array
                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

                // 7. Return the file, which sets Content-Disposition: attachment
                // This is the CRITICAL step to force the browser to download.
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading quotation document for Bid ID: {BidId}", bidId);
                return StatusCode(500, "An error occurred while processing the download request.");
            }
        }

        // Helper method to guess content type (You might need to place this elsewhere, like a helper class)
        private string GetContentType(string fileName)
        {
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}