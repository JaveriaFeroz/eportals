using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Common.Services;
using ProcureToPay.Areas.Procurement.Enums;
using ProcureToPay.Areas.Procurement.Models;
using ProcureToPay.Areas.Reports.Models;
using ProcureToPay.Areas.Reports.Services;
using ProcureToPay.Data;
using System.Security.Claims;

namespace ProcureToPay.Areas.Reports.Controllers
{
    [Area("Reports")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            ApplicationDbContext context,
            IReportService reportService,
            ILogger<ReportsController> logger)
        {
            _context = context;
            _reportService = reportService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private string GetCurrentUserName()
        {
            return User.Identity.Name ?? string.Empty;
        }

        // GET: Reports/Reports (Dashboard/Index)
        // GET: Reports/Reports (Dashboard/Index)
        public async Task<IActionResult> Index()
        {
            var currentMonth = DateTime.Now;
            var firstDayOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Get this month's PRs
            var thisMonthPRs = await _context.PurchaseRequests
                .Where(pr => pr.CreatedOn >= firstDayOfMonth && pr.CreatedOn <= lastDayOfMonth)
                .CountAsync();

            // Get this month's POs
            var thisMonthPOs = await _context.PurchaseOrders
                .Where(po => po.CreatedOn >= firstDayOfMonth && po.CreatedOn <= lastDayOfMonth)
                .CountAsync();

            // Get this month's Payment Requests
            var thisMonthPRQs = await _context.PaymentRequests
                .Where(prq => prq.CreatedOn >= firstDayOfMonth && prq.CreatedOn <= lastDayOfMonth)
                .CountAsync();

            // Get pending approvals (StateId = 2 means Submitted/In Approval)
            var pendingApprovals = await _context.PurchaseRequests
                .Where(pr => pr.StateId == 2)
                .CountAsync() +
                await _context.PurchaseOrders
                .Where(po => po.StateId == 2)
                .CountAsync() +
                await _context.PaymentRequests
                .Where(prq => prq.StateId == 2)
                .CountAsync();

            ViewBag.ThisMonthPRs = thisMonthPRs;
            ViewBag.ThisMonthPOs = thisMonthPOs;
            ViewBag.ThisMonthPRQs = thisMonthPRQs;
            ViewBag.PendingApprovals = pendingApprovals;

            return View();
        }

        // GET: Reports/Reports/PurchaseRequests
        public async Task<IActionResult> PurchaseRequests(PurchaseRequestReportFilter filter)
        {
            await PopulateDropdownsForPRReports();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var reportData = await _reportService.GetPurchaseRequestsReportAsync(filter, GetCurrentUserId());
                return Json(new
                {
                    draw = filter.Draw,
                    recordsTotal = reportData.TotalRecords,
                    recordsFiltered = reportData.FilteredRecords,
                    data = reportData.Data
                });
            }

            return View(filter);
        }

        // POST: Reports/Reports/ExportPurchaseRequestsToExcel
        [HttpPost]
        public async Task<IActionResult> ExportPurchaseRequestsToExcel(PurchaseRequestReportFilter filter)
        {
            try
            {
                filter.PageSize = int.MaxValue; // Get all records for export
                filter.PageIndex = 0;

                var reportData = await _reportService.GetPurchaseRequestsReportAsync(filter, GetCurrentUserId());
                var excelFile = await _reportService.ExportPurchaseRequestsToExcelAsync(reportData.Data);

                var fileName = $"PurchaseRequests_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting Purchase Requests to Excel");
                TempData["Error"] = "Failed to export report. Please try again.";
                return RedirectToAction(nameof(PurchaseRequests));
            }
        }

        // GET: Reports/Reports/PaymentRequests
        public async Task<IActionResult> PaymentRequests(PaymentRequestReportFilter filter)
        {
            await PopulateDropdownsForPaymentReports();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var reportData = await _reportService.GetPaymentRequestsReportAsync(filter, GetCurrentUserId());
                return Json(new
                {
                    draw = filter.Draw,
                    recordsTotal = reportData.TotalRecords,
                    recordsFiltered = reportData.FilteredRecords,
                    data = reportData.Data
                });
            }

            return View(filter);
        }

        // POST: Reports/Reports/ExportPaymentRequestsToExcel
        [HttpPost]
        public async Task<IActionResult> ExportPaymentRequestsToExcel(PaymentRequestReportFilter filter)
        {
            try
            {
                filter.PageSize = int.MaxValue;
                filter.PageIndex = 0;

                var reportData = await _reportService.GetPaymentRequestsReportAsync(filter, GetCurrentUserId());
                var excelFile = await _reportService.ExportPaymentRequestsToExcelAsync(reportData.Data);

                var fileName = $"PaymentRequests_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting Payment Requests to Excel");
                TempData["Error"] = "Failed to export report. Please try again.";
                return RedirectToAction(nameof(PaymentRequests));
            }
        }

        // GET: Reports/Reports/PurchaseOrders
        public async Task<IActionResult> PurchaseOrders(PurchaseOrderReportFilter filter)
        {
            await PopulateDropdownsForPOReports();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var reportData = await _reportService.GetPurchaseOrdersReportAsync(filter, GetCurrentUserId());
                return Json(new
                {
                    draw = filter.Draw,
                    recordsTotal = reportData.TotalRecords,
                    recordsFiltered = reportData.FilteredRecords,
                    data = reportData.Data
                });
            }

            return View(filter);
        }

        // POST: Reports/Reports/ExportPurchaseOrdersToExcel
        [HttpPost]
        public async Task<IActionResult> ExportPurchaseOrdersToExcel(PurchaseOrderReportFilter filter)
        {
            try
            {
                filter.PageSize = int.MaxValue;
                filter.PageIndex = 0;

                var reportData = await _reportService.GetPurchaseOrdersReportAsync(filter, GetCurrentUserId());
                var excelFile = await _reportService.ExportPurchaseOrdersToExcelAsync(reportData.Data);

                var fileName = $"PurchaseOrders_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting Purchase Orders to Excel");
                TempData["Error"] = "Failed to export report. Please try again.";
                return RedirectToAction(nameof(PurchaseOrders));
            }
        }

        // GET: Reports/Reports/POReorders
        [Route("POReorders")]
        public async Task<IActionResult> POReorders(POReorderReportFilter filter)
        {
            await PopulateDropdownsForPOReports();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var reportData = await _reportService.GetPOReordersReportAsync(filter, GetCurrentUserId());
                return Json(new
                {
                    draw = filter.Draw,
                    recordsTotal = reportData.TotalRecords,
                    recordsFiltered = reportData.FilteredRecords,
                    data = reportData.Data
                });
            }

            return View(filter);
        }

        // GET: Reports/Reports/POReorderSummary
        public async Task<IActionResult> POReorderSummary(POReorderReportFilter filter)
        {
            await PopulateDropdownsForPOReports();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var summaryData = await _reportService.GetPOReorderSummaryAsync(filter, GetCurrentUserId());
                return Json(summaryData);
            }

            return View(filter);
        }

        // POST: Reports/Reports/ExportPOReordersToExcel
        [HttpPost]
        public async Task<IActionResult> ExportPOReordersToExcel(POReorderReportFilter filter)
        {
            try
            {
                filter.PageSize = int.MaxValue;
                filter.PageIndex = 0;

                var reportData = await _reportService.GetPOReordersReportAsync(filter, GetCurrentUserId());
                var excelFile = await _reportService.ExportPOReordersToExcelAsync(reportData.Data);

                var fileName = $"POReorders_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting PO Reorders to Excel");
                TempData["Error"] = "Failed to export report. Please try again.";
                return RedirectToAction(nameof(POReorders));
            }
        }


        public async Task<IActionResult> WorkflowAnalysis(WorkflowAnalysisFilter filter)
        {
            await PopulateDropdownsForWorkflowReports();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                try
                {
                    _logger.LogInformation("=== WorkflowAnalysis AJAX Request ===");
                    _logger.LogInformation($"FromDate: {filter.FromDate}");
                    _logger.LogInformation($"ToDate: {filter.ToDate}");
                    _logger.LogInformation($"WorkFlowTypeId: {filter.WorkFlowTypeId}");
                    _logger.LogInformation($"UserId: {filter.UserId}");
                    _logger.LogInformation($"Action: {filter.Action}");
                    _logger.LogInformation($"IncludeCompleted: {filter.IncludeCompleted}");

                    var reportData = await _reportService.GetWorkflowAnalysisReportAsync(filter, GetCurrentUserId());

                    if (reportData == null)
                    {
                        _logger.LogWarning("Report data is null");
                        return Json(new WorkflowAnalysisData
                        {
                            Metrics = new List<WorkflowMetric>(),
                            StateDistribution = new List<StateDistributionItem>(),
                            UserProductivity = new List<UserProductivityItem>(),
                            Timeline = new List<WorkflowTimelineItem>()
                        });
                    }

                    _logger.LogInformation($"Metrics count: {reportData.Metrics?.Count ?? 0}");
                    _logger.LogInformation($"State distribution count: {reportData.StateDistribution?.Count ?? 0}");
                    _logger.LogInformation($"User productivity count: {reportData.UserProductivity?.Count ?? 0}");
                    _logger.LogInformation($"Timeline count: {reportData.Timeline?.Count ?? 0}");

                    return Json(reportData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in WorkflowAnalysis AJAX request");
                    return Json(new
                    {
                        error = true,
                        message = ex.Message,
                        details = ex.InnerException?.Message
                    });
                }
            }

            return View(filter);
        }

        // GET: Reports/Reports/ProcurementSummary
        public async Task<IActionResult> ProcurementSummary(ProcurementSummaryFilter filter)
        {
            await PopulateDropdownsForSummaryReports();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var reportData = await _reportService.GetProcurementSummaryReportAsync(filter, GetCurrentUserId());
                return Json(reportData);
            }

            return View(filter);
        }

        // POST: Reports/Reports/ExportProcurementSummaryToExcel
        [HttpPost]
        public async Task<IActionResult> ExportProcurementSummaryToExcel(ProcurementSummaryFilter filter)
        {
            try
            {
                var reportData = await _reportService.GetProcurementSummaryReportAsync(filter, GetCurrentUserId());
                var excelFile = await _reportService.ExportProcurementSummaryToExcelAsync(reportData);

                var fileName = $"ProcurementSummary_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting Procurement Summary to Excel");
                TempData["Error"] = "Failed to export report. Please try again.";
                return RedirectToAction(nameof(ProcurementSummary));
            }
        }

        private async Task PopulateDropdownsForPRReports()
        {
            ViewBag.Departments = await _context.Departments
                .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName })
                .ToListAsync();

            ViewBag.Branches = await _context.Branches
                .Select(b => new SelectListItem { Value = b.BranchId.ToString(), Text = b.BranchName })
                .ToListAsync();

            ViewBag.Users = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.UserName })
                .ToListAsync();

            ViewBag.ProductNatures = await _context.ProductNatures
              .Where(pn => pn.IsActive)
              .Select(pn => new SelectListItem { Value = pn.NatureId.ToString(), Text = pn.NatureName })
              .ToListAsync();

            ViewBag.Products = await _context.Products
               .Where(p => p.IsActive)
               .Select(p => new SelectListItem { Value = p.ProductId.ToString(), Text = p.ProductName })
               .ToListAsync();

            ViewBag.ServiceNatures = await _context.ServiceNatures
               .Where(sn => sn.IsActive)
               .Select(sn => new SelectListItem { Value = sn.NatureId.ToString(), Text = sn.NatureName })
               .ToListAsync();

            ViewBag.Services = await _context.Services
               .Where(s => s.IsActive)
               .Select(s => new SelectListItem { Value = s.ServiceId.ToString(), Text = s.ServiceName })
               .ToListAsync();

            ViewBag.States = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Draft" },
                new SelectListItem { Value = "2", Text = "Submitted" },
                new SelectListItem { Value = "3", Text = "Approved" },
                new SelectListItem { Value = "4", Text = "Rejected" },
                new SelectListItem { Value = "5", Text = "Returned" },
                new SelectListItem { Value = "10001", Text = "Cancelled" }
            };
        }


        private async Task PopulateDropdownsForPaymentReports()
        {
            ViewBag.Departments = await _context.Departments
                .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName })
                .ToListAsync();

            ViewBag.Branches = await _context.Branches
                .Select(b => new SelectListItem { Value = b.BranchId.ToString(), Text = b.BranchName })
                .ToListAsync();

            ViewBag.Users = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.UserName })
                .ToListAsync();

            ViewBag.PaymentNatures = await _context.PaymentNatures
                .Where(pn => pn.IsActive)
                .Select(pn => new SelectListItem { Value = pn.PaymentNatureId.ToString(), Text = pn.PaymentNatureName })
                .ToListAsync();

            ViewBag.PaymentSubNatures = await _context.SubNatures
                .Where(psn => psn.IsActive)
                .Select(psn => new SelectListItem { Value = psn.SubNatureId.ToString(), Text = psn.SubNatureName })
                .ToListAsync();

            ViewBag.PaymentTypes = await _context.PaymentTypes
                .Where(pt => pt.IsActive)
                .Select(pt => new SelectListItem { Value = pt.PaymentTypeId.ToString(), Text = pt.PaymentTypeName })
                .ToListAsync();

            ViewBag.States = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Saved" },
                new SelectListItem { Value = "2", Text = "Submitted" },
                new SelectListItem { Value = "3", Text = "Approved" },
                new SelectListItem { Value = "4", Text = "Rejected" },
                new SelectListItem { Value = "5", Text = "Returned" },
                new SelectListItem { Value = "10001", Text = "Cancelled" }
            };
        }
        private async Task PopulateDropdownsForPOReports()
        {
            await PopulateDropdownsForPRReports();

            ViewBag.Suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .Select(s => new SelectListItem { Value = s.SupplierId.ToString(), Text = s.SupplierName })
                .ToListAsync();

            ViewBag.ProductNatures = await _context.ProductNatures
               .Where(pn => pn.IsActive)
               .Select(pn => new SelectListItem { Value = pn.NatureId.ToString(), Text = pn.NatureName })
               .ToListAsync();

            ViewBag.Products = await _context.Products
               .Where(p => p.IsActive)
               .Select(p => new SelectListItem { Value = p.ProductId.ToString(), Text = p.ProductName })
               .ToListAsync();

            ViewBag.ServiceNatures = await _context.ServiceNatures
               .Where(sn => sn.IsActive)
               .Select(sn => new SelectListItem { Value = sn.NatureId.ToString(), Text = sn.NatureName })
               .ToListAsync();

            ViewBag.Services = await _context.Services
               .Where(s => s.IsActive)
               .Select(s => new SelectListItem { Value = s.ServiceId.ToString(), Text = s.ServiceName })
               .ToListAsync();
        }

        private async Task PopulateDropdownsForWorkflowReports()
        {
            await PopulateDropdownsForPRReports();

            ViewBag.WorkFlowTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Purchase Request" },
                new SelectListItem { Value = "2", Text = "Purchase Order" },
                 new SelectListItem { Value = "3", Text = "Payment Request" }
            };
        }

        private async Task PopulateDropdownsForSummaryReports()
        {
            await PopulateDropdownsForPOReports();
        }
        [HttpGet]
        // Add this comprehensive diagnostic action to your ReportsController

        [HttpGet]
        public async Task<IActionResult> DiagnoseAllReports()
        {
            try
            {
                var diagnostics = new
                {
                    // Purchase Requests Diagnostics
                    PurchaseRequests = new
                    {
                        Total = await _context.PurchaseRequests.CountAsync(),
                        WithItems = await _context.PurchaseRequests
                            .Include(pr => pr.Items)
                            .Where(pr => pr.Items.Any())
                            .CountAsync(),
                        WithDepartment = await _context.PurchaseRequests
                            .Where(pr => pr.DepartmentId != null)
                            .CountAsync(),
                        WithUser = await _context.PurchaseRequests
                            .Where(pr => pr.RequestedByUserId != null)
                            .CountAsync(),
                        StateDistribution = await _context.PurchaseRequests
                            .GroupBy(pr => pr.StateId)
                            .Select(g => new { StateId = g.Key, Count = g.Count() })
                            .ToListAsync(),
                        Sample = await _context.PurchaseRequests
                            .Include(pr => pr.RequestedByUser)
                            .Include(pr => pr.Department)
                            .Include(pr => pr.Branch)
                            .Include(pr => pr.Items)
                            .Select(pr => new
                            {
                                pr.Id,
                                pr.RequestNumber,
                                pr.Title,
                                pr.RequestDate,
                                pr.StateId,
                                DepartmentId = pr.DepartmentId,
                                DepartmentName = pr.Department != null ? pr.Department.DepartmentName : "NULL",
                                BranchId = pr.BranchId,
                                BranchName = pr.Branch != null ? pr.Branch.BranchName : "NULL",
                                RequestedByUserId = pr.RequestedByUserId,
                                RequestedByUserName = pr.RequestedByUser != null ? pr.RequestedByUser.UserName : "NULL",
                                ItemCount = pr.Items.Count,
                                TotalAmount = pr.Items.Sum(i => i.Quantity * i.UnitPrice),
                                pr.CreatedOn,
                                pr.UpdatedOn
                            })
                            .FirstOrDefaultAsync()
                    },

                    // Purchase Orders Diagnostics
                    PurchaseOrders = new
                    {
                        Total = await _context.PurchaseOrders.CountAsync(),
                        WithItems = await _context.PurchaseOrders
                            .Include(po => po.Items)
                            .Where(po => po.Items.Any())
                            .CountAsync(),
                        WithDepartment = await _context.PurchaseOrders
                            .Where(po => po.DepartmentId != null)
                            .CountAsync(),
                        WithSupplier = await _context.PurchaseOrders
                            .Where(po => po.VendorId != null)
                            .CountAsync(),
                        WithUser = await _context.PurchaseOrders
                            .Where(po => po.CreatedByUserId != null)
                            .CountAsync(),
                        StateDistribution = await _context.PurchaseOrders
                            .GroupBy(po => po.StateId)
                            .Select(g => new { StateId = g.Key, Count = g.Count() })
                            .ToListAsync(),
                        Sample = await _context.PurchaseOrders
                            .Include(po => po.CreatedByUser)
                            .Include(po => po.Department)
                            .Include(po => po.Branch)
                            .Include(po => po.Supplier)
                            .Include(po => po.Items)
                            .Select(po => new
                            {
                                po.Id,
                                po.PONumber,
                                po.Title,
                                po.PODate,
                                po.StateId,
                                DepartmentId = po.DepartmentId,
                                DepartmentName = po.Department != null ? po.Department.DepartmentName : "NULL",
                                BranchId = po.BranchId,
                                BranchName = po.Branch != null ? po.Branch.BranchName : "NULL",
                                VendorId = po.VendorId,
                                SupplierName = po.Supplier != null ? po.Supplier.SupplierName : "NULL",
                                CreatedByUserId = po.CreatedByUserId,
                                CreatedByUserName = po.CreatedByUser != null ? po.CreatedByUser.UserName : "NULL",
                                ItemCount = po.Items.Count,
                                po.TotalAmount,
                                po.TaxAmount,
                                po.GrandTotal,
                                po.CreatedOn,
                                po.UpdatedOn
                            })
                            .FirstOrDefaultAsync()
                    },

                    // Payment Requests Diagnostics
                    PaymentRequests = new
                    {
                        Total = await _context.PaymentRequests.CountAsync(),
                        WithDetails = await _context.PaymentRequests
                            .Include(pr => pr.Details)
                            .Where(pr => pr.Details.Any())
                            .CountAsync(),
                        WithGRN = await _context.PaymentRequests
                            .Where(pr => pr.GoodsReceiptNoteId != null)
                            .CountAsync(),
                        WithoutGRN = await _context.PaymentRequests
                            .Where(pr => pr.GoodsReceiptNoteId == null)
                            .CountAsync(),
                        WithUser = await _context.PaymentRequests
                            .Where(pr => pr.CreatedByUserId != null)
                            .CountAsync(),
                        WithDepartmentCode = await _context.PaymentRequests
                            .Where(pr => !string.IsNullOrEmpty(pr.DepartmentCode))
                            .CountAsync(),
                        WithBranchCode = await _context.PaymentRequests
                            .Where(pr => !string.IsNullOrEmpty(pr.BranchCode))
                            .CountAsync(),
                        StateDistribution = await _context.PaymentRequests
                            .GroupBy(pr => pr.StateId)
                            .Select(g => new { StateId = g.Key, Count = g.Count() })
                            .ToListAsync(),
                        PaymentNatureDistribution = await _context.PaymentRequests
                            .Where(pr => pr.PaymentNatureId != null)
                            .GroupBy(pr => pr.PaymentNatureId)
                            .Select(g => new { PaymentNatureId = g.Key, Count = g.Count() })
                            .ToListAsync(),
                        PaymentTypeDistribution = await _context.PaymentRequests
                            .Where(pr => pr.PaymentTypeId != null)
                            .GroupBy(pr => pr.PaymentTypeId)
                            .Select(g => new { PaymentTypeId = g.Key, Count = g.Count() })
                            .ToListAsync(),
                        Sample = await _context.PaymentRequests
                            .Include(pr => pr.CreatedByUser)
                            .Include(pr => pr.Details)
                            .Include(pr => pr.GoodsReceiptNote)
                            .Select(pr => new
                            {
                                pr.Id,
                                pr.PRQNumber,
                                pr.PayeeName,
                                pr.RequiredDate,
                                pr.StateId,
                                pr.DepartmentCode,
                                pr.BranchCode,
                                pr.PaymentNatureId,
                                pr.PaymentTypeId,
                                pr.PaymentModeId,
                                CreatedByUserId = pr.CreatedByUserId,
                                CreatedByUserName = pr.CreatedByUser != null ? pr.CreatedByUser.UserName : "NULL",
                                DetailsCount = pr.Details.Count,
                                TotalAmount = pr.Details.Sum(d => d.TotalAmount),
                                IsGRNBased = pr.GoodsReceiptNoteId != null,
                                GRNNumber = pr.GoodsReceiptNote != null ? pr.GoodsReceiptNote.GRNNumber : null,
                                pr.PIVNo,
                                pr.CSNo,
                                pr.CreatedOn,
                                pr.UpdatedOn
                            })
                            .FirstOrDefaultAsync()
                    },

                    // Workflow Analysis Diagnostics
                    WorkflowAnalysis = new
                    {
                        TotalFormHistories = await _context.FormHistories.CountAsync(),
                        ByWorkflowType = await _context.FormHistories
                            .GroupBy(fh => fh.WorkFlowTypeId)
                            .Select(g => new { WorkFlowTypeId = g.Key, Count = g.Count() })
                            .ToListAsync(),
                        ByAction = await _context.FormHistories
                            .GroupBy(fh => fh.Action)
                            .Select(g => new { Action = g.Key, Count = g.Count() })
                            .ToListAsync(),
                        ByUser = await _context.FormHistories
                            .GroupBy(fh => fh.ActionByUserId)
                            .Select(g => new { UserId = g.Key, Count = g.Count() })
                            .OrderByDescending(x => x.Count)
                            .Take(10)
                            .ToListAsync(),
                        DateRange = await _context.FormHistories
                            .Select(fh => new { fh.ActionDate })
                            .OrderBy(x => x.ActionDate)
                            .Select(x => x.ActionDate)
                            .ToListAsync(),
                        Sample = await _context.FormHistories
                            .OrderByDescending(fh => fh.ActionDate)
                            .Select(fh => new
                            {

                                fh.WorkFlowTypeId,
                                fh.FormId,
                                fh.Action,
                                fh.ActionByUserId,
                                fh.ActionByUserName,
                                fh.ActionDate,
                                fh.Comments
                            })
                            .Take(5)
                            .ToListAsync()
                    },

                    // Supporting Data
                    SupportingData = new
                    {
                        TotalDepartments = await _context.Departments.CountAsync(),
                        TotalBranches = await _context.Branches.CountAsync(),
                        TotalUsers = await _context.Users.Where(u => u.IsActive).CountAsync(),
                        TotalSuppliers = await _context.Suppliers.Where(s => s.IsActive).CountAsync(),
                        TotalPaymentNatures = await _context.PaymentNatures.Where(pn => pn.IsActive).CountAsync(),
                        TotalPaymentTypes = await _context.PaymentTypes.Where(pt => pt.IsActive).CountAsync(),
                        Departments = await _context.Departments
                            .Select(d => new { d.DepartmentId, d.DepartmentCode, d.DepartmentName })
                            .Take(5)
                            .ToListAsync(),
                        Branches = await _context.Branches
                            .Select(b => new { b.BranchId, b.BranchCode, b.BranchName })
                            .Take(5)
                            .ToListAsync()
                    }
                };

                return Json(diagnostics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in diagnostic endpoint");
                return Json(new
                {
                    Error = ex.Message,
                    InnerException = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetProductNaturesByPurchaseNatureType(PurchaseNatureType type)
        {
            IQueryable<Master.Models.ProductNature> query = _context.ProductNatures.Where(pn => pn.IsActive);

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

        [HttpGet]
        public async Task<JsonResult> GetServiceNaturesByPurchaseNatureType(PurchaseNatureType type)
        {
            IQueryable<Master.Models.ServiceNature> query = _context.ServiceNatures.Where(sn => sn.IsActive);

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

    }
}