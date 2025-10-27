using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Common.Enums;
using ProcureToPay.Areas.Procurement.Enums;
using ProcureToPay.Data;
using ProcureToPay.Helpers;
using ProcureToPay.Models;
using ProcureToPay.ViewModels;
using System.Diagnostics;
using System.Security.Claims;
using static ProcureToPay.Areas.Procurement.Models.PurchaseOrder;

namespace ProcureToPay.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
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

        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            var currentUser = await _context.Users
                .Include(u => u.Branch)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == GetCurrentUserId());

            if (currentUserId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            // ⭐ FIX: Store the currentUser object instead of the pre-compiled string.
            ViewBag.CurrentUser = currentUser;

            // We don't need ViewBag.currentUserName anymore, as the view will access the object.

            var today = DateTimeHelper.GetPakistanStandardTime().Date;
            await SetAllModulesCounts(currentUserId, today);

            return View();
        }


        public IActionResult Maintenance()
        {
            // This action simply returns the Maintainance.cshtml view.
            return View();
        }

        public async Task<IActionResult> DetailsList(int? statusId, int? moduleId, string RequestNumberFilter, DateTime? MinDate, DateTime? MaxDate)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            // Set Date Range Defaults
            var today = DateTimeHelper.GetPakistanStandardTime().Date;

            // Corrected logic for date range defaults and validation
            if (!MinDate.HasValue)
            {
                MinDate = today.AddMonths(-6);
            }

            if (!MaxDate.HasValue)
            {
                MaxDate = today;
            }

            // Enforce MaxDate is not greater than today
            if (MaxDate.Value.Date > today)
            {
                MaxDate = today;
            }

            // Crucial fix for MaxDate filter: add one day and use '<'
            // This includes all records up to the end of MaxDate
            var maxDateFilter = MaxDate.Value.Date.AddDays(1);

            var requests = new List<RequestDetailViewModel>();
            string title;
            string statusName = "";

            var prIds = await GetRelevantPurchaseRequestIds(currentUserId);
            var poIds = await GetRelevantPurchaseOrderIds(currentUserId);
            var prqIds = await GetRelevantPaymentRequestIds(currentUserId);

            // ... (StatusName logic remains the same, it's correct)
            if (statusId.HasValue)
            {
                switch (statusId.Value)
                {
                    case 0: statusName = "Draft's"; break;
                    case 1: statusName = "Pending"; break;
                    case 2: statusName = "Sent"; break;
                    case 3: statusName = "Approved"; break;
                    case 4: statusName = "Rejected"; break;
                    default: statusName = "All"; break;
                }
            }
            else
            {
                statusName = "All";
            }

            // Purchase Requests
            if (!moduleId.HasValue || moduleId == (int)WorkFlowType.PurchaseRequest)
            {
                var query = _context.PurchaseRequests
                    .Where(pr => prIds.Contains(pr.Id));

                if (statusId.HasValue)
                {
                    if (statusId == 0) // Drafts
                    {
                        query = query.Where(pr => pr.StateId == 1 && pr.RequestedByUserId == currentUserId);
                    }
                    else if (statusId == 1) // Pending (all pending requests, regardless of date)
                    {
                        query = query.Where(pr => pr.Owner == User.Identity.Name && pr.StateId != 3 && pr.StateId != 1);
                    }
                    else // Other statuses (Sent, Approved, Rejected)
                    {
                        query = query.Where(pr => pr.StateId == statusId);
                    }
                }

                var prs = await query
                    .Where(pr => (!MinDate.HasValue || pr.CreatedOn.Date >= MinDate.Value.Date)
                              && (!MaxDate.HasValue || pr.CreatedOn.Date < maxDateFilter) // Corrected filter
                              && (string.IsNullOrEmpty(RequestNumberFilter) || pr.RequestNumber.Contains(RequestNumberFilter)))
                    .Include(pr => pr.Department)
                    .Include(pr => pr.Branch)
                    .OrderByDescending(pr => pr.RequestDate)
                    .ToListAsync();

                requests.AddRange(prs.Select(pr => new RequestDetailViewModel
                {
                    Id = pr.Id,
                    RequestNumber = pr.RequestNumber ?? "N/A",
                    RequestDate = pr.RequestDate,
                    Department = pr.Department?.DepartmentName ?? "N/A",
                    Branch = pr.Branch?.BranchName ?? "N/A",
                    Status = pr.StateId.ToString(),
                    CurrentOwner = pr.Owner ?? "N/A",
                    WorkFlowTypeId = (int)WorkFlowType.PurchaseRequest
                }));
            }

            // Purchase Orders
            if (!moduleId.HasValue || moduleId == (int)WorkFlowType.PurchaseOrder)
            {
                var query = _context.PurchaseOrders
                    .Where(po => poIds.Contains(po.Id));

                if (statusId.HasValue)
                {
                    if (statusId == 0) // Drafts
                    {
                        query = query.Where(po => po.StateId == 1 && po.CreatedByUserId == currentUserId);
                    }
                    else if (statusId == 1) // Pending (all pending requests, regardless of date)
                    {
                        query = query.Where(po => po.Owner == User.Identity.Name && po.StateId != 3 && po.StateId != 1);
                    }
                    else // Other statuses (Sent, Approved, Rejected)
                    {
                        query = query.Where(po => po.StateId == statusId);
                    }
                }

                var pos = await query
                    .Where(po => (!MinDate.HasValue || po.CreatedOn.Date >= MinDate.Value.Date)
                              && (!MaxDate.HasValue || po.CreatedOn.Date < maxDateFilter) // Corrected filter
                              && (string.IsNullOrEmpty(RequestNumberFilter) || po.PONumber.Contains(RequestNumberFilter)))
                    .Include(po => po.Department)
                    .Include(po => po.Branch)
                    .OrderByDescending(po => po.PODate)
                    .ToListAsync();

                requests.AddRange(pos.Select(po => new RequestDetailViewModel
                {
                    Id = po.Id,
                    RequestNumber = po.PONumber ?? "N/A",
                    RequestDate = po.PODate,
                    Department = po.Department?.DepartmentName ?? "N/A",
                    Branch = po.Branch?.BranchName ?? "N/A",
                    Status = po.StateId.ToString(),
                    CurrentOwner = po.Owner ?? "N/A",
                    WorkFlowTypeId = (int)WorkFlowType.PurchaseOrder
                }));
            }

            // Payment Requests
            if (!moduleId.HasValue || moduleId == (int)WorkFlowType.PaymentRequest)
            {
                var query = _context.PaymentRequests
                    .Where(prq => prqIds.Contains(prq.Id));

                if (statusId.HasValue)
                {
                    if (statusId == 0) // Drafts
                    {
                        query = query.Where(prq => prq.StateId == 1 && prq.CreatedByUserId == currentUserId);
                    }
                    else if (statusId == 1) // Pending 
                    {
                        query = query.Where(prq => prq.Owner == User.Identity.Name && prq.StateId != 3 && prq.StateId != 1);
                    }
                    else // Other statuses (Sent, Approved, Rejected)
                    {
                        query = query.Where(prq => prq.StateId == statusId);
                    }
                }

                // Departments aur Branches ki tables ki zaroorat nahi hai agar sirf ID chahiye,
                // lekin hum inhe join rakhenge agar aage koi aur field names se chahiye ho.

                // Agar aapke PaymentRequest model mein DepartmentId aur BranchId direct properties hain:
                var prqRequests = await query
                    .Where(prq => (!MinDate.HasValue || prq.CreatedOn.Date >= MinDate.Value.Date)
                                  && (!MaxDate.HasValue || prq.CreatedOn.Date < maxDateFilter)
                                  && (string.IsNullOrEmpty(RequestNumberFilter) || prq.PRQNumber.Contains(RequestNumberFilter)))

                    .Include(prq => prq.CreatedByUser)
                    .Include(prq => prq.Department)
                    .Include(prq => prq.Branch)
                    .OrderByDescending(prq => prq.CreatedOn)
                    .ToListAsync();

                requests.AddRange(prqRequests.Select(prq => new RequestDetailViewModel
                {
                    Id = prq.Id,
                    RequestNumber = prq.PRQNumber ?? "N/A",
                    RequestDate = prq.CreatedOn,

                    // ✅ FIX: Department ID ko Department field mein map karein
                    Department = prq.Department?.DepartmentName ?? "N/A",

                    // ✅ FIX: Branch ID ko Branch field mein map karein
                    Branch = prq.Branch?.BranchName ?? "N/A",

                    Status = prq.StateId.ToString(),
                    CurrentOwner = prq.Owner ?? "N/A",
                    WorkFlowTypeId = (int)WorkFlowType.PaymentRequest
                }).ToList());
            }
            if (moduleId.HasValue)
            {
                var module = await _context.Modules.FindAsync(moduleId.Value);
                title = $"{statusName} {module?.DisplayName ?? "Requests"}";
            }
            else
            {
                title = $"{statusName} All Modules";
            }

            ViewBag.Modules = await _context.Modules.Where(m => m.IsActive).ToListAsync();
            ViewBag.SelectedModuleId = moduleId;
            ViewBag.SelectedStatusId = statusId;
            ViewBag.RequestNumberFilter = RequestNumberFilter;
            ViewBag.MinDate = MinDate.Value.ToString("yyyy-MM-dd");
            ViewBag.MaxDate = MaxDate.Value.ToString("yyyy-MM-dd");
            ViewBag.Title = title;

            return View(requests.OrderByDescending(r => r.RequestDate).ToList());
        }
        private async Task SetAllModulesCounts(int userId, DateTime today)
        {
            // Combine all relevant IDs for each module first
            var allPrIds = await GetRelevantPurchaseRequestIds(userId);
            var allPoIds = await GetRelevantPurchaseOrderIds(userId);
            var allPrqIds = await GetRelevantPaymentRequestIds(userId);

            // Total Draft Requests: Count from all relevant IDs where state is 1
            ViewBag.TotalDraftRequests = await _context.PurchaseRequests
                 .Where(pr => pr.StateId == 1 && pr.RequestedByUserId == userId)
                 .CountAsync() +
                 await _context.PurchaseOrders
                 .Where(po => po.StateId == 1 && po.CreatedByUserId == userId)
                 .CountAsync() +
                 await _context.PaymentRequests
                 .Where(prq => prq.StateId == 1 && prq.CreatedByUserId == userId)
                 .CountAsync();

            // Total Approved Requests: Count from all relevant IDs with StateId 3
            ViewBag.TotalApprovedRequests = await _context.PurchaseRequests
                .Where(pr => allPrIds.Contains(pr.Id) && pr.StateId == 3)
                .CountAsync() +
                await _context.PurchaseOrders
                .Where(po => allPoIds.Contains(po.Id) && po.StateId == 3)
                .CountAsync() +
                await _context.PaymentRequests
                .Where(prq => allPrqIds.Contains(prq.Id) && prq.StateId == 3)
                .CountAsync();

            // Total Pending Requests: Count from all relevant IDs where owner is current user
            ViewBag.TotalPendingRequests = await _context.PurchaseRequests
                .Where(pr => pr.Owner == User.Identity.Name && pr.StateId != 3 && pr.StateId != 1) // Exclude Drafts
                .CountAsync() +
                await _context.PurchaseOrders
                .Where(po => po.Owner == User.Identity.Name && po.StateId != 3 && po.StateId != 1) // Exclude Drafts
                .CountAsync() +
                await _context.PaymentRequests
                .Where(prq => prq.Owner == User.Identity.Name && prq.StateId != 3 && prq.StateId != 1) // Exclude Drafts
                .CountAsync();

            // Total Sent Requests: Count requests created by user with StateId 2
            ViewBag.TotalSentRequests = await _context.PurchaseRequests
             .Where(pr => allPrIds.Contains(pr.Id) && pr.StateId == 2)
             .CountAsync() +
             await _context.PurchaseOrders
             .Where(po => allPoIds.Contains(po.Id) && po.StateId == 2)
             .CountAsync() +
             await _context.PaymentRequests
             .Where(prq => allPrqIds.Contains(prq.Id) && prq.StateId == 2)
             .CountAsync();

            // Total Rejected Requests: Count from all relevant IDs with StateId 4
            ViewBag.TotalRejectRequests = await _context.PurchaseRequests
                .Where(pr => allPrIds.Contains(pr.Id) && pr.StateId == 4)
                .CountAsync() +
                await _context.PurchaseOrders
                .Where(po => allPoIds.Contains(po.Id) && po.StateId == 4)
                .CountAsync() +
                await _context.PaymentRequests
                .Where(prq => allPrqIds.Contains(prq.Id) && prq.StateId == 4)
                .CountAsync();
        }

        private async Task<HashSet<int>> GetRelevantPurchaseRequestIds(int userId)
        {
            var createdIds = await _context.PurchaseRequests.Where(pr => pr.RequestedByUserId == userId).Select(pr => pr.Id).ToListAsync();
            var historyIds = await _context.FormHistories.Where(fh => fh.WorkFlowTypeId == (short)WorkFlowType.PurchaseRequest && (fh.ActionByUserId == userId || fh.ToUserId == userId)).Select(fh => fh.FormId).ToListAsync();
            return createdIds.Union(historyIds).ToHashSet();
        }

        private async Task<HashSet<int>> GetRelevantPurchaseOrderIds(int userId)
        {
            var createdIds = await _context.PurchaseOrders.Where(po => po.CreatedByUserId == userId).Select(po => po.Id).ToListAsync();
            var historyIds = await _context.FormHistories.Where(fh => fh.WorkFlowTypeId == (short)WorkFlowType.PurchaseOrder && (fh.ActionByUserId == userId || fh.ToUserId == userId)).Select(fh => fh.FormId).ToListAsync();
            return createdIds.Union(historyIds).ToHashSet();
        }

        private async Task<HashSet<int>> GetRelevantPaymentRequestIds(int userId)
        {
            var createdIds = await _context.PaymentRequests.Where(prq => prq.CreatedByUserId == userId).Select(prq => prq.Id).ToListAsync();
            var historyIds = await _context.FormHistories.Where(fh => fh.WorkFlowTypeId == (short)WorkFlowType.PaymentRequest && (fh.ActionByUserId == userId || fh.ToUserId == userId)).Select(fh => fh.FormId).ToListAsync();
            return createdIds.Union(historyIds).ToHashSet();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}