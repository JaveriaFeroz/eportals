using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Common.Enums;
using ProcureToPay.Areas.Common.Services;
using ProcureToPay.Areas.Finance.Models;
using ProcureToPay.Areas.Finance.Models.DTOs;
using ProcureToPay.Areas.Finance.Services;
using ProcureToPay.Areas.Finance.ViewModels;
using ProcureToPay.Areas.UserManagement.Attributes;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Data;
using ProcureToPay.Helpers;
using System.Net.Mail;
using System.Security.Claims;
using static ProcureToPay.Areas.Common.Services.WorkflowService;

namespace ProcureToPay.Areas.Finance.Controllers
{
    [Area("Finance")]
    [Authorize]
    public class PaymentRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentRequestController> _logger;
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly IWorkflowService _workflowService;

        public PaymentRequestController(
            ApplicationDbContext context,
            ILogger<PaymentRequestController> logger,
            IPaymentRequestService paymentRequestService,
            IWorkflowService workflowService)
        {
            _context = context;
            _logger = logger;
            _paymentRequestService = paymentRequestService;
            _workflowService = workflowService;
        }

        public class WorkflowActionResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public object? NextApprover { get; set; }
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
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
        public async Task<JsonResult> GetApprovers(int paymentRequestId)
        {
            try
            {
                // 1. Find the Payment Request document.
                var paymentRequest = await _context.PaymentRequests.FindAsync(paymentRequestId);
                if (paymentRequest == null)
                {
                    return Json(new { success = false, message = "Payment Request not found." });
                }

                // Get the ID of the current user who is initiating the action
                var currentUserId = GetCurrentUserId();

                // Initialize the next approval sequence
                int nextApprovalSeq;

                // 2. Get all roles for the current user.
                var currentUserRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == currentUserId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                if (currentUserRoles.Any())
                {
                    // 3. Find the highest approval sequence covered by the current user's roles
                    // for this specific workflow, payment nature, and sub-nature.
                    var highestApprovalSequence = await _context.WorkFlowApprovalSequences
                        .Where(ws => ws.WorkFlowTypeId == paymentRequest.WorkFlowTypeId &&
                                     ws.PaymentNatureID == paymentRequest.PaymentNatureId &&
                                     ws.PaymentSubNatureID == paymentRequest.PaymentSubNatureId &&
                                     ws.IsActive &&
                                     currentUserRoles.Contains(ws.RoleID))
                        .OrderByDescending(ws => ws.ApprovalSeq)
                        .Select(ws => (int?)ws.ApprovalSeq)
                        .FirstOrDefaultAsync();

                    if (highestApprovalSequence.HasValue)
                    {
                        // Set the next approval sequence to one level above the current user's highest role,
                        // effectively skipping levels they are authorized to cover.
                        nextApprovalSeq = highestApprovalSequence.Value + 1;
                    }
                    else
                    {
                        // If the user's role is not found in the specific hierarchy, default to the first step.
                        nextApprovalSeq = 1;
                    }
                }
                else
                {
                    // If the user has no roles, default to the first step.
                    nextApprovalSeq = 1;
                }

                // 4. Use the newly determined `nextApprovalSeq` to fetch the eligible approvers.
                var users = await _workflowService.GetUsersForApprovalSequenceAsync(paymentRequest, nextApprovalSeq);

                if (!users.Any())
                {
                    return Json(new { success = false, message = "No approvers configured for the next step." });
                }

                var userList = users.Select(u => new
                {
                    value = u.Id,
                    text = $"{u.FirstName} {u.LastName} ({u.Email})"
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
        public async Task<JsonResult> GetPreviousUser(int paymentRequestId)
        {
            _logger.LogInformation("🎯 GetPreviousUser called with ID: {PaymentRequestId}", paymentRequestId);

            try
            {
                if (paymentRequestId <= 0)
                {
                    _logger.LogWarning("❌ Invalid paymentRequestId: {PaymentRequestId}", paymentRequestId);
                    return Json(new { success = false, message = "Invalid Purchase Request ID" });
                }

                var paymentRequest = await _context.PaymentRequests
                    .Include(pr => pr.CreatedByUser)
                    .FirstOrDefaultAsync(pr => pr.Id == paymentRequestId);

                if (paymentRequest == null)
                {
                    _logger.LogWarning("❌ Purchase request not found: {PaymentRequestId}", paymentRequestId);
                    return Json(new { success = false, message = "Purchase request not found." });
                }

                var currentUserId = GetCurrentUserId();
                _logger.LogInformation("🔍 Current user ID: {CurrentUserId}", currentUserId);

                // Get the most recent workflow history entry (excluding current user)
                var lastHistoryEntry = await _context.FormHistories
                    .Where(fh => fh.WorkFlowTypeId == paymentRequest.WorkFlowTypeId &&
                                 fh.FormId == paymentRequest.FormId &&
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
                if (paymentRequest.CreatedByUserId != currentUserId && paymentRequest.CreatedByUser != null)
                {
                    _logger.LogInformation("✅ Found original requester: {UserName}", paymentRequest.CreatedByUser.UserName);
                    return Json(new
                    {
                        success = true,
                        userId = paymentRequest.CreatedByUser.Id,
                        userName = paymentRequest.CreatedByUser.UserName,
                        email = paymentRequest.CreatedByUser.Email,
                        displayText = $"{paymentRequest.CreatedByUser.UserName} ({paymentRequest.CreatedByUser.Email}) - Original Requester"
                    });
                }

                _logger.LogInformation("⚠️ No previous user found for PR: {PaymentRequestId}", paymentRequestId);
                return Json(new { success = false, message = "No previous user found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting previous user for purchase request {PaymentRequestId}", paymentRequestId);
                return Json(new { success = false, message = "An error occurred while retrieving the previous user.", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetPreviousWorkflowUsers(int paymentRequestId)
        {
            System.Diagnostics.Debug.WriteLine($"Controller hit with ID: {paymentRequestId}");
            _logger.LogInformation("CONTROLLER HIT - Getting previous workflow users for purchase request {PaymentRequestId}", paymentRequestId);
            try
            {
                _logger.LogInformation("Getting previous workflow users for purchase request {PaymentRequestId}", paymentRequestId);

                var paymentRequest = await _context.PaymentRequests
                    .Include(pr => pr.CreatedByUser) // Include the requester
                    .FirstOrDefaultAsync(pr => pr.Id == paymentRequestId);

                if (paymentRequest == null)
                {
                    _logger.LogWarning("Purchase request {PaymentRequestId} not found", paymentRequestId);
                    return Json(new { success = false, message = "Purchase request not found." });
                }

                var currentUserId = GetCurrentUserId();
                _logger.LogInformation("Current user ID: {CurrentUserId}", currentUserId);

                // Get workflow history to find all unique users who have acted on this request
                var workflowHistory = await _context.FormHistories
                    .Where(fh => fh.WorkFlowTypeId == paymentRequest.WorkFlowTypeId &&
                                 fh.FormId == paymentRequest.FormId)
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
                if (paymentRequest.CreatedByUser != null && paymentRequest.CreatedByUserId != currentUserId)
                {
                    previousUsers.Add(new
                    {
                        value = paymentRequest.CreatedByUserId.ToString(),
                        text = $"{paymentRequest.CreatedByUser.UserName} ({paymentRequest.CreatedByUser.Email}) - Original Requester",
                        group = new { name = "Original Requester" }
                    });
                    _logger.LogInformation("Added original requester: {UserName}", paymentRequest.CreatedByUser.UserName);
                }

                // Add users from workflow history (excluding current user)
                var processedUserIds = new HashSet<int> { currentUserId };
                if (paymentRequest.CreatedByUserId != currentUserId)
                {
                    processedUserIds.Add(paymentRequest.CreatedByUserId);
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
                _logger.LogError(ex, "Error getting previous workflow users for purchase request {PaymentRequestId}", paymentRequestId);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while retrieving previous users.",
                    error = ex.Message
                });
            }
        }// GET: Finance/PaymentRequest

        [RequirePermission("Payment Request", "View")]
        public async Task<IActionResult> Index(int? userId = null, short? stateId = null)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserName = GetCurrentUserName();
            var currentUserRoles = await GetUserRolesAsync(currentUserId);

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.CurrentUserName = currentUserName;
            ViewBag.CurrentUserRoles = currentUserRoles;

            // Step 1: Find all PRQ IDs created by the current user.
            var createdPrqIds = await _context.PaymentRequests
                .Where(prq => prq.CreatedByUserId == currentUserId)
                .Select(prq => prq.Id)
                .ToListAsync();

            // Step 2: Find all PRQ IDs where the current user performed a workflow action.
            var actedOnPrqIds = await _context.FormHistories
                .Where(fh => fh.WorkFlowTypeId == (short)WorkFlowType.PaymentRequest && fh.ActionByUserId == currentUserId)
                .Select(fh => fh.FormId)
                .ToListAsync();

            // Step 3: Find all PRQ IDs where the current user is the next designated approver.
            var assignedToUserPrqIds = await _context.PaymentRequests
                .Where(prq => prq.Owner == currentUserName && prq.StateId != (short)PaymentRequestState.Rejected && prq.StateId != (short)PaymentRequestState.Cancelled)
                .Select(prq => prq.Id)
                .ToListAsync();

            // Step 4: Combine all unique IDs from the three lists.
            var relevantPrqIds = createdPrqIds.Union(actedOnPrqIds).Union(assignedToUserPrqIds).ToHashSet();

            // Step 5: Query the PaymentRequests table using the consolidated set of IDs.
            // This is the final, efficient query that fetches only the relevant records.
            var paymentRequests = await _context.PaymentRequests
                .Where(prq => relevantPrqIds.Contains(prq.Id))
                .Include(prq => prq.CreatedByUser)
                .Include(prq => prq.GoodsReceiptNote)
                .Include(prq => prq.Details)
                .Include(prq => prq.Attachments)
                .OrderByDescending(prq => prq.CreatedOn)
                .ToListAsync();

            var viewModel = new PaymentRequestIndexViewModel
            {
                PaymentRequests = new List<PaymentRequestListItemViewModel>()
            };

            foreach (var pr in paymentRequests)
            {
                var stateName = await _workflowService.GetStateNameAsync(pr.WorkFlowTypeId, pr.StateId);

                viewModel.PaymentRequests.Add(new PaymentRequestListItemViewModel
                {
                    Id = pr.Id,
                    GRNId = pr.GoodsReceiptNoteId,
                    PRQNumber = pr.PRQNumber ?? "N/A",
                    RequiredDate = pr.RequiredDate,
                    PayeeName = pr.PayeeName ?? "N/A",
                    TotalAmount = pr.TotalAmount,
                    StateId = pr.StateId,
                    StateName = stateName,
                    Owner = pr.Owner,
                    CreatedOn = pr.CreatedOn,
                    CreatedByUserName = pr.CreatedByUser?.UserName ?? "N/A",
                    GRNNumber = pr.GoodsReceiptNote?.GRNNumber,
                    IsGRNBased = pr.GoodsReceiptNoteId.HasValue,
                    PaymentType = pr.GoodsReceiptNoteId.HasValue ? "GRN-Based" : "Direct",
                    DetailsCount = pr.Details?.Count ?? 0,
                    AttachmentsCount = pr.Attachments?.Count ?? 0
                });
            }

            return View(viewModel);
        }

        [RequirePermission("Payment Request", "View")]
        // GET: Finance/PaymentRequest/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var paymentRequest = await _paymentRequestService.GetPaymentRequestByIdAsync(id.Value);
            if (paymentRequest == null) return NotFound();

            // 1. Get the current logged-in user's ID.
            var currentUserId = GetCurrentUserId();

            // B. Check if the user is in the workflow history.
            var historyEntities = await _workflowService.GetFormHistoryAsync(
                paymentRequest.WorkFlowTypeId, paymentRequest.FormId);
          

            // --- Added logic to fetch descriptive names (this part is fine, no changes needed) ---
            var paymentNatureName = (await _context.PaymentNatures.FindAsync(paymentRequest.PaymentNatureId))?.PaymentNatureName ?? "N/A";
            var paymentSubNatureName = paymentRequest.PaymentSubNatureId.HasValue ?
                (await _context.SubNatures.FindAsync(paymentRequest.PaymentSubNatureId.Value))?.SubNatureName ?? "N/A" : "N/A";
            var branchName = !string.IsNullOrEmpty(paymentRequest.BranchCode) ?
                (await _context.Branches.FirstOrDefaultAsync(b => b.BranchId.ToString() == paymentRequest.BranchCode))?.BranchName ?? "N/A" : "N/A";
            var departmentName = !string.IsNullOrEmpty(paymentRequest.DepartmentCode) ?
                (await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId.ToString() == paymentRequest.DepartmentCode))?.DepartmentName ?? "N/A" : "N/A";
            string? nonGrnSupplierName = null;
            if (!paymentRequest.GoodsReceiptNoteId.HasValue && paymentRequest.SupplierId.HasValue)
            {
                
                var supplier = await _context.Suppliers.FindAsync((short)paymentRequest.SupplierId.Value);

                nonGrnSupplierName = supplier?.SupplierName;
            }
            var attachmentTypes = await _context.AttachmentTypes
                .OrderBy(at => at.Name)
                .ToListAsync();

            var currentUser = await _context.Users
                .Include(u => u.Branch)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == GetCurrentUserId());

            var fullName = (currentUser?.FirstName + " " + currentUser?.LastName)?.Trim() ?? "Unknown User";

            var viewModel = new PaymentRequestDetailsViewModel
            {
                Id = paymentRequest.Id,
                GRNId = paymentRequest.GoodsReceiptNoteId ?? 0,
                PRQNumber = paymentRequest.PRQNumber ?? "N/A",
                RequiredDate = paymentRequest.RequiredDate,
                PayeeName = paymentRequest.PayeeName ?? "N/A",
                TotalAmount = paymentRequest.TotalAmount,
                StateId = paymentRequest.StateId,
                StateName = await _workflowService.GetStateNameAsync(paymentRequest.WorkFlowTypeId, paymentRequest.StateId),
                Owner = paymentRequest.Owner,
                CreatedOn = paymentRequest.CreatedOn,
                CreatedByUserName = paymentRequest.CreatedByUser?.UserName ?? "N/A",
                CreatedByUserId = paymentRequest.CreatedByUserId,
                BranchCode = paymentRequest.BranchCode,
                BranchName = branchName,
                DepartmentCode = paymentRequest.DepartmentCode,
                DepartmentName = departmentName,
                PaymentModeId = paymentRequest.PaymentModeId,
                PaymentTypeId = paymentRequest.PaymentTypeId,
                PaymentNatureId = paymentRequest.PaymentNatureId,
                PaymentNatureName = paymentNatureName,
                PaymentSubNatureId = paymentRequest.PaymentSubNatureId,
                PaymentSubNatureName = paymentSubNatureName,
                SelfApplicant = paymentRequest.SelfApplicant ?? false,
                PIVNo = paymentRequest.PIVNo,
                CSNo = paymentRequest.CSNo,
                CurrencyId = paymentRequest.CurrencyId,
                IsGRNBased = paymentRequest.GoodsReceiptNoteId.HasValue,
                GRNNumber = paymentRequest.GoodsReceiptNote?.GRNNumber,
                GRNReceiptDate = paymentRequest.GoodsReceiptNote?.ReceiptDate,
                PONumber = paymentRequest.GoodsReceiptNote?.PurchaseOrder?.PONumber,
                PODate = paymentRequest.GoodsReceiptNote?.PurchaseOrder?.PODate,
                SupplierName = paymentRequest.GoodsReceiptNote?.PurchaseOrder?.Supplier?.SupplierName ?? nonGrnSupplierName,
                SupplierCode = paymentRequest.GoodsReceiptNote?.PurchaseOrder?.Supplier?.NTN,
                GRNTotalAmount = paymentRequest.GoodsReceiptNote?.TotalAmount,
                SupportsDetails = paymentRequest.SupportsDetails,
                SupportsCostAllocations = paymentRequest.SupportsCostAllocations,
                IsJobRelatedPayment = paymentRequest.IsJobRelatedPayment,
                IsOtherPayment = paymentRequest.IsOtherPayment,
                Details = paymentRequest.Details.Select(d => new PaymentRequestDetailViewModel
                {
                    Id = d.Id,
                    InvoiceNo = d.InvoiceNo,
                    InvoiceDate = d.InvoiceDate,
                    Description = d.Description,
                    AmountExTax = d.AmountExTax,
                    STRate = d.STRate,
                    OtherTax = d.OtherTax,
                    TotalAmount = d.TotalAmount
                }).ToList(),
                CostAllocations = paymentRequest.CostAllocations.Select(ca => new PaymentRequestCostAllocationViewModel
                {
                    Id = ca.Id,
                    BranchCode = ca.BranchCode,
                    BranchName = ca.BranchName,
                    DepartmentCode = ca.DepartmentCode,
                    DepartmentName = ca.DepartmentName,
                    Rate = ca.Rate
                }).ToList(),
                Attachments = paymentRequest.Attachments.Select(a => new PaymentRequestAttachmentViewModel
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FileContentType = a.FileContentType,
                    FileSizeKB = a.FileSizeKB,
                    AttachmentTypeId = a.AttachmentTypeId,
                    CreatedOn = a.CreatedOn,
                    AttachmentTypeName = attachmentTypes.FirstOrDefault(at => at.Id == a.AttachmentTypeId)?.Name ?? "N/A"
                }).ToList(),
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
                AttachmentTypes = new SelectList(attachmentTypes, "Id", "Name")
            };

            var validNextStates = await _paymentRequestService.GetValidNextStatesAsync(paymentRequest);
            ViewBag.ValidNextStates = validNextStates;

            var currentUserName = GetCurrentUserName();
            var currentUserRoles = await GetUserRolesAsync(currentUserId);

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.CurrentUserName = currentUserName;
            ViewBag.CurrentUserRoles = currentUserRoles;

            return View(viewModel);
        }
        [RequirePermission("Payment Request", "Add")]
        // GET: Finance/PaymentRequest/CreateFromGRN
        public async Task<IActionResult> CreateFromGRN()
        {
            var currentUserId = GetCurrentUserId();
            var eligibleGRNs = await _paymentRequestService.GetEligibleGRNsForPaymentRequestAsync(currentUserId);

            var viewModel = new CreatePaymentRequestFromGRNViewModel
            {
                EligibleGRNs = new List<EligibleGRNViewModel>()
            };

            foreach (var grn in eligibleGRNs)
            {
                var stateName = await _workflowService.GetStateNameAsync(
                    (short)WorkFlowType.GoodsReceiptNote, grn.StateId);

                viewModel.EligibleGRNs.Add(new EligibleGRNViewModel
                {
                    Id = grn.Id,
                    POId = grn.PurchaseOrderId,
                    GRNNumber = grn.GRNNumber ?? "N/A",
                    ReceiptDate = grn.ReceiptDate,
                    PONumber = grn.PurchaseOrder?.PONumber ?? "N/A",
                    PODate = grn.PurchaseOrder?.PODate ?? DateTime.MinValue,
                    SupplierName = grn.PurchaseOrder?.Supplier?.SupplierName ?? "N/A",
                    TotalAmount = grn.Items?.Sum(item => item.TotalPrice) ?? 0,
                    StateName = stateName,
                    DepartmentName = grn.DepartmentCode,
                    CreatedOn = grn.CreatedOn,
                    ItemsCount = grn.Items?.Count ?? 0
                });
            }

            return View(viewModel);
        }

        [RequirePermission("Payment Request", "Add")]
        // GET: Finance/PaymentRequest/CreateGRNBased?grnId=5
        public async Task<IActionResult> CreateGRNBased(int? grnId)
        {

            if (grnId == null)
            {
                TempData["Error"] = "Please select a GRN to create a Payment Request.";
                return RedirectToAction(nameof(CreateFromGRN));
            }

            var grn = await _context.GoodsReceiptNotes
                .Include(g => g.PurchaseOrder)
                .ThenInclude(po => po.Supplier)
                .Include(g => g.Items) // This line is the key fix
                .FirstOrDefaultAsync(g => g.Id == grnId);

            if (grn == null)
            {
                TempData["Error"] = "Selected GRN not found.";
                return RedirectToAction(nameof(CreateFromGRN));
            }


            // Check if GRN is eligible
            if (grn.StateId != (short)PaymentRequestState.Saved)
            {
                TempData["Error"] = "Payment Request can only be created for approved/completed GRNs.";
                return RedirectToAction(nameof(CreateFromGRN));
            }


            var totalReceivedQuantity = grn.Items?.Sum(item => item.ReceivedQuantity) ?? 0;

            var currentUser = await _context.Users
                .Include(u => u.Branch)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == GetCurrentUserId());

            var fullName = (currentUser?.FirstName + " " + currentUser?.LastName)?.Trim() ?? "Unknown User";
            var viewModel = new CreateGRNBasedPaymentRequestViewModel
            {
                GoodsReceiptNoteId = grn.Id,
                GRNNumber = grn.GRNNumber ?? "N/A",
                GRNReceiptDate = grn.ReceiptDate,
                PONumber = grn.PurchaseOrder?.PONumber ?? "N/A",
                SupplierName = grn.PurchaseOrder?.Supplier?.SupplierName ?? "N/A",
                GRNReceivedQuantity = totalReceivedQuantity,
                GRNTotalAmount = grn.TotalAmount,

                // Set PayeeName, BranchCode, and DepartmentCode from the current user
                PayeeName = fullName,
                DepartmentId = (short?)(currentUser?.DepartmentId),
                BranchId = (short?)currentUser?.BranchId,
                DepartmentCode = currentUser?.DepartmentId.ToString(),
                BranchCode = currentUser?.BranchId.ToString(),

                RequiredDate = DateTime.Today.AddDays(7), // Default to 7 days from today
                CreatedByUserId = GetCurrentUserId(),
                CreatedByUserName = currentUser?.UserName ?? "Unknown User"
            };

            viewModel.Suppliers = await _context.Suppliers
             .Where(s => s.IsActive)
            .Select(pm => new SelectListItem
            {
                Value = pm.SupplierId.ToString(),
                Text = pm.SupplierName
            }).ToListAsync();

            viewModel.Branches = await _context.Branches
                .Where(s => s.IsActive)
          .Select(pm => new SelectListItem
          {
              Value = pm.BranchId.ToString(),
              Text = pm.BranchName
          })
          .ToListAsync();


            viewModel.Departments = await _context.Departments
                .Where(s => s.IsActive)
          .Select(pm => new SelectListItem
          {
              Value = pm.DepartmentId.ToString(),
              Text = pm.DepartmentName
          })
          .ToListAsync();



            viewModel.PaymentModes = await _context.PaymentModes
                .Where(s => s.IsActive)
            .Select(pm => new SelectListItem
            {
                Value = pm.PaymentModeId.ToString(),
                Text = pm.PaymentModeName
            })
            .ToListAsync();

            viewModel.PaymentTypes = await _context.PaymentTypes
                .Where(s => s.IsActive)
                .Select(pt => new SelectListItem
                {
                    Value = pt.PaymentTypeId.ToString(),
                    Text = pt.PaymentTypeName
                })
                .ToListAsync();

            viewModel.PaymentNatures = await _context.PaymentNatures
                .Where(s => s.IsActive)
           .Select(pm => new SelectListItem
           {
               Value = pm.PaymentNatureId.ToString(),
               Text = pm.PaymentNatureName
           })
           .ToListAsync();

            viewModel.SubNatures = await _context.SubNatures
    .Where(s => s.IsActive && s.SubNatureName != "HR Payments")
    .Select(pn => new SelectListItem
    {
        Value = pn.SubNatureId.ToString(),
        Text = pn.SubNatureName
    })
    .ToListAsync();

            viewModel.Companies = await _context.Companies
                .Where(s => s.IsActive)
               .Select(pn => new SelectListItem
               {
                   Value = pn.CompanyId.ToString(),
                   Text = pn.CompanyName
               })
               .ToListAsync();

            viewModel.Currencies = await _context.Currencies
                .Where(s => s.IsActive)
               .Select(pn => new SelectListItem
               {
                   Value = pn.CurrencyId.ToString(),
                   Text = pn.CurrencyName
               })
               .ToListAsync();

            return View(viewModel);

        }

        // POST: Finance/PaymentRequest/CreateGRNBased
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Payment Request", "Add")]
        public async Task<IActionResult> CreateGRNBased(CreateGRNBasedPaymentRequestViewModel model)
        {
            if (model.Details == null || !model.Details.Any() || model.Details.All(d => d.AmountExTax <= 0))
            {
                ModelState.AddModelError("Details", "At least one payment detail with an amount greater than zero is required.");
            }

            // Check if total rate for cost allocations is 100%
            if (model.CostAllocations != null && model.CostAllocations.Any())
            {
                decimal totalRate = (decimal)model.CostAllocations.Sum(ca => ca.Rate);
                if (totalRate < 0 || totalRate > 100)
                {
                    ModelState.AddModelError("CostAllocations", "The total rate for all cost allocations must be between 0 and 100.");
                }
            }

            if (model.Details != null && model.Details.Any())
            {
                var invoiceNumbers = model.Details.Select(d => d.InvoiceNo).Where(i => !string.IsNullOrEmpty(i)).ToList();

                var distinctInvoiceNumbers = invoiceNumbers.Distinct().ToList();
                if (invoiceNumbers.Count != distinctInvoiceNumbers.Count)
                {
                    ModelState.AddModelError("Details", "Duplicate Invoice Numbers are not allowed in the same request.");
                }
                else if (invoiceNumbers.Any())
                {
                    var duplicateInDatabase = await _context.PaymentRequestDetails
                        .AnyAsync(d => invoiceNumbers.Contains(d.InvoiceNo));

                    if (duplicateInDatabase)
                    {
                        ModelState.AddModelError("Details", "One or more of the provided Invoice Numbers already exist.");
                    }
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    
                    var request = new CreateGRNBasedPaymentRequestDto
                    {
                        GoodsReceiptNoteId = model.GoodsReceiptNoteId,
                        RequiredDate = model.RequiredDate,
                        PayeeName = model.PayeeName,
                        SupplierId = model.SupplierId,
                        PaymentModeId = model.PaymentModeId,
                        PaymentTypeId = model.PaymentTypeId,
                        PaymentNatureId = model.PaymentNatureId,
                        PaymentSubNatureId = model.PaymentSubNatureId,
                        CurrencyId = model.CurrencyId,
                        SelfApplicant = model.SelfApplicant,
                        Remarks = model.Remarks,
                        DepartmentId = model.DepartmentId,
                        BranchId = model.BranchId,
                        DepartmentCode = model.DepartmentCode.ToString(),
                        BranchCode = model.BranchCode,
                        CompanyCode = model.CompanyCode,
                        CreatedByUserId = GetCurrentUserId(),

                        RequestNatureId = model.RequestNatureId,
                        RequestTypeId = model.RequestTypeId,


                        Details = model.Details?.Where(d => d.AmountExTax > 0).Select(d => new PaymentRequestDetailDto
                        {
                            InvoiceNo = d.InvoiceNo,
                            InvoiceDate = d.InvoiceDate,
                            Description = d.Description,
                            AmountExTax = d.AmountExTax,
                            STRate = d.STRate,
                            OtherTax = d.OtherTax,
                            TotalAmount = d.TotalAmount
                        }).ToList() ?? new List<PaymentRequestDetailDto>(),

                        CostAllocations = model.CostAllocations?.Where(ca => ca.Rate > 0).Select(ca => new PaymentRequestCostAllocationDto
                        {
                            BranchCode = ca.BranchCode,
                            BranchName = ca.BranchName,
                            DepartmentCode = ca.DepartmentCode,
                            DepartmentName = ca.DepartmentName,
                            Rate = ca.Rate
                        }).ToList() ?? new List<PaymentRequestCostAllocationDto>(),

                    };

                    var paymentRequest = await _paymentRequestService.CreateGRNBasedPaymentRequestAsync(request);

                    TempData["Success"] = $"Payment Request {paymentRequest.PRQNumber} created successfully.";
                    return RedirectToAction(nameof(Details), new { id = paymentRequest.Id });
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Error"] = ex.Message;
                    _logger.LogWarning(ex, "Business rule violation during GRN-based Payment Request creation");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating GRN-based Payment Request");
                    TempData["Error"] = "An unexpected error occurred while creating the Payment Request.";
                }
            }

            var currentUser = await _context.Users.FindAsync(GetCurrentUserId());
            model.CreatedByUserName = currentUser?.UserName ?? "Unknown User";
            model.DepartmentCode = currentUser?.DepartmentId.ToString();
            model.BranchCode = currentUser?.BranchId.ToString();
            model.Suppliers = await _context.Suppliers.Select(pm => new SelectListItem { Value = pm.SupplierId.ToString(), Text = pm.SupplierName }).ToListAsync();
            model.Branches = await _context.Branches.Select(pm => new SelectListItem { Value = pm.BranchId.ToString(), Text = pm.BranchName }).ToListAsync();
            model.Departments = await _context.Departments.Select(pm => new SelectListItem { Value = pm.DepartmentId.ToString(), Text = pm.DepartmentName }).ToListAsync();
            model.PaymentModes = await _context.PaymentModes.Select(pm => new SelectListItem { Value = pm.PaymentModeId.ToString(), Text = pm.PaymentModeName }).ToListAsync();
            model.PaymentTypes = await _context.PaymentTypes.Select(pt => new SelectListItem { Value = pt.PaymentTypeId.ToString(), Text = pt.PaymentTypeName }).ToListAsync();
            model.PaymentNatures = await _context.PaymentNatures.Select(pm => new SelectListItem { Value = pm.PaymentNatureId.ToString(), Text = pm.PaymentNatureName }).ToListAsync();
            model.SubNatures = await _context.SubNatures.Select(pn => new SelectListItem { Value = pn.SubNatureId.ToString(), Text = pn.SubNatureName }).ToListAsync();
            model.Currencies = await _context.Currencies.Select(pn => new SelectListItem { Value = pn.CurrencyId.ToString(), Text = pn.CurrencyName }).ToListAsync();
            model.Companies = await _context.Companies.Select(pn => new SelectListItem { Value = pn.CompanyId.ToString(), Text = pn.CompanyName }).ToListAsync();
            // Reload model data on validation failure
            await ReloadGRNBasedViewModel(model);
            return View(model);
        }

        // GET: Finance/PaymentRequest/CreateDirect
        [RequirePermission("Payment Request", "Add")]
        public async Task<IActionResult> CreateDirect(short? grnId = 0)
        {
            var currentUser = await _context.Users.FindAsync(GetCurrentUserId());

            var fullName = (currentUser?.FirstName + " " + currentUser?.LastName)?.Trim() ?? "Unknown User";

            var viewModel = new CreateDirectPaymentRequestViewModel
            {
                GrnId = grnId,
                PayeeName = fullName,
                RequiredDate = DateTime.Today.AddDays(7),
                CreatedByUserId = GetCurrentUserId(),
                DepartmentId = (short?)(currentUser?.DepartmentId),
                BranchId = (short?)currentUser?.BranchId,
                // Populate the branch and department from the user's data
                DepartmentCode = currentUser?.DepartmentId.ToString(),
                BranchCode = currentUser?.BranchId.ToString(),

            };


            viewModel.AttachmentTypes = await _context.AttachmentTypes
            .Where(at => at.PaymentNatureId == 2) // Filter by PaymentNatureId
            .Select(at => new SelectListItem
            {
                Value = at.Id.ToString(), // Use the actual Id as the Value
                Text = at.Name
            })
            .ToListAsync();

            viewModel.Suppliers = await _context.Suppliers
                .Where(pm => pm.IsActive)
            .Select(pm => new SelectListItem
            {
                Value = pm.SupplierId.ToString(),
                Text = pm.SupplierName
            }).ToListAsync();

            viewModel.Branches = await _context.Branches
                .Where(pm => pm.IsActive)
          .Select(pm => new SelectListItem
          {
              Value = pm.BranchId.ToString(),
              Text = pm.BranchName
          })
          .ToListAsync();


            viewModel.Departments = await _context.Departments
                .Where(pm => pm.IsActive)
          .Select(pm => new SelectListItem
          {
              Value = pm.DepartmentId.ToString(),
              Text = pm.DepartmentName
          })
          .ToListAsync();



            viewModel.PaymentModes = await _context.PaymentModes
                .Where(pm => pm.IsActive)
            .Select(pm => new SelectListItem
            {
                Value = pm.PaymentModeId.ToString(),
                Text = pm.PaymentModeName
            })
            .ToListAsync();

            viewModel.PaymentTypes = await _context.PaymentTypes
                .Where(pm => pm.IsActive)
                .Select(pt => new SelectListItem
                {
                    Value = pt.PaymentTypeId.ToString(),
                    Text = pt.PaymentTypeName
                })
                .ToListAsync();

            viewModel.PaymentNatures = await _context.PaymentNatures
                .Where(pm => pm.IsActive)
           .Select(pm => new SelectListItem
           {
               Value = pm.PaymentNatureId.ToString(),
               Text = pm.PaymentNatureName
           })
           .ToListAsync();

         viewModel.SubNatures = await _context.SubNatures
        .Where(sn => sn.IsActive && sn.SubNatureName != "PO / WO")
         .Select(pn => new SelectListItem
         {
             Value = pn.SubNatureId.ToString(),
             Text = pn.SubNatureName
         })
         .ToListAsync();
            viewModel.Companies = await _context.Companies
                .Where(pm => pm.IsActive)
               .Select(pn => new SelectListItem
               {
                   Value = pn.CompanyId.ToString(),
                   Text = pn.CompanyName
               })
               .ToListAsync();

            viewModel.Currencies = await _context.Currencies
                .Where(pm => pm.IsActive)
              .Select(pn => new SelectListItem
              {
                  Value = pn.CurrencyId.ToString(),
                  Text = pn.CurrencyName
              })
              .ToListAsync();

            return View(viewModel);

        }

        // POST: Finance/PaymentRequest/CreateDirect
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Payment Request", "Add")]
        public async Task<IActionResult> CreateDirect(CreateDirectPaymentRequestViewModel model, short? grnId)
        {

            // Custom validation logic
            if (model.Details == null || !model.Details.Any() || model.Details.All(d => d.AmountExTax <= 0))
            {
                ModelState.AddModelError("Details", "At least one payment detail with an amount greater than zero is required.");
            }

            if (model.CostAllocations != null && model.CostAllocations.Any())
            {
                decimal totalRate = (decimal)model.CostAllocations.Sum(ca => ca.Rate);
                if (totalRate < 0 || totalRate > 100)
                {
                    ModelState.AddModelError("CostAllocations", "The total rate for all cost allocations must be between 0 and 100.");
                }
            }

            if (model.Details != null && model.Details.Any())
            {
                var invoiceNumbers = model.Details.Select(d => d.InvoiceNo).Where(i => !string.IsNullOrEmpty(i)).ToList();
                if (invoiceNumbers.Any())
                {
                    var duplicateInDatabase = await _context.PaymentRequestDetails
                        .AnyAsync(d => invoiceNumbers.Contains(d.InvoiceNo));

                    if (duplicateInDatabase)
                    {
                        ModelState.AddModelError("Details", "One or more of the provided Invoice Numbers already exist.");
                    }
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new CreateDirectPaymentRequestDto
                    {
                        RequiredDate = model.RequiredDate,
                        PayeeName = model.PayeeName,
                        SupplierId = model.SupplierId,
                        PaymentModeId = model.PaymentModeId,
                        PaymentTypeId = model.PaymentTypeId,
                        PaymentNatureId = model.PaymentNatureId,
                        PaymentSubNatureId = model.PaymentSubNatureId,
                        CurrencyId = model.CurrencyId,
                        SelfApplicant = model.SelfApplicant,
                        Remarks = model.Remarks,
                        RequestNatureId = model.RequestNatureId,
                        RequestTypeId = model.RequestTypeId,
                        DepartmentId = model.DepartmentId,
                        BranchId = model.BranchId,
                        DepartmentCode = model.DepartmentCode,
                        BranchCode = model.BranchCode,
                        CompanyCode = model.CompanyCode,
                        CreatedByUserId = GetCurrentUserId(),

                        Details = model.Details?.Where(d => d.AmountExTax > 0).Select(d => new PaymentRequestDetailDto
                        {
                            InvoiceNo = d.InvoiceNo,
                            InvoiceDate = d.InvoiceDate,
                            Description = d.Description,
                            AmountExTax = d.AmountExTax,
                            STRate = d.STRate,
                            OtherTax = d.OtherTax,
                            TotalAmount = d.TotalAmount
                        }).ToList() ?? new List<PaymentRequestDetailDto>(),

                        CostAllocations = model.CostAllocations?.Where(ca => ca.Rate > 0).Select(ca => new PaymentRequestCostAllocationDto
                        {
                            BranchCode = ca.BranchCode,
                            BranchName = ca.BranchName,
                            DepartmentCode = ca.DepartmentCode,
                            DepartmentName = ca.DepartmentName,
                            Rate = ca.Rate
                        }).ToList() ?? new List<PaymentRequestCostAllocationDto>(),

                    };

                    var paymentRequest = await _paymentRequestService.CreateDirectPaymentRequestAsync(request);

                    TempData["Success"] = $"Payment Request {paymentRequest.PRQNumber} created successfully.";
                    return RedirectToAction(nameof(Details), new { id = paymentRequest.Id });
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Error"] = ex.Message;
                    _logger.LogWarning(ex, "Business rule violation during direct Payment Request creation");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating direct Payment Request");
                    TempData["Error"] = "An unexpected error occurred while creating the Payment Request.";
                }
            }

            // Reload user info on validation failure
            var currentUser = await _context.Users.FindAsync(GetCurrentUserId());
            model.CreatedByUserName = currentUser?.UserName ?? "Unknown User";
            model.DepartmentCode = currentUser?.DepartmentId.ToString();
            model.BranchCode = currentUser?.BranchId.ToString();
            model.Suppliers = await _context.Suppliers.Select(pm => new SelectListItem { Value = pm.SupplierId.ToString(), Text = pm.SupplierName }).ToListAsync();
            model.Branches = await _context.Branches.Select(pm => new SelectListItem { Value = pm.BranchId.ToString(), Text = pm.BranchName }).ToListAsync();
            model.Departments = await _context.Departments.Select(pm => new SelectListItem { Value = pm.DepartmentId.ToString(), Text = pm.DepartmentName }).ToListAsync();
            model.PaymentModes = await _context.PaymentModes.Select(pm => new SelectListItem { Value = pm.PaymentModeId.ToString(), Text = pm.PaymentModeName }).ToListAsync();
            model.PaymentTypes = await _context.PaymentTypes.Select(pt => new SelectListItem { Value = pt.PaymentTypeId.ToString(), Text = pt.PaymentTypeName }).ToListAsync();
            model.PaymentNatures = await _context.PaymentNatures.Select(pm => new SelectListItem { Value = pm.PaymentNatureId.ToString(), Text = pm.PaymentNatureName }).ToListAsync();
            model.SubNatures = await _context.SubNatures.Select(pn => new SelectListItem { Value = pn.SubNatureId.ToString(), Text = pn.SubNatureName }).ToListAsync();
            model.Companies = await _context.Companies.Select(pn => new SelectListItem { Value = pn.CompanyId.ToString(), Text = pn.CompanyName }).ToListAsync();
            model.Currencies = await _context.Currencies.Select(pn => new SelectListItem { Value = pn.CurrencyId.ToString(), Text = pn.CurrencyName }).ToListAsync();
            model.GrnId = grnId;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PIVNo(UpdatePIVNoViewModel model)
        {
            // The ModelState.IsValid check will automatically run the validation
            // from the UpdatePIVNoViewModel.
            if (!ModelState.IsValid)
            {
                // Now that the ViewModel has PIVNo, we can access the value for a more specific error message
                TempData["Error"] = "PIV Number must be exactly 11 digits.";
                return RedirectToAction(nameof(Details), new { id = model.id });
            }

            try
            {
                var success = await _paymentRequestService.UpdatePivNoAsync(model.id, model.PIVNo); // Pass the correct property name

                if (success)
                {
                    TempData["Success"] = $"PIV Number {model.PIVNo} successfully added to Payment Request.";
                    return RedirectToAction(nameof(Details), new { id = model.id });
                }
                else
                {
                    // Now that we have a PIVNo property, we can use it here
                    TempData["Error"] = $"Payment Request not found or PIV Number could not be updated. The PIV Number '{model.PIVNo}' may already exist.";
                    return RedirectToAction(nameof(Details), new { id = model.id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating PIV No. for Payment Request {Id}", model.id);
                TempData["Error"] = "An unexpected error occurred while updating the PIV Number.";
                return RedirectToAction(nameof(Details), new { id = model.id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CSNo(UpdateCSNoViewModel model)
        {
            // The ModelState.IsValid check will automatically run the validation
            // from the UpdateCSNoViewModel.
            if (!ModelState.IsValid)
            {
                // Now that the ViewModel has CSNo, we can access the value for a more specific error message
                TempData["Error"] = "CS Number must be exactly 11 digits.";
                return RedirectToAction(nameof(Details), new { id = model.id });
            }

            try
            {
                var success = await _paymentRequestService.UpdateCSNoAsync(model.id, model.CSNo); // Pass the correct property name

                if (success)
                {
                    TempData["Success"] = $"CS Number {model.CSNo} successfully added to Payment Request.";
                    return RedirectToAction(nameof(Details), new { id = model.id });
                }
                else
                {
                    // Now that we have a CSNo property, we can use it here
                    TempData["Error"] = $"Payment Request not found or CS Number could not be updated. The CS Number '{model.CSNo}' may already exist.";
                    return RedirectToAction(nameof(Details), new { id = model.id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating CS No. for Payment Request {Id}", model.id);
                TempData["Error"] = "An unexpected error occurred while updating the CS Number.";
                return RedirectToAction(nameof(Details), new { id = model.id });
            }
        }

        // Inside your Finance/PaymentRequestController.cs
        [HttpGet]
        [RequirePermission("Payment Request", "Edit")]
        public async Task<IActionResult> EditGRNBased(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentRequest = await _context.PaymentRequests
                .Include(pr => pr.GoodsReceiptNote)
                    .ThenInclude(grn => grn.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                .Include(pr => pr.Details)
                .Include(pr => pr.Attachments)
                .Include(pr => pr.CreatedByUser)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (paymentRequest == null)
            {
                return NotFound();
            }

            // Check if the request is in an editable state
            if (paymentRequest.StateId != WorkflowService.STATE_SAVED &&
                paymentRequest.StateId != WorkflowService.STATE_RETURNED)
            {
                // Return a Forbidden status with a plain text message.
                return StatusCode(403, "Payment Request cannot be edited in its current state.");
            }

            var currentUserId = GetCurrentUserId();
            // Ensure only the creator or owner can edit
            if (paymentRequest.CreatedByUserId != currentUserId && paymentRequest.Owner != GetCurrentUserName())
            {
                // Return a Forbidden status with a plain text message.
                return StatusCode(403, "You are not authorized to edit this Payment Request.");
            }

            var currentUser = await _context.Users
                .Include(u => u.Branch)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == GetCurrentUserId());

            // Set the PayeeName to the current user's full name if the SelfApplicant checkbox is checked
            // The JavaScript will handle setting this on the client-side as well
            var fullName = (currentUser?.FirstName + " " + currentUser?.LastName)?.Trim() ?? "Unknown User";

            var viewModel = new EditGRNBasedPaymentRequestViewModel
            {
                Id = paymentRequest.Id,
                GoodsReceiptNoteId = paymentRequest.GoodsReceiptNoteId,
                GRNNumber = paymentRequest.GoodsReceiptNote?.GRNNumber ?? "N/A",
                GRNReceiptDate = paymentRequest.GoodsReceiptNote?.ReceiptDate ?? DateTime.Now,
                PONumber = paymentRequest.GoodsReceiptNote?.PurchaseOrder?.PONumber ?? "N/A",
                SupplierName = paymentRequest.GoodsReceiptNote?.PurchaseOrder?.Supplier?.SupplierName ?? "N/A",
                GRNTotalAmount = paymentRequest.GoodsReceiptNote?.TotalAmount ?? 0,
                PayeeName = fullName, // Keep existing value unless self-applicant
                RequiredDate = paymentRequest.RequiredDate,
                SelfApplicant = paymentRequest.SelfApplicant ?? false,
                DepartmentCode = paymentRequest.DepartmentCode,
                BranchCode = paymentRequest.BranchCode,
                CompanyCode = paymentRequest.CompanyCode,
                CurrencyId = paymentRequest.CurrencyId ?? 0,
                PaymentModeId = paymentRequest.PaymentModeId,
                PaymentTypeId = paymentRequest.PaymentTypeId,
                PaymentNatureId = paymentRequest.PaymentNatureId,
                PaymentSubNatureId = paymentRequest.PaymentSubNatureId,
                CreatedByUserId = paymentRequest.CreatedByUserId,
                CreatedByUserName = fullName,

                Details = paymentRequest.Details?.Select(d => new PaymentRequestDetailViewModel
                {
                    Id = d.Id,
                    InvoiceNo = d.InvoiceNo,
                    InvoiceDate = d.InvoiceDate,
                    Description = d.Description,
                    AmountExTax = d.AmountExTax,
                    STRate = d.STRate,
                    OtherTax = d.OtherTax,
                    TotalAmount = d.TotalAmount
                }).ToList(),

                Suppliers = await _context.Suppliers.Select(s => new SelectListItem { Value = s.SupplierId.ToString(), Text = s.SupplierName }).ToListAsync(),
                Departments = await _context.Departments.Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName }).ToListAsync(),
                Branches = await _context.Branches.Select(b => new SelectListItem { Value = b.BranchId.ToString(), Text = b.BranchName }).ToListAsync(),
                PaymentModes = await _context.PaymentModes.Select(pm => new SelectListItem { Value = pm.PaymentModeId.ToString(), Text = pm.PaymentModeName }).ToListAsync(),
                PaymentTypes = await _context.PaymentTypes.Select(pt => new SelectListItem { Value = pt.PaymentTypeId.ToString(), Text = pt.PaymentTypeName }).ToListAsync(),
                PaymentNatures = await _context.PaymentNatures.Select(pn => new SelectListItem { Value = pn.PaymentNatureId.ToString(), Text = pn.PaymentNatureName }).ToListAsync(),
                SubNatures = await _context.SubNatures
                    .Where(sn => sn.SubNatureName != "HR Payments")
                    .Select(sn => new SelectListItem { Value = sn.SubNatureId.ToString(), Text = sn.SubNatureName })
                    .ToListAsync(),
                Companies = await _context.Companies.Select(c => new SelectListItem { Value = c.CompanyId.ToString(), Text = c.CompanyName }).ToListAsync(),
                Currencies = await _context.Currencies.Select(c => new SelectListItem { Value = c.CurrencyId.ToString(), Text = c.CurrencyName }).ToListAsync(),
            };

            return PartialView(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Payment Request", "Edit")]
        public async Task<IActionResult> EditGRNBased(int id, EditGRNBasedPaymentRequestViewModel model)
        {
            if (model.Details == null || !model.Details.Any() || model.Details.All(d => d.AmountExTax <= 0))
            {
                ModelState.AddModelError("Details", "At least one payment detail with an amount greater than zero is required.");
            }
            

            if (id != model.Id)
            {
                TempData["Error"] = "Payment Request ID mismatch.";
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await ReloadEditGRNBasedViewModel(model);
                return View(model);
            }

            try
            {
                var existingRequest = await _context.PaymentRequests
                    .Include(pr => pr.Details)
                    .Include(pr => pr.CostAllocations)
                    .Include(pr => pr.Attachments)
                    .Include(pr => pr.GoodsReceiptNote)
                    .FirstOrDefaultAsync(pr => pr.Id == id);

                if (existingRequest == null)
                {
                    TempData["Error"] = "Payment Request not found.";
                    return NotFound();
                }

                if (existingRequest.StateId != WorkflowService.STATE_SAVED &&
                    existingRequest.StateId != WorkflowService.STATE_RETURNED)
                {
                    TempData["Error"] = "Payment Request cannot be edited in its current state.";
                    return StatusCode(403, "Payment Request cannot be edited in its current state.");
                }

                var currentUserId = GetCurrentUserId();
                if (existingRequest.CreatedByUserId != currentUserId && existingRequest.Owner != GetCurrentUserName())
                {
                    TempData["Error"] = "You are not authorized to edit this Payment Request.";
                    return StatusCode(403, "You are not authorized to edit this Payment Request.");
                }

                existingRequest.RequiredDate = model.RequiredDate;
                existingRequest.PayeeName = model.PayeeName;
                existingRequest.SelfApplicant = model.SelfApplicant;
                existingRequest.PIVNo = model.PIVNo;
                existingRequest.CSNo = model.CSNo;
                existingRequest.CurrencyId = (short)model.CurrencyId;
                existingRequest.SupplierId = (short?)model.SupplierId;
                existingRequest.PaymentModeId = model.PaymentModeId;
                existingRequest.PaymentTypeId = model.PaymentTypeId;
                existingRequest.PaymentNatureId = model.PaymentNatureId;
                existingRequest.PaymentSubNatureId = model.PaymentSubNatureId;
                existingRequest.CompanyCode = model.CompanyCode;
                existingRequest.DepartmentCode = model.DepartmentCode;
                existingRequest.BranchCode = model.BranchCode;
                existingRequest.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();
                existingRequest.UpdatedByUserId = currentUserId;

                // Handle Details (create, update, delete)
                var detailIds = model.Details?.Where(d => d.Id > 0).Select(d => d.Id).ToList() ?? new List<int>();
                var detailsToDelete = existingRequest.Details.Where(d => !detailIds.Contains(d.Id)).ToList();
                _context.PaymentRequestDetails.RemoveRange(detailsToDelete);

                if (model.Details != null)
                {
                    foreach (var detailModel in model.Details)
                    {
                        if (detailModel.Id > 0)
                        {
                            var existingDetail = existingRequest.Details.FirstOrDefault(d => d.Id == detailModel.Id);
                            if (existingDetail != null)
                            {
                                existingDetail.InvoiceNo = detailModel.InvoiceNo;
                                existingDetail.InvoiceDate = detailModel.InvoiceDate;
                                existingDetail.Description = detailModel.Description;
                                existingDetail.AmountExTax = detailModel.AmountExTax;
                                existingDetail.STRate = detailModel.STRate;
                                existingDetail.OtherTax = detailModel.OtherTax;
                                existingDetail.TotalAmount = detailModel.AmountExTax + detailModel.OtherTax + detailModel.STRate;
                            }
                        }
                        else
                        {
                            existingRequest.Details.Add(new PaymentRequestDetail
                            {
                                InvoiceNo = detailModel.InvoiceNo,
                                InvoiceDate = detailModel.InvoiceDate,
                                Description = detailModel.Description,
                                AmountExTax = detailModel.AmountExTax,
                                STRate = detailModel.STRate,
                                OtherTax = detailModel.OtherTax,
                                TotalAmount = detailModel.AmountExTax + detailModel.OtherTax + detailModel.STRate
                            });
                        }
                    }
                }


                await _context.SaveChangesAsync();
                TempData["Success"] = $"Payment Request {existingRequest.PRQNumber} updated successfully.";
                return RedirectToAction(nameof(Details), new { id = existingRequest.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentRequestExists(model.Id))
                {
                    TempData["Error"] = "The Payment Request you are trying to edit was deleted by another user.";
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Concurrency conflict updating Payment Request {Id}", id);
                    TempData["Error"] = "A concurrency error occurred. The record was modified by another user. Please try again.";
                    await ReloadEditGRNBasedViewModel(model);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing GRN-based Payment Request with ID {Id}", id);
                TempData["Error"] = "An unexpected error occurred while updating the Payment Request.";
            }

            await ReloadEditGRNBasedViewModel(model);
            return View(model);
        }

        [HttpGet]
        [RequirePermission("Payment Request", "Edit")]
        public async Task<IActionResult> EditDirect(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentRequest = await _context.PaymentRequests
                .Include(pr => pr.GoodsReceiptNote)
                    .ThenInclude(grn => grn.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                .Include(pr => pr.Details)
                .Include(pr => pr.CostAllocations)
                .Include(pr => pr.Attachments)
                .Include(pr => pr.CreatedByUser)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (paymentRequest == null)
            {
                return NotFound();
            }

            // Check if the request is in an editable state
            if (paymentRequest.StateId != WorkflowService.STATE_SAVED &&
                paymentRequest.StateId != WorkflowService.STATE_RETURNED)
            {
                // Return a Forbidden status with a plain text message.
                return StatusCode(403, "Payment Request cannot be edited in its current state.");
            }

            
            var currentUserId = GetCurrentUserId();
            // Ensure only the creator or owner can edit
            if (paymentRequest.CreatedByUserId != currentUserId && paymentRequest.Owner != GetCurrentUserName())
            {
                // Return a Forbidden status with a plain text message.
                return StatusCode(403, "You are not authorized to edit this Payment Request.");
            }

            var currentUser = await _context.Users
                .Include(u => u.Branch)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == GetCurrentUserId());

            var fullName = (currentUser?.FirstName + " " + currentUser?.LastName)?.Trim() ?? "Unknown User";
            // Now, map the PaymentRequest entity to your view model
            var viewModel = new EditDirectPaymentRequestViewModel
            {
                Id = paymentRequest.Id,
                PayeeName = fullName,
                RequiredDate = paymentRequest.RequiredDate,
                SelfApplicant = paymentRequest.SelfApplicant ?? false,
                DepartmentCode = paymentRequest.DepartmentCode,
                BranchCode = paymentRequest.BranchCode,
                CompanyCode = paymentRequest.CompanyCode,
                CurrencyId = paymentRequest.CurrencyId ?? 0,
                PaymentModeId = paymentRequest.PaymentModeId,
                PaymentTypeId = paymentRequest.PaymentTypeId,
                PaymentNatureId = paymentRequest.PaymentNatureId,
                PaymentSubNatureId = paymentRequest.PaymentSubNatureId,
                CreatedByUserId = paymentRequest.CreatedByUserId,
                CreatedByUserName = GetCurrentUserName(),

                Details = paymentRequest.Details?.Select(d => new PaymentRequestDetailViewModel
                {
                    Id = d.Id,
                    InvoiceNo = d.InvoiceNo,
                    InvoiceDate = d.InvoiceDate,
                    Description = d.Description,
                    AmountExTax = d.AmountExTax,
                    STRate = d.STRate,
                    OtherTax = d.OtherTax,
                    TotalAmount = d.TotalAmount
                }).ToList(),

                CostAllocations = paymentRequest.CostAllocations?.Select(ca => new PaymentRequestCostAllocationViewModel
                {
                    Id = ca.Id,
                    BranchCode = ca.BranchCode,
                    BranchName = ca.BranchName,
                    DepartmentCode = ca.DepartmentCode,
                    DepartmentName = ca.DepartmentName,
                    Rate = ca.Rate
                }).ToList(),

                Departments = await _context.Departments
                 .Where(d => d.IsActive)
                 .Select(d => new SelectListItem
                 {
                     // Using ID for the Value property
                     Value = d.DepartmentId.ToString(),
                     Text = d.DepartmentName
                 })
                 .ToListAsync(),

                     Branches = await _context.Branches
                 .Where(b => b.IsActive)
                 .Select(b => new SelectListItem
                 {
                     // Using ID for the Value property
                     Value = b.BranchId.ToString(),
                     Text = b.BranchName
                 })
                 .ToListAsync(),

                Suppliers = await _context.Suppliers.Select(s => new SelectListItem { Value = s.SupplierId.ToString(), Text = s.SupplierName }).ToListAsync(),
                PaymentModes = await _context.PaymentModes.Select(pm => new SelectListItem { Value = pm.PaymentModeId.ToString(), Text = pm.PaymentModeName }).ToListAsync(),
                PaymentTypes = await _context.PaymentTypes.Select(pt => new SelectListItem { Value = pt.PaymentTypeId.ToString(), Text = pt.PaymentTypeName }).ToListAsync(),
                PaymentNatures = await _context.PaymentNatures.Select(pn => new SelectListItem { Value = pn.PaymentNatureId.ToString(), Text = pn.PaymentNatureName }).ToListAsync(),
                SubNatures = await _context.SubNatures.Where(sn => sn.SubNatureName != "PO / WO").Select(c => new SelectListItem { Value = c.SubNatureId.ToString(), Text = c.SubNatureName }).ToListAsync(),
                
                Companies = await _context.Companies.Select(c => new SelectListItem { Value = c.CompanyId.ToString(), Text = c.CompanyName }).ToListAsync(),
                Currencies = await _context.Currencies.Select(c => new SelectListItem { Value = c.CurrencyId.ToString(), Text = c.CurrencyName }).ToListAsync(),
            };

            return PartialView(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Payment Request", "Edit")]
        public async Task<IActionResult> EditDirect(int id, EditDirectPaymentRequestViewModel model)
        {
            if (model.Details == null || !model.Details.Any() || model.Details.All(d => d.AmountExTax <= 0))
            {
                ModelState.AddModelError("Details", "At least one payment detail with an amount greater than zero is required.");
            }

            // Check if new attachments were uploaded without an attachment type
            //if (model.NewAttachments != null && model.NewAttachments.Any() && model.AttachmentTypeId == 0)
            //{
            //    ModelState.AddModelError("AttachmentTypeId", "Please select an Attachment Type for the uploaded files.");
            //}

            if (id != model.Id)
            {
                TempData["Error"] = "Payment Request ID mismatch.";
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await ReloadEditDirectViewModel(model);
                return View(model);
            }

            try
            {
                var existingRequest = await _context.PaymentRequests
                    .Include(pr => pr.Details)
                    .Include(pr => pr.CostAllocations)
                    .Include(pr => pr.Attachments)
                    .Include(pr => pr.GoodsReceiptNote)
                    .FirstOrDefaultAsync(pr => pr.Id == id);

                if (existingRequest == null)
                {
                    TempData["Error"] = "Payment Request not found.";
                    return NotFound();
                }

                if (existingRequest.StateId != WorkflowService.STATE_SAVED &&
                    existingRequest.StateId != WorkflowService.STATE_RETURNED)
                {
                    TempData["Error"] = "Payment Request cannot be edited in its current state.";
                    return StatusCode(403, "Payment Request cannot be edited in its current state.");
                }

                var currentUserId = GetCurrentUserId();
                if (existingRequest.CreatedByUserId != currentUserId && existingRequest.Owner != GetCurrentUserName())
                {
                    TempData["Error"] = "You are not authorized to edit this Payment Request.";
                    return StatusCode(403, "You are not authorized to edit this Payment Request.");
                }

                existingRequest.RequiredDate = model.RequiredDate;
                existingRequest.PayeeName = model.PayeeName;
                existingRequest.SelfApplicant = model.SelfApplicant;
                existingRequest.PIVNo = model.PIVNo;
                existingRequest.CSNo = model.CSNo;
                existingRequest.CurrencyId = (short)model.CurrencyId;
                existingRequest.SupplierId = (short?)model.SupplierId;
                existingRequest.PaymentModeId = model.PaymentModeId;
                existingRequest.PaymentTypeId = model.PaymentTypeId;
                existingRequest.PaymentNatureId = model.PaymentNatureId;
                existingRequest.PaymentSubNatureId = model.PaymentSubNatureId;
                existingRequest.CompanyCode = model.CompanyCode;
                existingRequest.DepartmentCode = model.DepartmentCode;
                existingRequest.BranchCode = model.BranchCode;
                existingRequest.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();
                existingRequest.UpdatedByUserId = currentUserId;

                // Handle Details (create, update, delete)
                var detailIds = model.Details?.Where(d => d.Id > 0).Select(d => d.Id).ToList() ?? new List<int>();
                var detailsToDelete = existingRequest.Details.Where(d => !detailIds.Contains(d.Id)).ToList();
                _context.PaymentRequestDetails.RemoveRange(detailsToDelete);

                if (model.Details != null)
                {
                    foreach (var detailModel in model.Details)
                    {
                        if (detailModel.Id > 0)
                        {
                            var existingDetail = existingRequest.Details.FirstOrDefault(d => d.Id == detailModel.Id);
                            if (existingDetail != null)
                            {
                                existingDetail.InvoiceNo = detailModel.InvoiceNo;
                                existingDetail.InvoiceDate = detailModel.InvoiceDate;
                                existingDetail.Description = detailModel.Description;
                                existingDetail.AmountExTax = detailModel.AmountExTax;
                                existingDetail.STRate = detailModel.STRate;
                                existingDetail.OtherTax = detailModel.OtherTax;
                                existingDetail.TotalAmount = detailModel.TotalAmount;
                            }
                        }
                        else
                        {
                            existingRequest.Details.Add(new PaymentRequestDetail
                            {
                                InvoiceNo = detailModel.InvoiceNo,
                                InvoiceDate = detailModel.InvoiceDate,
                                Description = detailModel.Description,
                                AmountExTax = detailModel.AmountExTax,
                                STRate = detailModel.STRate,
                                OtherTax = detailModel.OtherTax,
                                TotalAmount = detailModel.TotalAmount
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Payment Request {existingRequest.PRQNumber} updated successfully.";
                return RedirectToAction(nameof(Details), new { id = existingRequest.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentRequestExists(model.Id))
                {
                    TempData["Error"] = "The Payment Request you are trying to edit was deleted by another user.";
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Concurrency conflict updating Payment Request {Id}", id);
                    TempData["Error"] = "A concurrency error occurred. The record was modified by another user. Please try again.";
                    await ReloadEditDirectViewModel(model);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing GRN-based Payment Request with ID {Id}", id);
                TempData["Error"] = "An unexpected error occurred while updating the Payment Request.";
            }

            await ReloadEditDirectViewModel(model);
            return View(model);
        }

        private bool PaymentRequestExists(int id)
        {
            return _context.PaymentRequests.Any(e => e.Id == id);
        }

        // POST: Finance/PaymentRequest/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            try
            {
                var paymentRequest = await _paymentRequestService.SubmitPaymentRequestAsync(id, GetCurrentUserId());

                var nextStateName = await _workflowService.GetStateNameAsync(
                    paymentRequest.WorkFlowTypeId, paymentRequest.StateId);

                TempData["Success"] = $"Payment Request {paymentRequest.PRQNumber} has been submitted for {nextStateName}.";
                return RedirectToAction(nameof(Details), new { id = paymentRequest.Id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                _logger.LogWarning(ex, "Business rule violation during Payment Request submission");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting Payment Request {Id}", id);
                TempData["Error"] = "An unexpected error occurred while submitting the Payment Request.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Finance/PaymentRequest/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _paymentRequestService.DeletePaymentRequestAsync(id);
                if (success)
                {
                    TempData["Success"] = "Payment Request deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Payment Request not found or could not be deleted.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                _logger.LogWarning(ex, "Business rule violation during Payment Request deletion");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Payment Request {Id}", id);
                TempData["Error"] = "An unexpected error occurred while deleting the Payment Request.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Finance/PaymentRequest/GetAttachmentTypes
        [HttpGet]
        public async Task<IActionResult> GetAttachmentTypes(short? paymentNatureId = null, short? stateId = null)
        {
            try
            {
                var attachmentTypes = await _paymentRequestService.GetAttachmentTypesForPaymentRequestAsync(
                    paymentNatureId, stateId);

                return Json(attachmentTypes.Select(at => new {
                    Id = at.Id,
                    Name = at.Name
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachment types");
                return Json(new { error = "Error loading attachment types" });
            }
        }

        // POST: Finance/PaymentRequest/WorkflowAction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WorkflowActionPRQ(int id, string action, string comments = "", int? returnToUserId = null, bool returnToPrevious = false, int? submitToUserId = null, int? approveToUserId = null)
        {
            var paymentRequest = await _context.PaymentRequests
                .Include(pr => pr.CreatedByUser)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (paymentRequest == null) return NotFound();

            var currentUserId = GetCurrentUserId();

            bool canPerform = await _workflowService.CanUserPerformActionAsync(
                currentUserId, paymentRequest, action);

            if (!canPerform)
            {
                TempData["Error"] = $"You are not authorized to {action} this Payment Request.";
                return RedirectToAction(nameof(Details), new { id });
            }

            WorkflowActionResult result;

            if (action.Equals("submit", StringComparison.OrdinalIgnoreCase))
            {
                if (!submitToUserId.HasValue)
                {
                    TempData["Error"] = "Please select a user to submit the request to.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var workflowResult = await _workflowService.ProcessWorkflowActionAsync(
                    paymentRequest, action, comments, currentUserId, submitToUserId);

                result = new WorkflowActionResult
                {
                    Success = workflowResult.Success,
                    Message = workflowResult.Message,
                    NextApprover = workflowResult.NextApprover
                };
            }
            else if (action.Equals("approve", StringComparison.OrdinalIgnoreCase))
            {
                // Removed the hardcoded PAYBLE_ROLE_ID and the associated logic.
                // The service method will now handle determining if this is the final approval
                // based on the workflow sequence configuration for the Payment Request.
                var workflowResult = await _workflowService.ProcessWorkflowActionAsync(
                    paymentRequest, action, comments, currentUserId, approveToUserId);

                result = new WorkflowActionResult
                {
                    Success = workflowResult.Success,
                    Message = workflowResult.Message,
                    NextApprover = workflowResult.NextApprover
                };
            }
            else if (action.Equals("return", StringComparison.OrdinalIgnoreCase))
            {
                int targetUserId;

                if (returnToPrevious)
                {
                    var previousUserResult = await GetPreviousUserForReturn(paymentRequest);
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

                result = await ProcessReturnToSpecificUser(paymentRequest, targetUserId, comments, currentUserId);
            }
            else
            {
                // Default logic for other actions (reject, cancel).
                var workflowResult = await _workflowService.ProcessWorkflowActionAsync(
                    paymentRequest, action, comments, currentUserId);

                result = new WorkflowActionResult
                {
                    Success = workflowResult.Success,
                    Message = workflowResult.Message,
                    NextApprover = workflowResult.NextApprover
                };
            }

            if (result.Success)
            {
                _context.Update(paymentRequest);
                await _context.SaveChangesAsync();

                TempData["Success"] = result.Message;

                if (!string.IsNullOrEmpty(paymentRequest.Owner) && paymentRequest.Owner != "System" && paymentRequest.Owner != (paymentRequest.CreatedByUser?.UserName ?? "Unknown Creator"))
                {
                    TempData["Info"] = $"Next assignee: {paymentRequest.Owner}";
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
        private async Task<PreviousUserResult> GetPreviousUserForReturn(PaymentRequest paymentRequest)
        {
            var currentUserId = GetCurrentUserId();

            try
            {
                // First, try to get the most recent user from workflow history (excluding current user)
                var lastHistoryEntry = await _context.FormHistories
                    .Where(fh => fh.WorkFlowTypeId == paymentRequest.WorkFlowTypeId &&
                                 fh.FormId == paymentRequest.FormId &&
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
                if (paymentRequest.CreatedByUserId != currentUserId)
                {
                    var requester = await _context.Users.FindAsync(paymentRequest.CreatedByUserId);
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
                _logger.LogError(ex, "Error finding previous user for return on purchase request {PaymentRequestId}", paymentRequest.Id);
                return new PreviousUserResult
                {
                    Success = false,
                    Message = "An error occurred while finding the previous user."
                };
            }
        }

        private async Task<WorkflowActionResult> ProcessReturnToSpecificUser(
    PaymentRequest paymentRequest, // Model is PaymentRequest
    int returnToUserId,
    string comments,
    int currentUserId)
        {
            var returnToUser = await _context.Users.FindAsync(returnToUserId);
            if (returnToUser == null)
            {
                return new WorkflowActionResult
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
            // This finds the highest step the user is *authorized* to approve based on their roles.
            var targetUserHighestApprovalSequence = await _context.WorkFlowApprovalSequences
                .Where(ws => ws.WorkFlowTypeId == paymentRequest.WorkFlowTypeId &&
                             ws.IsActive &&
                             targetUserRoles.Contains(ws.RoleID) &&
                             ws.PaymentNatureID == paymentRequest.PaymentNatureId && // Added specific filtering for accuracy
                             ws.PaymentSubNatureID == paymentRequest.PaymentSubNatureId) // Added specific filtering for accuracy
                .OrderByDescending(ws => ws.ApprovalSeq)
                .Select(ws => (int?)ws.ApprovalSeq)
                .FirstOrDefaultAsync();

            // 3. Determine the sequence the request should be returned to.
            // This sequence number will be used by the submit logic to calculate the skip ahead.
            int returnedToSequence = targetUserHighestApprovalSequence.HasValue
                ? targetUserHighestApprovalSequence.Value
                : 0;

            // Preserve original state for history
            short fromStateId = paymentRequest.StateId;

            // Update request state and owner
            paymentRequest.StateId = WorkflowService.STATE_RETURNED;
            paymentRequest.Owner = returnToUser.UserName ?? returnToUser.Email;

            // Set CurrentApprovalSequence based on the target user's role hierarchy
            paymentRequest.CurrentApprovalSequence = returnedToSequence;

            // Reset status flags
            paymentRequest.SyncStatusWithState();
            paymentRequest.Approved = false;
            paymentRequest.Rejected = false;
            paymentRequest.IsCompleted = false;

            // Add workflow history entry.
            await _workflowService.AddFormHistoryAsync(
                paymentRequest.WorkFlowTypeId,
                paymentRequest.FormId,
                fromStateId,
                WorkflowService.STATE_RETURNED,
                "Returned",
                comments,
                currentUserId,
                returnToUserId);

            return new WorkflowActionResult
            {
                Success = true,
                Message = $"Payment Request returned to {returnToUser.UserName} successfully. Approval sequence reset to {returnedToSequence}.",
                NextApprover = returnToUser
            };
        }
        private async Task ReloadGRNBasedViewModel(CreateGRNBasedPaymentRequestViewModel model)
        {
            var grn = await _context.GoodsReceiptNotes
                .Include(g => g.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .FirstOrDefaultAsync(g => g.Id == model.GoodsReceiptNoteId);

            if (grn != null)
            {
                model.GRNNumber = grn.GRNNumber ?? "N/A";
                model.GRNReceiptDate = grn.ReceiptDate;
                model.PONumber = grn.PurchaseOrder?.PONumber ?? "N/A";
                model.SupplierName = grn.PurchaseOrder?.Supplier?.SupplierName ?? "N/A";
                model.GRNTotalAmount = grn.TotalAmount;
            }

            var currentUser = await _context.Users.FindAsync(GetCurrentUserId());
            model.CreatedByUserName = currentUser?.UserName ?? "Unknown User";
        }
        private async Task ReloadEditGRNBasedViewModel(EditGRNBasedPaymentRequestViewModel model)
        {
            // Reload all dropdowns
            model.Suppliers = await _context.Suppliers.Select(s => new SelectListItem { Value = s.SupplierId.ToString(), Text = s.SupplierName }).ToListAsync();
            model.Departments = await _context.Departments.Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName }).ToListAsync();
            model.Branches = await _context.Branches.Select(b => new SelectListItem { Value = b.BranchId.ToString(), Text = b.BranchName }).ToListAsync();
            model.PaymentModes = await _context.PaymentModes.Select(pm => new SelectListItem { Value = pm.PaymentModeId.ToString(), Text = pm.PaymentModeName }).ToListAsync();
            model.PaymentTypes = await _context.PaymentTypes.Select(pt => new SelectListItem { Value = pt.PaymentTypeId.ToString(), Text = pt.PaymentTypeName }).ToListAsync();
            model.PaymentNatures = await _context.PaymentNatures.Select(pn => new SelectListItem { Value = pn.PaymentNatureId.ToString(), Text = pn.PaymentNatureName }).ToListAsync();
            model.SubNatures = await _context.SubNatures.Select(sn => new SelectListItem { Value = sn.SubNatureId.ToString(), Text = sn.SubNatureName }).ToListAsync();
            model.Companies = await _context.Companies.Select(c => new SelectListItem { Value = c.CompanyId.ToString(), Text = c.CompanyName }).ToListAsync();
            model.Currencies = await _context.Currencies.Select(c => new SelectListItem { Value = c.CurrencyId.ToString(), Text = c.CurrencyName }).ToListAsync();

            // Reload the list of attachment types based on the selected PaymentNatureId
            model.AttachmentTypes = await _context.AttachmentTypes
                .Where(at => at.PaymentNatureId == model.PaymentNatureId)
                .Select(at => new SelectListItem { Value = at.Id.ToString(), Text = at.Name }).ToListAsync();

            // Retrieve and reload existing attachments from the database
            if (model.Id > 0)
            {
                var existingRequest = await _context.PaymentRequests
                    .Include(pr => pr.Attachments)
                        .ThenInclude(a => a.AttachmentType)
                    .FirstOrDefaultAsync(pr => pr.Id == model.Id);

                if (existingRequest != null)
                {
                    model.ExistingAttachments = existingRequest.Attachments.Select(a => new PaymentRequestAttachmentViewModel
                    {
                        Id = a.Id,
                        FileName = a.FileName,
                        FileSizeKB = a.FileSizeKB,
                        AttachmentTypeId = a.AttachmentTypeId,
                        AttachmentTypeName = a.AttachmentType?.Name
                    }).ToList();
                }
            }
        }

        private async Task ReloadEditDirectViewModel(EditDirectPaymentRequestViewModel model)
        {
            // Reload all dropdowns
            model.Suppliers = await _context.Suppliers.Select(s => new SelectListItem { Value = s.SupplierId.ToString(), Text = s.SupplierName }).ToListAsync();
            model.Departments = await _context.Departments.Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName }).ToListAsync();
            model.Branches = await _context.Branches.Select(b => new SelectListItem { Value = b.BranchId.ToString(), Text = b.BranchName }).ToListAsync();
            model.PaymentModes = await _context.PaymentModes.Select(pm => new SelectListItem { Value = pm.PaymentModeId.ToString(), Text = pm.PaymentModeName }).ToListAsync();
            model.PaymentTypes = await _context.PaymentTypes.Select(pt => new SelectListItem { Value = pt.PaymentTypeId.ToString(), Text = pt.PaymentTypeName }).ToListAsync();
            model.PaymentNatures = await _context.PaymentNatures.Select(pn => new SelectListItem { Value = pn.PaymentNatureId.ToString(), Text = pn.PaymentNatureName }).ToListAsync();
            model.SubNatures = await _context.SubNatures.Select(sn => new SelectListItem { Value = sn.SubNatureId.ToString(), Text = sn.SubNatureName }).ToListAsync();
            model.Companies = await _context.Companies.Select(c => new SelectListItem { Value = c.CompanyId.ToString(), Text = c.CompanyName }).ToListAsync();
            model.Currencies = await _context.Currencies.Select(c => new SelectListItem { Value = c.CurrencyId.ToString(), Text = c.CurrencyName }).ToListAsync();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAttachments(int paymentRequestId, List<IFormFile> Attachments, short AttachmentTypeId)
        {
            if (Attachments == null || !Attachments.Any())
            {
                TempData["Error"] = "No files were selected for upload.";
                return RedirectToAction(nameof(Details), new { id = paymentRequestId });
            }

            var existingPaymentRequest = await _context.PaymentRequests
                .Include(pr => pr.Attachments)
                .FirstOrDefaultAsync(pr => pr.Id == paymentRequestId);

            if (existingPaymentRequest == null)
            {
                TempData["Error"] = "Payment Request not found.";
                return RedirectToAction(nameof(Index));
            }

            var filesAdded = 0;
            var filesSkipped = 0;
            var existingFileNames = existingPaymentRequest.Attachments.Select(a => a.FileName).ToHashSet();

            foreach (var file in Attachments)
            {
                if (file == null || file.Length == 0) continue;

                // Check for duplicate file names before processing
                if (existingFileNames.Contains(file.FileName))
                {
                    filesSkipped++;
                    _logger.LogInformation("Skipped duplicate attachment: {FileName} for PRQ ID: {PaymentRequestId}", file.FileName, paymentRequestId);
                    continue;
                }

                try
                {
                    using var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    var newAttachment = new PaymentRequestAttachment
                    {
                        PaymentRequestId = paymentRequestId,
                        FileName = file.FileName,
                        FileContentType = file.ContentType,
                        FileContent = memoryStream.ToArray(),
                        FileSizeKB = file.Length / 1024.0m, // Convert bytes to KB
                        AttachmentTypeId = AttachmentTypeId, // <--- New line added to save the type ID
                        CreatedOn = DateTime.UtcNow,
                        CreatedByUserId = GetCurrentUserId()
                    };

                    existingPaymentRequest.Attachments.Add(newAttachment);
                    filesAdded++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving attachment {FileName} for PRQ ID {PaymentRequestId}", file.FileName, paymentRequestId);
                }
            }

            if (filesAdded > 0)
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = $"{filesAdded} attachment(s) uploaded successfully.";
            }

            if (filesSkipped > 0)
            {
                var message = filesAdded > 0
                    ? $"{filesAdded} file(s) uploaded. {filesSkipped} duplicate file(s) skipped."
                    : $"{filesSkipped} duplicate file(s) were skipped.";
                TempData["Info"] = message;
            }
            else if (filesAdded == 0 && filesSkipped == 0)
            {
                TempData["Error"] = "No new attachments were uploaded.";
            }

            return RedirectToAction(nameof(Details), new { id = paymentRequestId });
        }

        public async Task<IActionResult> ViewAttachment(int id)
        {
            var attachment = await _context.PaymentRequestAttachments.FindAsync(id);
            if (attachment == null)
            {
                return NotFound();
            }

            // Set the Content-Disposition header to 'inline'
            var contentDispositionHeader = new System.Net.Mime.ContentDisposition
            {
                FileName = attachment.FileName,
                Inline = true, // This is the key difference
            };

            Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());
            return File(attachment.FileContent, attachment.FileContentType);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            // Find the attachment by its ID in the database
            var attachment = await _context.PaymentRequestAttachments.FindAsync(id);

            // If the attachment is not found, return a 404 Not Found error
            if (attachment == null)
            {
                _logger.LogWarning("Attempted to download non-existent attachment with ID: {AttachmentId}", id);
                return NotFound();
            }

            try
            {
                // Return the file content to the user
                // The File() method handles setting the Content-Disposition header to trigger a download
                return File(attachment.FileContent, attachment.FileContentType, attachment.FileName);
            }
            catch (Exception ex)
            {
                // Log the exception to help with debugging
                _logger.LogError(ex, "Error occurred while attempting to download attachment with ID: {AttachmentId}", id);

                // Return a server error and a user-friendly message
                return StatusCode(500, "An error occurred while processing the download request.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachment(int id, int paymentRequestId)
        {
            var attachment = await _context.PaymentRequestAttachments
                .FirstOrDefaultAsync(a => a.Id == id && a.PaymentRequestId == paymentRequestId);

            if (attachment == null)
            {
                TempData["Error"] = "Attachment not found.";
                return RedirectToAction(nameof(Details), new { id = paymentRequestId });
            }

            // Add a security and state check here to ensure the user can actually delete
            // This is a simplified check. You may need to add more robust logic.
            var paymentRequest = await _context.PaymentRequests
                .FirstOrDefaultAsync(pr => pr.Id == paymentRequestId);

            if (paymentRequest == null || paymentRequest.StateId != 1)
            {
                TempData["Error"] = "Cannot delete attachments in the current state.";
                return RedirectToAction(nameof(Details), new { id = paymentRequestId });
            }

            try
            {
                _context.PaymentRequestAttachments.Remove(attachment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Attachment deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId}", id);
                TempData["Error"] = "An error occurred while deleting the attachment. Please try again.";
            }

            return RedirectToAction(nameof(Details), new { id = paymentRequestId });
        }
    }
}