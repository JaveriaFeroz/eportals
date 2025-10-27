using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Common.Enums;
using ProcureToPay.Areas.Common.Services;
using ProcureToPay.Areas.Procurement.Enums;
using ProcureToPay.Areas.Procurement.Models;
using ProcureToPay.Areas.UserManagement.Attributes;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Data;
using ProcureToPay.Helpers;
using System.Linq;
using System.Security.Claims;


namespace ProcureToPay.Areas.Procurement.Controllers
{
    [Area("Procurement")]
    [Authorize]
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

        // Helper class for workflow action results
        public class WorkflowActionResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public object? NextApprover { get; set; } // Can be User or null
        }

        private int GetCurrentUserId()
        {
            // Ensure the user is authenticated and has a NameIdentifier claim
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            return 0; // Or throw an exception if an authenticated user must always have an ID
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
        public async Task<JsonResult> GetApprovers(int purchaseRequestId)
        {
            try
            {
                var purchaseRequest = await _context.PurchaseRequests.FindAsync(purchaseRequestId);
                if (purchaseRequest == null)
                {
                    return Json(new { success = false, message = "Purchase Request not found." });
                }

                // Get the ID of the current user who is initiating the action
                var currentUserId = GetCurrentUserId();

                // Initialize the next approval sequence
                int nextApprovalSeq;

                var currentUserRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == currentUserId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                if (currentUserRoles.Any())
                {
                    var highestApprovalSequence = await _context.WorkFlowApprovalSequences
                        .Where(ws => ws.WorkFlowTypeId == purchaseRequest.WorkFlowTypeId &&
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
                var users = await _workflowService.GetUsersForApprovalSequenceAsync(purchaseRequest, nextApprovalSeq);

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
        public async Task<JsonResult> GetPreviousUser(int purchaseRequestId)
        {
            _logger.LogInformation("🎯 GetPreviousUser called with ID: {PurchaseRequestId}", purchaseRequestId);

            try
            {
                if (purchaseRequestId <= 0)
                {
                    _logger.LogWarning("❌ Invalid purchaseRequestId: {PurchaseRequestId}", purchaseRequestId);
                    return Json(new { success = false, message = "Invalid Purchase Request ID" });
                }

                var purchaseRequest = await _context.PurchaseRequests
                    .Include(pr => pr.RequestedByUser)
                    .FirstOrDefaultAsync(pr => pr.Id == purchaseRequestId);

                if (purchaseRequest == null)
                {
                    _logger.LogWarning("❌ Purchase request not found: {PurchaseRequestId}", purchaseRequestId);
                    return Json(new { success = false, message = "Purchase request not found." });
                }

                var currentUserId = GetCurrentUserId();
                _logger.LogInformation("🔍 Current user ID: {CurrentUserId}", currentUserId);

                // Get the most recent workflow history entry (excluding current user)
                var lastHistoryEntry = await _context.FormHistories
                    .Where(fh => fh.WorkFlowTypeId == purchaseRequest.WorkFlowTypeId &&
                                 fh.FormId == purchaseRequest.FormId &&
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
                if (purchaseRequest.RequestedByUserId != currentUserId && purchaseRequest.RequestedByUser != null)
                {
                    _logger.LogInformation("✅ Found original requester: {UserName}", purchaseRequest.RequestedByUser.UserName);
                    return Json(new
                    {
                        success = true,
                        userId = purchaseRequest.RequestedByUser.Id,
                        userName = purchaseRequest.RequestedByUser.UserName,
                        email = purchaseRequest.RequestedByUser.Email,
                        displayText = $"{purchaseRequest.RequestedByUser.UserName} ({purchaseRequest.RequestedByUser.Email}) - Original Requester"
                    });
                }

                _logger.LogInformation("⚠️ No previous user found for PR: {PurchaseRequestId}", purchaseRequestId);
                return Json(new { success = false, message = "No previous user found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting previous user for purchase request {PurchaseRequestId}", purchaseRequestId);
                return Json(new { success = false, message = "An error occurred while retrieving the previous user.", error = ex.Message });
            }
        }
     
        [Route("Procurement/PurchaseRequest/GetPreviousWorkflowUsers")]
        [HttpGet]
        public async Task<JsonResult> GetPreviousWorkflowUsers(int purchaseRequestId)
        {
            System.Diagnostics.Debug.WriteLine($"Controller hit with ID: {purchaseRequestId}");
            _logger.LogInformation("CONTROLLER HIT - Getting previous workflow users for purchase request {PurchaseRequestId}", purchaseRequestId);
            try
            {
                _logger.LogInformation("Getting previous workflow users for purchase request {PurchaseRequestId}", purchaseRequestId);

                var purchaseRequest = await _context.PurchaseRequests
                    .Include(pr => pr.RequestedByUser) // Include the requester
                    .FirstOrDefaultAsync(pr => pr.Id == purchaseRequestId);

                if (purchaseRequest == null)
                {
                    _logger.LogWarning("Purchase request {PurchaseRequestId} not found", purchaseRequestId);
                    return Json(new { success = false, message = "Purchase request not found." });
                }

                var currentUserId = GetCurrentUserId();
                _logger.LogInformation("Current user ID: {CurrentUserId}", currentUserId);

                // Get workflow history to find all unique users who have acted on this request
                var workflowHistory = await _context.FormHistories
                    .Where(fh => fh.WorkFlowTypeId == purchaseRequest.WorkFlowTypeId &&
                                 fh.FormId == purchaseRequest.FormId)
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
                if (purchaseRequest.RequestedByUser != null && purchaseRequest.RequestedByUserId != currentUserId)
                {
                    previousUsers.Add(new
                    {
                        value = purchaseRequest.RequestedByUserId.ToString(),
                        text = $"{purchaseRequest.RequestedByUser.UserName} ({purchaseRequest.RequestedByUser.Email}) - Original Requester",
                        group = new { name = "Original Requester" }
                    });
                    _logger.LogInformation("Added original requester: {UserName}", purchaseRequest.RequestedByUser.UserName);
                }

                // Add users from workflow history (excluding current user)
                var processedUserIds = new HashSet<int> { currentUserId };
                if (purchaseRequest.RequestedByUserId != currentUserId)
                {
                    processedUserIds.Add(purchaseRequest.RequestedByUserId);
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
                _logger.LogError(ex, "Error getting previous workflow users for purchase request {PurchaseRequestId}", purchaseRequestId);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while retrieving previous users.",
                    error = ex.Message
                });
            }
        }
        private async Task PopulateDropdowns()
        {
            try
            {
                var departments = await _context.Departments.ToListAsync();
                ViewBag.Departments = departments.Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName
                }).ToList();

                var branches = await _context.Branches.ToListAsync();
                ViewBag.Branches = branches.Select(b => new SelectListItem
                {
                    Value = b.BranchId.ToString(),
                    Text = b.BranchName
                }).ToList();

                ViewBag.PurchaseNatureTypes = Enum.GetValues(typeof(PurchaseNatureType))
                    .Cast<PurchaseNatureType>()
                    .Select(pnt => new SelectListItem
                    {
                        Value = ((short)pnt).ToString(),
                        Text = pnt.GetDisplayName()
                    }).ToList();

                ViewBag.PurchaseItemTypes = Enum.GetValues(typeof(PurchaseItemType))
                    .Cast<PurchaseItemType>()
                    .Select(pit => new SelectListItem
                    {
                        Value = ((short)pit).ToString(),
                        Text = pit.GetDisplayName()
                    }).ToList();

               
                ViewBag.ProductNatures = (await _context.ProductNatures
                    .Where(pn => pn.IsActive)
                    .OrderBy(pn => pn.NatureName)
                    .Select(pn => new SelectListItem
                    {
                        Value = pn.NatureId.ToString(),
                        Text = pn.NatureName
                    }).ToListAsync());

                ViewBag.ServiceNatures = (await _context.ServiceNatures
                    .Where(sn => sn.IsActive)
                    .OrderBy(sn => sn.NatureName)
                    .Select(sn => new SelectListItem
                    {
                        Value = sn.NatureId.ToString(),
                        Text = sn.NatureName
                    }).ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating dropdowns for Purchase Request");
                ViewBag.Departments = new List<SelectListItem>();
                ViewBag.Branches = new List<SelectListItem>();
                ViewBag.PurchaseNatureTypes = new List<SelectListItem>();
                ViewBag.PurchaseItemTypes = new List<SelectListItem>();
                ViewBag.ProductNatures = new List<SelectListItem>();
                ViewBag.ServiceNatures = new List<SelectListItem>();
            }
        }

        // GET: Procurement/PurchaseRequest/GetProductNaturesByPurchaseNatureType
        // GET: Procurement/PurchaseRequest/GetProductNaturesByPurchaseNatureType
        [HttpGet]
        public async Task<JsonResult> GetProductNaturesByPurchaseNatureType(PurchaseNatureType type)
        {
            IQueryable<Master.Models.ProductNature> query = _context.ProductNatures
                .Where(pn => pn.IsActive);

            if (type == PurchaseNatureType.Opex)
            {
                query = query.Where(pn => pn.IsOpex);
            }
            else if (type == PurchaseNatureType.Capex)
            {
                query = query.Where(pn => pn.IsCapex);
            }
            else
            {
                return Json(new List<SelectListItem>());
            }

            // ⭐ FIX: Filter to show only Natures that have at least one active Product
            query = query.Where(pn => _context.Products.Any(p =>
            p.ProductNatureId == pn.NatureId && p.IsActive));

            var productNatures = await query
                .OrderBy(pn => pn.NatureName)
                .Select(pn => new SelectListItem
                {
                    Value = pn.NatureId.ToString(),
                    Text = pn.NatureName
                })
                .ToListAsync();

            return Json(productNatures);
        }
        // GET: Procurement/PurchaseRequest/GetServiceNaturesByPurchaseNatureType
        [HttpGet]
        public async Task<JsonResult> GetServiceNaturesByPurchaseNatureType(PurchaseNatureType type)
        {
            IQueryable<Master.Models.ServiceNature> query = _context.ServiceNatures
                .Where(sn => sn.IsActive);

            if (type == PurchaseNatureType.Opex)
            {
                query = query.Where(sn => sn.IsOpex);
            }
            else if (type == PurchaseNatureType.Capex)
            {
                query = query.Where(sn => sn.IsCapex);
            }
            else
            {
                return Json(new List<SelectListItem>());
            }

            // ⭐ FIX: Filter to show only Natures that have at least one active Service
            query = query.Where(sn => _context.Services.Any(s =>
            s.ServiceNatureId == sn.NatureId && s.IsActive));
            var serviceNatures = await query
                .OrderBy(sn => sn.NatureName)
                .Select(sn => new SelectListItem
                {
                    Value = sn.NatureId.ToString(),
                    Text = sn.NatureName
                })
                .ToListAsync();

            return Json(serviceNatures);
        }
        [HttpGet]
        public async Task<JsonResult> GetProductsByNatureId(int? natureId)
        {
            if (!natureId.HasValue || natureId.Value == 0)
            {
                return Json(new List<SelectListItem>());
            }

            var products = await _context.Products
                .Where(p => p.IsActive && p.ProductNatureId == natureId.Value)
                .OrderBy(p => p.ProductName)
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = p.ProductName
                })
                .ToListAsync();
            return Json(products);
        }

        [HttpGet]
        public async Task<JsonResult> GetServicesByNatureId(int? natureId)
        {
            if (!natureId.HasValue || natureId.Value == 0)
            {
                return Json(new List<SelectListItem>());
            }

            var services = await _context.Services
                .Where(s => s.IsActive && s.ServiceNatureId == natureId.Value)
                .OrderBy(s => s.ServiceName)
                .Select(s => new SelectListItem
                {
                    Value = s.ServiceId.ToString(),
                    Text = s.ServiceName
                })
                .ToListAsync();
            return Json(services);
        }

        [HttpGet]
        public async Task<JsonResult> GetProductDetails(int productId)
        {
            var product = await _context.Products
                .Include(p => p.UoM) // Assuming you have a navigation property to UoM
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product != null)
            {
                return Json(new
                {
                    success = true,
                    name = product.ProductName,
                    unitPrice = product.UnitPrice,
                    uoMId = product.UoMId,
                    uoMName = product.UoM?.UoMName // Null-conditional operator for safety
                });
            }
            return Json(new { success = false, message = "Product not found." });
        }

        [HttpGet]
        public async Task<JsonResult> GetServiceDetails(int serviceId)
        {
            var service = await _context.Services
                .Include(s => s.UoM) // Assuming you have a navigation property to UoM
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

            if (service != null)
            {
                return Json(new
                {
                    success = true,
                    name = service.ServiceName,
                    unitPrice = service.UnitPrice,
                    uoMId = service.UoMId,
                    uoMName = service.UoM?.UoMName // Null-conditional operator for safety
                });
            }
            return Json(new { success = false, message = "Service not found." });
        }

        [HttpGet]
        public async Task<JsonResult> GetUoMs()
        {
            var uoms = await _context.UoMs
                .Where(u => u.IsActive) // Assuming IsActive exists for UoM
                .OrderBy(u => u.UoMName)
                .Select(u => new SelectListItem
                {
                    Value = u.UoMId.ToString(),
                    Text = u.UoMName
                })
                .ToListAsync();
            return Json(uoms);
        }

        private async Task<string> GenerateRequestNumberAsync()
        {
            var date = DateTime.Now;
            var prefix = $"PR{date:yyyyMMdd}";

            var lastRequest = await _context.PurchaseRequests
                .Where(pr => pr.RequestNumber != null && pr.RequestNumber.StartsWith(prefix))
                .OrderByDescending(pr => pr.RequestNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastRequest != null)
            {
                var lastSequence = lastRequest.RequestNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }
            return $"{prefix}{sequence:D3}";
        }

        // GET: Procurement/PurchaseRequest
        [RequirePermission("Purchase Request", "View")]
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            var currentUserName = GetCurrentUserName();
            var currentUserRoles = await GetUserRolesAsync(currentUserId);

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.CurrentUserName = currentUserName;
            ViewBag.CurrentUserRoles = currentUserRoles;

            // Step 1: Find all PR IDs created by the current user.
            var createdPrIds = await _context.PurchaseRequests
                .Where(pr => pr.RequestedByUserId == currentUserId)
                .Select(pr => pr.Id)
                .ToListAsync();

            // Step 2: Find all PR IDs where the current user performed a workflow action.
            var actedOnPrIds = await _context.FormHistories
                .Where(fh => fh.WorkFlowTypeId == (short)WorkFlowType.PurchaseRequest && fh.ActionByUserId == currentUserId)
                .Select(fh => fh.FormId)
                .ToListAsync();

            // Step 3: Find all PR IDs where the current user is the next designated approver (ToUserId).
            var assignedToUserPrIds = await _context.FormHistories
                .Where(fh => fh.WorkFlowTypeId == (short)WorkFlowType.PurchaseRequest && fh.ToUserId == currentUserId)
                .Select(fh => fh.FormId)
                .ToListAsync();

            // Step 4: Combine all unique IDs from the three lists.
            var relevantPrIds = new HashSet<int>();
            foreach (var id in createdPrIds)
            {
                relevantPrIds.Add(id);
            }
            foreach (var id in actedOnPrIds)
            {
                relevantPrIds.Add(id);
            }
            foreach (var id in assignedToUserPrIds)
            {
                relevantPrIds.Add(id);
            }

            // Step 5: Query the PurchaseRequests table using the consolidated set of IDs.
            // This is the final, efficient query that fetches only the relevant records.
            var purchaseRequests = await _context.PurchaseRequests
                .Where(pr => relevantPrIds.Contains(pr.Id))
                .Include(pr => pr.RequestedByUser)
                .Include(pr => pr.Department)
                .Include(pr => pr.Branch)
                .Include(pr => pr.ProducNature)
                .Include(pr => pr.ServiceNature)
                .OrderByDescending(pr => pr.CreatedOn)
                .ToListAsync();

            return View(purchaseRequests);
        }

        [RequirePermission("Purchase Request", "View")]
        // GET: Procurement/PurchaseRequest/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var purchaseRequest = await _context.PurchaseRequests
                .Include(pr => pr.RequestedByUser)
                .Include(pr => pr.Department)
                .Include(pr => pr.Branch)
                .Include(pr => pr.Items)
                .Include(pr => pr.ProducNature)
                .Include(pr => pr.ServiceNature)
                .Include(pr => pr.Attachments)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (purchaseRequest == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            var currentUserName = GetCurrentUserName();

            var history = await _workflowService.GetFormHistoryAsync(
                purchaseRequest.WorkFlowTypeId,
                purchaseRequest.FormId);

            // The rest of your code remains the same, as it's for populating the view.
            ViewBag.WorkflowHistory = history;
            ViewBag.CurrentStateName = await _workflowService.GetStateNameAsync(
                purchaseRequest.WorkFlowTypeId,
                purchaseRequest.StateId);

            var currentUserRoles = await GetUserRolesAsync(currentUserId);

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.CurrentUserName = currentUserName;
            ViewBag.CurrentUserRoles = currentUserRoles;

            return View(purchaseRequest);
        }

        // GET: Procurement/PurchaseRequest/Create
        [RequirePermission("Purchase Request", "Add")]
        public async Task<IActionResult> Create()
        {
            var currentUserId = GetCurrentUserId();

            var user = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            var purchaseRequest = new PurchaseRequest
            {
                RequestDate = DateTime.Now,
                RequiredDate = DateTime.Now.AddDays(7),
                RequestedByUserId = currentUserId,
                DepartmentId = user?.DepartmentId,
                BranchId = user?.BranchId,
                Status = RequestStatus.Draft,
                StateId = WorkflowService.STATE_SAVED,
                WorkFlowTypeId = (short)ProcureToPay.Areas.Common.Enums.WorkFlowType.PurchaseRequest,
                Owner = User.Identity.Name ?? "",
                DepartmentCode = user?.Department?.DepartmentCode ?? "",
                BranchCode = user?.Branch?.BranchCode ?? "",
                PurchaseNatureType = PurchaseNatureType.Opex,
                PurchaseItemType = PurchaseItemType.Goods
            };

            await PopulateDropdowns();
            return View(purchaseRequest);
        }

        // POST: Procurement/PurchaseRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Purchase Request", "Add")]
        public async Task<IActionResult> Create(PurchaseRequest purchaseRequest)
        {
            var currentUserId = GetCurrentUserId();

            ModelState.Remove("CreatedByUser"); // Audit
            ModelState.Remove("UpdatedByUser"); // Audit

            if (purchaseRequest.Items != null)
            {
                for (int i = 0; i < purchaseRequest.Items.Count; i++)
                {
                    ModelState.Remove($"Items[{i}].CreatedByUser");
                    ModelState.Remove($"Items[{i}].UpdatedByUser");
                    ModelState.Remove($"Items[{i}].Narration");
                }
            }

            if (purchaseRequest.PurchaseNatureType == 0) // Checks if a value was selected at all
            {
                ModelState.AddModelError("PurchaseNatureType", "Please select a Purchase Nature Type.");
            }

            if (purchaseRequest.PurchaseItemType == 0) // Checks if a value was selected at all
            {
                ModelState.AddModelError("PurchaseItemType", "Please select a Purchase Item Type.");
            }

            if (purchaseRequest.Items == null || !purchaseRequest.Items.Any())
            {
                // Add a model state error for the Items collection.
                ModelState.AddModelError("Items", "At least one item must be added to the purchase request.");
            }
            if (ModelState.IsValid) // Now, ModelState.IsValid will correctly run IValidatableObject and data annotations
            {
                try
                {
                    // Properties set programmatically (already removed from ModelState)
                    purchaseRequest.RequestNumber = await GenerateRequestNumberAsync();
                    purchaseRequest.RequestDate = DateTime.Now;
                    purchaseRequest.RequestedByUserId = currentUserId;
                    purchaseRequest.CreatedByUserId = currentUserId;
                    purchaseRequest.CreatedOn = DateTimeHelper.GetPakistanStandardTime();
                    purchaseRequest.StateId = WorkflowService.STATE_SAVED;
                    purchaseRequest.Owner = User.Identity.Name ?? "";
                    purchaseRequest.Status = RequestStatus.Draft;
                    purchaseRequest.WorkFlowTypeId = (short)ProcureToPay.Areas.Common.Enums.WorkFlowType.PurchaseRequest;

                    //  purchaseRequest.TotalAmount = purchaseRequest.Items?.Sum(i => i.TotalPrice) ?? 0;

                    var user = await _context.Users
                        .Include(u => u.Department)
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == currentUserId);

                    if (user != null)
                    {
                        // Set these if they are not bound from form or need to be overridden by user defaults
                        // Using ?? to preserve form-bound value if user provided one, otherwise use default
                        purchaseRequest.DepartmentId = purchaseRequest.DepartmentId ?? user.DepartmentId;
                        purchaseRequest.BranchId = purchaseRequest.BranchId ?? user.BranchId;
                        purchaseRequest.DepartmentCode = user.Department?.DepartmentCode ?? "";
                        purchaseRequest.BranchCode = user.Branch?.BranchCode ?? "";
                        // If CompanyCode is from user:
                        // purchaseRequest.CompanyCode = user.CompanyCode ?? "";
                    }

                    purchaseRequest.SyncStatusWithState();

                    // EF Core will use ProductNatureId and ServiceNatureId for saving
                    // No need to set navigation properties to null here if they were removed from ModelState.

                    _context.PurchaseRequests.Add(purchaseRequest);
                    await _context.SaveChangesAsync();

                    await _workflowService.AddFormHistoryAsync(purchaseRequest.WorkFlowTypeId, purchaseRequest.FormId, 0, WorkflowService.STATE_SAVED, "Created", "Purchase Request created", currentUserId, null);

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Purchase Request created successfully.";
                    return RedirectToAction(nameof(Details), new { id = purchaseRequest.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating purchase request for user {UserId}", currentUserId);
                    TempData["Error"] = "An error occurred while creating the purchase request. Please try again.";
                }
            }
            else
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToList();

                _logger.LogWarning("ModelState validation failed for purchase request creation. Errors: {Errors}",
                    string.Join(", ", errors.Select(e => $"{e.Field}: {string.Join(", ", e.Errors)}")));
            }

            // Always repopulate dropdowns if returning to the view due to an error
            await PopulateDropdowns();
            return View(purchaseRequest);
        }

        [RequirePermission("Purchase Request", "Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var purchaseRequest = await _context.PurchaseRequests

                .Include(pr => pr.ProducNature)
                .Include(pr => pr.ServiceNature)
                .Include(pr => pr.Department)
                .Include(pr => pr.Branch)
                .Include(pr => pr.Items)
                .ThenInclude(item => item.Product) // Make sure to load product details for the item rows
                .ThenInclude(p => p.UoM)
                .Include(pr => pr.Items)
                .ThenInclude(item => item.Service) // Make sure to load service details for the item rows
                .ThenInclude(s => s.UoM)
                
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (purchaseRequest == null) return NotFound();

            // The other checks for state and user ID are good, keep them.
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

            await PopulateDropdowns(); // Populate all the viewbag data

            // Check if the request is an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // Return only the form HTML (the Partial View)
                return PartialView("_EditPurchaseRequestForm", purchaseRequest);
            }

            // For a normal full-page request, return the full View
            return View(purchaseRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Purchase Request", "Edit")]
        public async Task<IActionResult> Edit(int id, PurchaseRequest purchaseRequest)
        {
            if (id != purchaseRequest.Id)
            {
                return Json(new { success = false, message = "Invalid request ID." });
            }

            ModelState.Remove("CreatedByUser");
            ModelState.Remove("UpdatedByUser");

            if (purchaseRequest.Items != null)
            {
                for (int i = 0; i < purchaseRequest.Items.Count; i++)
                {
                    ModelState.Remove($"Items[{i}].CreatedByUser");
                    ModelState.Remove($"Items[{i}].UpdatedByUser");
                    ModelState.Remove($"Items[{i}].Narration");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRequest = await _context.PurchaseRequests
                        .Include(pr => pr.Items)
                        .FirstOrDefaultAsync(pr => pr.Id == id);

                    if (existingRequest == null)
                    {
                        return Json(new { success = false, message = "Purchase request not found." });
                    }

                    // Security check
                    if (existingRequest.CreatedByUserId != GetCurrentUserId())
                    {
                        return Json(new { success = false, message = "You can only edit your own Purchase Requests." });
                    }

                    // Update main properties
                    existingRequest.Description = purchaseRequest.Description;
                    existingRequest.RequiredDate = purchaseRequest.RequiredDate;
                    existingRequest.DepartmentId = purchaseRequest.DepartmentId;
                    existingRequest.BranchId = purchaseRequest.BranchId;
                    existingRequest.PurchaseNatureType = purchaseRequest.PurchaseNatureType;
                    existingRequest.PurchaseItemType = purchaseRequest.PurchaseItemType;
                    existingRequest.ProductNatureId = purchaseRequest.ProductNatureId;
                    existingRequest.ServiceNatureId = purchaseRequest.ServiceNatureId;
                    existingRequest.CompanyCode = purchaseRequest.CompanyCode;
                    existingRequest.UpdatedByUserId = GetCurrentUserId();
                    existingRequest.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                    // **Important: Update nested items**
                    _context.PurchaseRequestDetails.RemoveRange(existingRequest.Items);
                    if (purchaseRequest.Items != null)
                    {
                        existingRequest.Items = purchaseRequest.Items;
                    }

                    existingRequest.SyncStatusWithState();

                    _context.Update(existingRequest);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Purchase Request updated successfully.";
                    var redirectUrl = Url.Action(nameof(Details), new { id = purchaseRequest.Id });
                    return Json(new { success = true, message = "Purchase Request updated successfully.", redirectUrl = redirectUrl });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return Json(new { success = false, message = "A concurrency error occurred. Please try again.", error = ex.Message });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "An error occurred while updating the purchase request.", error = ex.Message });
                }
            }
            // New logic for when ModelState.IsValid is false
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
            );
            return Json(new { success = false, message = "Validation failed. Please correct the errors.", errors = errors });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WorkflowAction(int id, string action, string comments = "", int? returnToUserId = null, bool returnToPrevious = false, int? submitToUserId = null, int? approveToUserId = null)
        {
            var purchaseRequest = await _context.PurchaseRequests
                .Include(pr => pr.RequestedByUser)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (purchaseRequest == null) return NotFound();

            var currentUserId = GetCurrentUserId();

            bool canPerform = await _workflowService.CanUserPerformActionAsync(
                currentUserId, purchaseRequest, action);

            if (!canPerform)
            {
                TempData["Error"] = $"You are not authorized to {action} this Purchase Request.";
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
                    purchaseRequest, action, comments, currentUserId, submitToUserId);

                result = new WorkflowActionResult
                {
                    Success = workflowResult.Success,
                    Message = workflowResult.Message,
                    NextApprover = workflowResult.NextApprover
                };
            }
            else if (action.Equals("approve", StringComparison.OrdinalIgnoreCase))
            {
                // Removed hardcoded CEO_ROLE_ID check.
                // The service method will now handle determining if this is the final approval
                // based on the workflow sequence configuration.
                var workflowResult = await _workflowService.ProcessWorkflowActionAsync(
                    purchaseRequest, action, comments, currentUserId, approveToUserId);

                result = new WorkflowActionResult
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
                    var previousUserResult = await GetPreviousUserForReturn(purchaseRequest);
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

                result = await ProcessReturnToSpecificUser(purchaseRequest, targetUserId, comments, currentUserId);
            }
            else
            {
                // Default logic for other actions (reject, cancel).
                var workflowResult = await _workflowService.ProcessWorkflowActionAsync(
                    purchaseRequest, action, comments, currentUserId);

                result = new WorkflowActionResult
                {
                    Success = workflowResult.Success,
                    Message = workflowResult.Message,
                    NextApprover = workflowResult.NextApprover
                };
            }

            if (result.Success)
            {
                _context.Update(purchaseRequest);
                await _context.SaveChangesAsync();

                TempData["Success"] = result.Message;

                if (!string.IsNullOrEmpty(purchaseRequest.Owner) && purchaseRequest.Owner != "System" && purchaseRequest.Owner != (purchaseRequest.RequestedByUser?.UserName ?? "Unknown Creator"))
                {
                    TempData["Info"] = $"Next assignee: {purchaseRequest.Owner}";
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

        private async Task<PreviousUserResult> GetPreviousUserForReturn(PurchaseRequest purchaseRequest)
        {
            var currentUserId = GetCurrentUserId();

            try
            {
                // First, try to get the most recent user from workflow history (excluding current user)
                var lastHistoryEntry = await _context.FormHistories
                    .Where(fh => fh.WorkFlowTypeId == purchaseRequest.WorkFlowTypeId &&
                                 fh.FormId == purchaseRequest.FormId &&
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
                if (purchaseRequest.RequestedByUserId != currentUserId)
                {
                    var requester = await _context.Users.FindAsync(purchaseRequest.RequestedByUserId);
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
                _logger.LogError(ex, "Error finding previous user for return on purchase request {PurchaseRequestId}", purchaseRequest.Id);
                return new PreviousUserResult
                {
                    Success = false,
                    Message = "An error occurred while finding the previous user."
                };
            }
        }


        private async Task<WorkflowActionResult> ProcessReturnToSpecificUser(
    PurchaseRequest purchaseRequest,
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
            var targetUserHighestApprovalSequence = await _context.WorkFlowApprovalSequences
                .Where(ws => ws.WorkFlowTypeId == purchaseRequest.WorkFlowTypeId &&
                             ws.IsActive &&
                             targetUserRoles.Contains(ws.RoleID))
                .OrderByDescending(ws => ws.ApprovalSeq)
                .Select(ws => (int?)ws.ApprovalSeq)
                .FirstOrDefaultAsync();

            // 3. Determine the sequence the PR should be returned to.
            // If roles are found, set it to the highest sequence. If not, set to 0 (or 1) for a full restart.
            int returnedToSequence = targetUserHighestApprovalSequence.HasValue
                ? targetUserHighestApprovalSequence.Value
                : 0; // Using 0 ensures the submit logic calculates the next step correctly.


            // Update purchase request state
            short fromStateId = purchaseRequest.StateId;
            purchaseRequest.StateId = WorkflowService.STATE_RETURNED;
            purchaseRequest.Owner = returnToUser.UserName ?? returnToUser.Email;

            // ⭐ FIX: Set CurrentApprovalSequence based on the target user's role hierarchy
            purchaseRequest.CurrentApprovalSequence = returnedToSequence;

            purchaseRequest.SyncStatusWithState();
            purchaseRequest.Approved = false;
            purchaseRequest.Rejected = false;
            purchaseRequest.IsCompleted = false;

            // Add workflow history entry. Pass returnToUserId as toUserId.
            await _workflowService.AddFormHistoryAsync(
                purchaseRequest.WorkFlowTypeId,
                purchaseRequest.FormId,
                fromStateId, // The state *before* returning
                WorkflowService.STATE_RETURNED,
                "Returned", // More descriptive action for history
                comments,
                currentUserId,
                returnToUserId); // Explicitly pass the user ID to whom it's being returned

            return new WorkflowActionResult
            {
                Success = true,
                Message = $"Purchase Request returned to {returnToUser.UserName} successfully. Approval sequence reset to {returnedToSequence}.",
                NextApprover = returnToUser
            };
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
                .Include(pr => pr.ProducNature)
                .Include(pr => pr.ServiceNature)
                .Where(pr => pr.StateId == WorkflowService.STATE_SUBMITTED &&
                             pr.Owner == userName)
                .OrderBy(pr => pr.CreatedOn)
                .ToListAsync();

            return View(pendingApprovals);
        }

        private bool PurchaseRequestExists(int id)
        {
            return _context.PurchaseRequests.Any(e => e.Id == id);
        }

        [HttpPost] // <--- THIS IS CRITICAL! Ensure this attribute is present.
        public IActionResult AddPurchaseRequestDetailRow([FromQuery] int index)
        {
            var newItem = new PurchaseRequestItem
            {
                ItemName = string.Empty,
                Quantity = 1,
                UnitPrice = 0,
                GSTRate = 0,
                VATRate = 0,
                Narration = string.Empty
            };
            ViewData["index"] = index;
            return PartialView("_PurchaseRequestDetailRow", newItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAttachments(int purchaseRequestId, List<IFormFile> Attachments)
        {
            if (Attachments == null || !Attachments.Any())
            {
                TempData["Error"] = "No files were selected for upload.";
                return RedirectToAction(nameof(Details), new { id = purchaseRequestId });
            }

            var existingPurchaseRequest = await _context.PurchaseRequests
                .Include(pr => pr.Attachments)
                .FirstOrDefaultAsync(pr => pr.Id == purchaseRequestId);

            if (existingPurchaseRequest == null)
            {
                TempData["Error"] = "Purchase Request not found.";
                return RedirectToAction(nameof(Index));
            }

            var filesAdded = 0;
            var filesSkipped = 0;
            var existingFileNames = existingPurchaseRequest.Attachments.Select(a => a.FileName).ToHashSet();

            foreach (var file in Attachments)
            {
                if (file == null || file.Length == 0) continue;

                // Check for duplicate file names before processing
                if (existingFileNames.Contains(file.FileName))
                {
                    filesSkipped++;
                    _logger.LogInformation("Skipped duplicate attachment: {FileName} for PR ID: {PurchaseRequestId}", file.FileName, purchaseRequestId);
                    continue;
                }

                try
                {
                    using var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    var newAttachment = new PurchaseRequestAttachment
                    {
                        PurchaseRequestId = purchaseRequestId,
                        FileName = file.FileName,
                        FileContentType = file.ContentType,
                        FileContent = memoryStream.ToArray(),
                        FileSizeKB = file.Length / 1024.0m, // Convert bytes to KB
                        CreatedOn = DateTime.UtcNow,
                        CreatedByUserId = GetCurrentUserId()
                    };

                    existingPurchaseRequest.Attachments.Add(newAttachment);
                    filesAdded++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving attachment {FileName} for PR ID {PurchaseRequestId}", file.FileName, purchaseRequestId);
                    // Optionally, you can add an error to TempData for each failed file
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

            return RedirectToAction(nameof(Details), new { id = purchaseRequestId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachment(int id, int purchaseRequestId)
        {
            var attachment = await _context.PurchaseRequestAttachments
                .FirstOrDefaultAsync(a => a.Id == id && a.PurchaseRequestId == purchaseRequestId);

            if (attachment == null)
            {
                TempData["Error"] = "Attachment not found.";
                return RedirectToAction(nameof(Details), new { id = purchaseRequestId });
            }

            // Add a security and state check here to ensure the user can actually delete
            // This is a simplified check. You may need to add more robust logic.
            var purchaseRequest = await _context.PurchaseRequests
                .FirstOrDefaultAsync(pr => pr.Id == purchaseRequestId);

            if (purchaseRequest == null || purchaseRequest.StateId != 2)
            {
                TempData["Error"] = "Cannot delete attachments in the current state.";
                return RedirectToAction(nameof(Details), new { id = purchaseRequestId });
            }

            try
            {
                _context.PurchaseRequestAttachments.Remove(attachment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Attachment deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId}", id);
                TempData["Error"] = "An error occurred while deleting the attachment. Please try again.";
            }

            return RedirectToAction(nameof(Details), new { id = purchaseRequestId });
        }
        public async Task<IActionResult> ViewAttachment(int id)
        {
            var attachment = await _context.PurchaseRequestAttachments.FindAsync(id);
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
            var attachment = await _context.PurchaseRequestAttachments.FindAsync(id);

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
    }
}





