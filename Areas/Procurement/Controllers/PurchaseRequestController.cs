using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Common.Services;
using ProcureToPay.Areas.Procurement.Models;
using ProcureToPay.Data;
using System.Security.Claims;

namespace ProcureToPay.Areas.Procurement.Controllers
{
    [Area("Procurement")]
    public class PurchaseRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowService _workflowService;
        private readonly ILogger<PurchaseRequestController> _logger;

        public PurchaseRequestController(
            ApplicationDbContext context,
            IWorkflowService workflowService,
            ILogger<PurchaseRequestController> logger)
        {
            _context = context;
            _workflowService = workflowService;
            _logger = logger;
        }

        // Helper method to get current user ID
        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        // GET: Procurement/PurchaseRequest
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();

            var purchaseRequests = await _context.PurchaseRequests
                .Include(pr => pr.RequestedByUser)
                .Include(pr => pr.Department)
                .Include(pr => pr.Branch)
                .Where(pr => pr.RequestedByUserId == currentUserId || pr.Owner == User.Identity.Name)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync();

            return View(purchaseRequests);
        }

        // GET: Procurement/PurchaseRequest/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var purchaseRequest = await _context.PurchaseRequests
                .Include(pr => pr.RequestedByUser)
                .Include(pr => pr.Department)
                .Include(pr => pr.Branch)
                .Include(pr => pr.Items)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (purchaseRequest == null) return NotFound();

            // Get workflow history
            var history = await _workflowService.GetFormHistoryAsync(
                purchaseRequest.WorkFlowTypeId,
                purchaseRequest.FormId);

            ViewBag.WorkflowHistory = history;
            ViewBag.CurrentStateName = await _workflowService.GetStateNameAsync(
                purchaseRequest.WorkFlowTypeId,
                purchaseRequest.StateId);

            return View(purchaseRequest);
        }

        // GET: Procurement/PurchaseRequest/Create
        public IActionResult Create()
        {
            var purchaseRequest = new PurchaseRequest
            {
                RequestDate = DateTime.Now,
                RequiredDate = DateTime.Now.AddDays(7),
                RequestedByUserId = GetCurrentUserId()
            };

            return View(purchaseRequest);
        }

        // POST: Procurement/PurchaseRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseRequest purchaseRequest)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = GetCurrentUserId();

                // Set audit fields
                purchaseRequest.CreatedByUserId = currentUserId;
                purchaseRequest.CreatedOn = DateTime.UtcNow;
                purchaseRequest.CreatedAt = DateTime.UtcNow;

                // Set workflow defaults
                purchaseRequest.StateId = WorkflowService.STATE_SAVED;
                purchaseRequest.Owner = User.Identity.Name;
                purchaseRequest.RequestedByUserId = currentUserId;

                // Calculate total amount from items
                purchaseRequest.TotalAmount = purchaseRequest.Items?.Sum(i => i.TotalPrice) ?? 0;

                // Sync status with state
                purchaseRequest.SyncStatusWithState();

                _context.PurchaseRequests.Add(purchaseRequest);
                await _context.SaveChangesAsync();

                // Add initial workflow history
                await _workflowService.AddFormHistoryAsync(
                    purchaseRequest.WorkFlowTypeId,
                    purchaseRequest.FormId,
                    0, // From no state
                    WorkflowService.STATE_SAVED,
                    "Created",
                    "Purchase Request created",
                    currentUserId);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Purchase Request created successfully.";
                return RedirectToAction(nameof(Details), new { id = purchaseRequest.Id });
            }

            return View(purchaseRequest);
        }

        // GET: Procurement/PurchaseRequest/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var purchaseRequest = await _context.PurchaseRequests
                .Include(pr => pr.Items)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (purchaseRequest == null) return NotFound();

            // Check if user can edit (only in saved or returned state)
            if (purchaseRequest.StateId != WorkflowService.STATE_SAVED &&
                purchaseRequest.StateId != WorkflowService.STATE_RETURNED)
            {
                TempData["Error"] = "Purchase Request cannot be edited in its current state.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var currentUserId = GetCurrentUserId();
            if (purchaseRequest.CreatedByUserId != currentUserId)
            {
                TempData["Error"] = "You can only edit your own Purchase Requests.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(purchaseRequest);
        }

        // POST: Procurement/PurchaseRequest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PurchaseRequest purchaseRequest)
        {
            if (id != purchaseRequest.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserId = GetCurrentUserId();

                    // Update audit fields
                    purchaseRequest.UpdatedByUserId = currentUserId;
                    purchaseRequest.UpdatedOn = DateTime.UtcNow;
                    purchaseRequest.UpdatedAt = DateTime.UtcNow;

                    // Calculate total amount
                    purchaseRequest.TotalAmount = purchaseRequest.Items?.Sum(i => i.TotalPrice) ?? 0;

                    // Sync status with state
                    purchaseRequest.SyncStatusWithState();

                    _context.Update(purchaseRequest);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Purchase Request updated successfully.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseRequestExists(purchaseRequest.Id))
                        return NotFound();
                    throw;
                }
            }
            return View(purchaseRequest);
        }

        // POST: Workflow Actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WorkflowAction(int id, string action, string comments = "")
        {
            var purchaseRequest = await _context.PurchaseRequests.FindAsync(id);
            if (purchaseRequest == null) return NotFound();

            var currentUserId = GetCurrentUserId();

            // Check if user can perform this action
            bool canPerform = await _workflowService.CanUserPerformActionAsync(
                currentUserId, purchaseRequest, action);

            if (!canPerform)
            {
                TempData["Error"] = $"You are not authorized to {action} this Purchase Request.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Process the workflow action
            var result = await _workflowService.ProcessWorkflowActionAsync(
                purchaseRequest, action, comments, currentUserId);

            if (result.Success)
            {
                // Update the entity in the database
                _context.Update(purchaseRequest);
                await _context.SaveChangesAsync();

                TempData["Success"] = result.Message;

                if (result.NextApprover != null)
                {
                    TempData["Info"] = $"Next approver: {result.NextApprover.UserName}";
                }
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Pending Approvals
        public async Task<IActionResult> PendingApprovals()
        {
            var currentUserId = GetCurrentUserId();
            var userName = User.Identity.Name;

            var pendingApprovals = await _context.PurchaseRequests
                .Include(pr => pr.RequestedByUser)
                .Include(pr => pr.Department)
                .Include(pr => pr.Branch)
                .Where(pr => pr.StateId == WorkflowService.STATE_SUBMITTED &&
                           pr.Owner == userName)
                .OrderBy(pr => pr.CreatedAt)
                .ToListAsync();

            return View(pendingApprovals);
        }

        // Helper method to check if purchase request exists
        private bool PurchaseRequestExists(int id)
        {
            return _context.PurchaseRequests.Any(e => e.Id == id);
        }
    }
}