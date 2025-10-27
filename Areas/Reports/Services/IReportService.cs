using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ProcureToPay.Areas.Common.Services;
using ProcureToPay.Areas.Finance.Models;
using ProcureToPay.Areas.Procurement.Models;
using ProcureToPay.Areas.Reports.Models;
using ProcureToPay.Data;
using System.Drawing;

namespace ProcureToPay.Areas.Reports.Services
{
    public interface IReportService
    {
        Task<ReportResponse<PurchaseRequestReportItem>> GetPurchaseRequestsReportAsync(PurchaseRequestReportFilter filter, int currentUserId);
        Task<ReportResponse<PurchaseOrderReportItem>> GetPurchaseOrdersReportAsync(PurchaseOrderReportFilter filter, int currentUserId);
        Task<ReportResponse<PaymentRequestReportData>> GetPaymentRequestsReportAsync(PaymentRequestReportFilter filter, int currentUserId);
        Task<WorkflowAnalysisData> GetWorkflowAnalysisReportAsync(WorkflowAnalysisFilter filter, int currentUserId);
        Task<ProcurementSummaryData> GetProcurementSummaryReportAsync(ProcurementSummaryFilter filter, int currentUserId);
        Task<ReportResponse<POReorderReportItem>> GetPOReordersReportAsync(POReorderReportFilter filter, int currentUserId);
        Task<POReorderSummaryData> GetPOReorderSummaryAsync(POReorderReportFilter filter, int currentUserId);

        // Excel Export Methods
        Task<byte[]> ExportPurchaseRequestsToExcelAsync(List<PurchaseRequestReportItem> data);
        Task<byte[]> ExportPurchaseOrdersToExcelAsync(List<PurchaseOrderReportItem> data);
        Task<byte[]> ExportPaymentRequestsToExcelAsync(List<PaymentRequestReportData> data);
        Task<byte[]> ExportProcurementSummaryToExcelAsync(ProcurementSummaryData data);
        Task<byte[]> ExportPOReordersToExcelAsync(List<POReorderReportItem> data);

    }

    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportService> _logger;

        public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Purchase Request Reports
        public async Task<ReportResponse<PurchaseRequestReportItem>> GetPurchaseRequestsReportAsync(PurchaseRequestReportFilter filter, int currentUserId)
        {
            try
            {
                _logger.LogInformation("Starting Purchase Request Report generation");

                var query = _context.PurchaseRequests
                    .Include(pr => pr.RequestedByUser)
                    .Include(pr => pr.Department)
                    .Include(pr => pr.Branch)
                    .Include(pr => pr.Items)
                    .AsQueryable();

                _logger.LogInformation($"Base query created. Total PRs in database: {await _context.PurchaseRequests.CountAsync()}");

                var totalRecords = await query.CountAsync();
                _logger.LogInformation($"Total records before filtering: {totalRecords}");

                // Apply all filters
                if (filter.OnlyMyRequests)
                {
                    var currentUserName = await _context.Users
                        .Where(u => u.Id == currentUserId)
                        .Select(u => u.UserName)
                        .FirstOrDefaultAsync() ?? "";

                    _logger.LogInformation($"Filtering for current user: {currentUserId}, Username: {currentUserName}");
                    query = query.Where(pr => pr.RequestedByUserId == currentUserId || pr.Owner == currentUserName);
                }

                if (filter.FromDate.HasValue)
                {
                    _logger.LogInformation($"Filtering FromDate: {filter.FromDate.Value}");
                    query = query.Where(pr => pr.RequestDate >= filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    _logger.LogInformation($"Filtering ToDate: {filter.ToDate.Value}");
                    query = query.Where(pr => pr.RequestDate <= filter.ToDate.Value);
                }

                if (filter.DepartmentId.HasValue)
                {
                    _logger.LogInformation($"Filtering DepartmentId: {filter.DepartmentId.Value}");
                    query = query.Where(pr => pr.DepartmentId == filter.DepartmentId.Value);
                }

                if (filter.BranchId.HasValue)
                {
                    _logger.LogInformation($"Filtering BranchId: {filter.BranchId.Value}");
                    query = query.Where(pr => pr.BranchId == filter.BranchId.Value);
                }

                if (filter.RequestedByUserId.HasValue)
                {
                    _logger.LogInformation($"Filtering RequestedByUserId: {filter.RequestedByUserId.Value}");
                    query = query.Where(pr => pr.RequestedByUserId == filter.RequestedByUserId.Value);
                }

                if (filter.StateId.HasValue)
                {
                    _logger.LogInformation($"Filtering StateId: {filter.StateId.Value}");
                    query = query.Where(pr => pr.StateId == filter.StateId.Value);
                }

                if (!string.IsNullOrEmpty(filter.RequestNumber))
                {
                    _logger.LogInformation($"Filtering RequestNumber: {filter.RequestNumber}");
                    query = query.Where(pr => pr.RequestNumber != null && pr.RequestNumber.Contains(filter.RequestNumber));
                }

                // --- Naye filter properties ko query mein apply karein ---
                if (filter.ProductId.HasValue)
                {
                    query = query.Where(pr => pr.Items.Any(item => item.ProductId == filter.ProductId.Value));
                }

                if (filter.ServiceId.HasValue)
                {
                    query = query.Where(pr => pr.Items.Any(item => item.ServiceId == filter.ServiceId.Value));
                }

                // --- End of new filters ---

                // Apply search
                if (!string.IsNullOrEmpty(filter.SearchValue))
                {
                    _logger.LogInformation($"Applying search: {filter.SearchValue}");
                    query = query.Where(pr =>
                        (pr.RequestNumber != null && pr.RequestNumber.Contains(filter.SearchValue)) ||
                        (pr.Title != null && pr.Title.Contains(filter.SearchValue)) ||
                        (pr.Description != null && pr.Description.Contains(filter.SearchValue)) ||
                        (pr.RequestedByUser != null && pr.RequestedByUser.UserName.Contains(filter.SearchValue)) ||
                        // New condition to search for items by name
                        pr.Items.Any(item => (item.ItemName != null && item.ItemName.Contains(filter.SearchValue)))
                    );
                }

                // Get filtered count BEFORE amount filtering
                var filteredRecords = await query.CountAsync();
                _logger.LogInformation($"Records after filtering (before amount filter): {filteredRecords}");

                // Amount filters - apply separately to avoid EF Core translation issues
                if (filter.MinAmount.HasValue || filter.MaxAmount.HasValue)
                {
                    _logger.LogInformation($"Filtering by amount - Min: {filter.MinAmount}, Max: {filter.MaxAmount}");

                    var matchingIds = await query
                        .Select(pr => new {
                            pr.Id,
                            TotalAmount = pr.Items.Sum(i => i.Quantity * i.UnitPrice)
                        })
                        .Where(x =>
                            (!filter.MinAmount.HasValue || x.TotalAmount >= filter.MinAmount.Value) &&
                            (!filter.MaxAmount.HasValue || x.TotalAmount <= filter.MaxAmount.Value))
                        .Select(x => x.Id)
                        .ToListAsync();

                    _logger.LogInformation($"Records matching amount filter: {matchingIds.Count}");

                    query = query.Where(pr => matchingIds.Contains(pr.Id));
                    filteredRecords = matchingIds.Count;
                }

                _logger.LogInformation($"Final filtered count: {filteredRecords}");

                var sortedQuery = ApplySortingToPurchaseRequests(query, filter.SortColumnIndex, filter.SortDirection);

                var pagedData = await sortedQuery
                    .Skip(filter.PageIndex * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {pagedData.Count} records for page {filter.PageIndex}");

                // Get item names for the report
                var prItemNames = pagedData.ToDictionary(
                    pr => pr.Id,
                    pr => string.Join(", ", pr.Items.Select(i => i.ItemName))
                );

                var reportItems = pagedData.Select(pr => new PurchaseRequestReportItem
                {
                    Id = pr.Id,
                    RequestNumber = pr.RequestNumber ?? "",
                    Title = pr.Title ?? "",
                    Description = pr.Description ?? "",
                    RequestDate = pr.RequestDate,
                    RequiredDate = pr.RequiredDate,
                    RequestedByUserName = pr.RequestedByUser?.UserName ?? "",
                    DepartmentName = pr.Department?.DepartmentName ?? "",
                    BranchName = pr.Branch?.BranchName ?? "",
                    StatusName = GetStatusName(pr.StateId),
                    PurchaseNature = pr.PurchaseNatureType.ToString(),
                    PurchaseType = pr.PurchaseItemType.ToString(),
                    TotalAmount = pr.Items?.Sum(i => i.Quantity * i.UnitPrice) ?? 0,
                    Owner = pr.Owner ?? "",
                    CreatedOn = pr.CreatedOn,
                    DaysInCurrentState = (DateTime.Now - (pr.UpdatedOn ?? pr.CreatedOn)).Days,
                    HasPurchaseOrders = _context.PurchaseRequestOrderMappings.Any(m => m.PurchaseRequestId == pr.Id),

                    // Item display logic
                    ItemDisplay = (filter.ProductNatureId.HasValue || filter.ProductId.HasValue || filter.ServiceNatureId.HasValue || filter.ServiceId.HasValue)
                        ? prItemNames.ContainsKey(pr.Id) ? prItemNames[pr.Id] : ""
                        : pr.Items?.Count.ToString() ?? "0"
                }).ToList();

                _logger.LogInformation($"Successfully generated report with {reportItems.Count} items");

                return new ReportResponse<PurchaseRequestReportItem>
                {
                    Data = reportItems,
                    TotalRecords = totalRecords,
                    FilteredRecords = filteredRecords,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Purchase Request report");
                return new ReportResponse<PurchaseRequestReportItem>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Data = new List<PurchaseRequestReportItem>(),
                    TotalRecords = 0,
                    FilteredRecords = 0
                };
            }
        }
        #endregion

        #region Purchase Order Reports

        public async Task<ReportResponse<PurchaseOrderReportItem>> GetPurchaseOrdersReportAsync(PurchaseOrderReportFilter filter, int currentUserId)
        {
            try
            {
                _logger.LogInformation("=== Starting Purchase Order Report ===");

                var query = _context.PurchaseOrders
                    .Include(po => po.CreatedByUser)
                    .Include(po => po.Department)
                    .Include(po => po.Branch)
                    .Include(po => po.Supplier)
                    .Include(po => po.Items)
                        // SourcePurchaseRequestItem ko include karna zaroori hai
                        // agar aap ProductId/ServiceId par filter kar rahe hain.
                        .ThenInclude(poItem => poItem.SourcePurchaseRequestItem)
                    .AsQueryable();

                var totalRecords = await _context.PurchaseOrders.CountAsync();
                _logger.LogInformation($"Total POs in database: {totalRecords}");

                // =========================================================
                // 1. FILTERS (Saare filters lagana)
                // =========================================================

                if (filter.OnlyMyOrders)
                {
                    var currentUserName = await _context.Users
                        .Where(u => u.Id == currentUserId)
                        .Select(u => u.UserName)
                        .FirstOrDefaultAsync() ?? "";

                    query = query.Where(po => po.CreatedByUserId == currentUserId || po.Owner == currentUserName);
                }

                // Date and Simple Filters
                if (filter.FromDate.HasValue)
                    query = query.Where(po => po.PODate >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    // Ensure ToDate includes the entire day
                    query = query.Where(po => po.PODate <= filter.ToDate.Value.AddDays(1).AddSeconds(-1));

                if (filter.DepartmentId.HasValue)
                    query = query.Where(po => po.DepartmentId == filter.DepartmentId.Value);

                if (filter.BranchId.HasValue)
                    query = query.Where(po => po.BranchId == filter.BranchId.Value);

                if (filter.SupplierId.HasValue)
                    query = query.Where(po => po.VendorId == filter.SupplierId.Value);

                if (filter.CreatedByUserId.HasValue)
                    query = query.Where(po => po.CreatedByUserId == filter.CreatedByUserId.Value);

                if (filter.StateId.HasValue)
                    query = query.Where(po => po.StateId == filter.StateId.Value);

                // Amount Filters (Optional: yeh MinAmount/MaxAmount inputs aapke HTML mein commented out the)
                if (filter.MinAmount.HasValue)
                    query = query.Where(po => po.GrandTotal >= filter.MinAmount.Value);

                if (filter.MaxAmount.HasValue)
                    query = query.Where(po => po.GrandTotal <= filter.MaxAmount.Value);

                if (filter.ExpectedDeliveryFrom.HasValue)
                    query = query.Where(po => po.ExpectedDeliveryDate >= filter.ExpectedDeliveryFrom.Value);

                if (filter.ExpectedDeliveryTo.HasValue)
                    query = query.Where(po => po.ExpectedDeliveryDate <= filter.ExpectedDeliveryTo.Value);

                // PONumber Filter
                if (!string.IsNullOrEmpty(filter.PONumber))
                    query = query.Where(po => po.PONumber != null && po.PONumber.Contains(filter.PONumber));


                // --- Product and Service Cascading Filters (FIXED: ProductNatureId/ServiceNatureId ko bhi use kiya gaya) ---

                if (filter.ProductId.HasValue)
                {
                    // Note: ProductID filter ProductNatureId ke baghair bhi kaam karega, jaisa ke JS mein tha
                    query = query.Where(po => po.Items.Any(poItem =>
                        poItem.SourcePurchaseRequestItem != null &&
                        poItem.SourcePurchaseRequestItem.ProductId == filter.ProductId.Value
                    ));
                }

                if (filter.ServiceId.HasValue)
                {
                    query = query.Where(po => po.Items.Any(poItem =>
                        poItem.SourcePurchaseRequestItem != null &&
                        poItem.SourcePurchaseRequestItem.ServiceId == filter.ServiceId.Value
                    ));
                }

                // --- End Product and Service Cascading Filters ---

                // --- Search Logic ---
                if (!string.IsNullOrEmpty(filter.SearchValue))
                {
                    query = query.Where(po =>
                        (po.PONumber != null && po.PONumber.Contains(filter.SearchValue)) ||
                        (po.Title != null && po.Title.Contains(filter.SearchValue)) ||
                        (po.Description != null && po.Description.Contains(filter.SearchValue)) ||
                        (po.Supplier != null && po.Supplier.SupplierName.Contains(filter.SearchValue)) ||
                        po.Items.Any(item => (item.ItemName != null && item.ItemName.Contains(filter.SearchValue)))
                    );
                }
                // --- End Search Logic ---

                // =========================================================
                // 2. COUNT AND PAGING (Sorting aur Skip/Take)
                // =========================================================

                // Final filtered records count
                var filteredRecords = await query.CountAsync();
                _logger.LogInformation($"Records after filtering: {filteredRecords}");

                // Apply sorting
                var sortedQuery = ApplySortingToPurchaseOrders(query, filter.SortColumnIndex, filter.SortDirection);

                // Apply paging
                var pagedData = await sortedQuery
                    .Skip(filter.PageIndex * filter.PageSize) // For Page 2: 1 * 25 = 25
                    .Take(filter.PageSize)                   // Take next 25
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {pagedData.Count} records");

                // =========================================================
                // 3. MAPPING (Data ko Report Item mein convert karna)
                // =========================================================

                // Get related PR numbers efficiently
                var poIds = pagedData.Select(po => po.Id).ToList();
                var relatedPRs = await _context.PurchaseRequestOrderMappings
                    .Where(m => poIds.Contains(m.PurchaseOrderId))
                    .Include(m => m.PurchaseRequest)
                    .GroupBy(m => m.PurchaseOrderId)
                    .Select(g => new { POId = g.Key, PRNumbers = string.Join(", ", g.Select(m => m.PurchaseRequest.RequestNumber)) })
                    .ToDictionaryAsync(x => x.POId, x => x.PRNumbers);

                // Report Items ki final mapping
                var reportItems = pagedData.Select(po => new PurchaseOrderReportItem
                {
                    Id = po.Id,
                    PONumber = po.PONumber ?? "",
                    Title = po.Title ?? "",
                    Description = po.Description ?? "",
                    PODate = po.PODate,
                    ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                    SupplierName = po.Supplier?.SupplierName ?? "",
                    CreatedByUserName = po.CreatedByUser?.UserName ?? "",
                    DepartmentName = po.Department?.DepartmentName ?? "",
                    BranchName = po.Branch?.BranchName ?? "",
                    StatusName = GetStatusName(po.StateId),
                    TotalAmount = po.TotalAmount,
                    TaxAmount = po.TaxAmount,
                    GrandTotal = po.GrandTotal,
                    Owner = po.Owner ?? "",
                    CreatedOn = po.CreatedOn,
                    DaysInCurrentState = (DateTime.Now - (po.UpdatedOn ?? po.CreatedOn)).Days,
                    RelatedPRNumbers = relatedPRs.ContainsKey(po.Id) ? relatedPRs[po.Id] : "",

                    // Item display logic
                    ItemDisplay = po.Items?.Count.ToString() ?? "0" // Default is item count

                }).ToList();

                // ItemDisplay ko search/filter results ke mutabiq detail mein show karna
                // Agar koi item filter active hai, toh item names dikhao.
                if (filter.ProductId.HasValue || filter.ServiceId.HasValue)
                {
                    foreach (var item in reportItems)
                    {
                        var poData = pagedData.First(p => p.Id == item.Id);
                        item.ItemDisplay = string.Join(", ", poData.Items.Select(i => i.ItemName));
                    }
                }

                _logger.LogInformation($"=== PO Report Complete: {reportItems.Count} items ===");

                // Final Response
                return new ReportResponse<PurchaseOrderReportItem>
                {
                    Data = reportItems,
                    TotalRecords = totalRecords,
                    FilteredRecords = filteredRecords,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPurchaseOrdersReportAsync");
                return new ReportResponse<PurchaseOrderReportItem>
                {
                    Success = false,
                    ErrorMessage = "Server Error: " + ex.Message, // Clear error message bheja gaya hai
                    Data = new List<PurchaseOrderReportItem>(),
                    TotalRecords = 0,
                    FilteredRecords = 0
                };
            }
        }
        #endregion

        #region Payment Request Reports

        public async Task<ReportResponse<PaymentRequestReportData>> GetPaymentRequestsReportAsync(PaymentRequestReportFilter filter, int currentUserId)
        {
            try
            {
                _logger.LogInformation("=== Starting Payment Request Report ===");

                var query = _context.PaymentRequests
                .Include(pr => pr.CreatedByUser)
                .Include(pr => pr.Details)
                .Include(pr => pr.GoodsReceiptNote)
                .AsQueryable();

                var totalRecords = await _context.PaymentRequests.CountAsync();
                _logger.LogInformation($"Total Payment Requests in database: {totalRecords}");

                // Apply all filters (OnlyMyRequests, Dates, Dept, Branch, etc.) ...
                // ... (Aapka saara filtering code yahan) ...

                // Apply filters
                if (filter.OnlyMyRequests)
                {
                    var currentUserName = await _context.Users
                       .Where(u => u.Id == currentUserId)
                       .Select(u => u.UserName)
                       .FirstOrDefaultAsync() ?? "";

                    query = query.Where(pr => pr.CreatedByUserId == currentUserId || pr.Owner == currentUserName);
                }

                if (filter.FromDate.HasValue)
                    query = query.Where(pr => pr.CreatedOn >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(pr => pr.CreatedOn <= filter.ToDate.Value);

                if (filter.DepartmentId.HasValue)
                {
                    query = query.Where(pr => pr.DepartmentId == filter.DepartmentId.Value);
                }

                if (filter.BranchId.HasValue)
                {
                    query = query.Where(pr => pr.BranchId == filter.BranchId.Value);
                }

                if (filter.RequestedByUserId.HasValue)
                    query = query.Where(pr => pr.CreatedByUserId == filter.RequestedByUserId.Value);

                if (filter.StateId.HasValue)
                    query = query.Where(pr => pr.StateId == filter.StateId.Value);

                if (filter.PaymentNatureId.HasValue)
                    query = query.Where(pr => pr.PaymentNatureId == filter.PaymentNatureId.Value);

                if (filter.PaymentSubNatureId.HasValue)
                    query = query.Where(pr => pr.PaymentSubNatureId == filter.PaymentSubNatureId.Value);

                if (filter.PaymentTypeId.HasValue)
                    query = query.Where(pr => pr.PaymentTypeId == filter.PaymentTypeId.Value);

                if (!string.IsNullOrEmpty(filter.PRQNumber))
                    query = query.Where(pr => pr.PRQNumber != null && pr.PRQNumber.Contains(filter.PRQNumber));

                if (filter.IsGRNBased.HasValue)
                    query = query.Where(pr => filter.IsGRNBased.Value ? pr.GoodsReceiptNoteId != null : pr.GoodsReceiptNoteId == null);

                if (!string.IsNullOrEmpty(filter.InvoiceNumber))
                {
                    query = query.Where(pr => pr.Details.Any(d => d.InvoiceNo != null && d.InvoiceNo.Contains(filter.InvoiceNumber)));
                }

                if (!string.IsNullOrEmpty(filter.SearchValue))
                {
                    query = query.Where(pr =>
                        (pr.PRQNumber != null && pr.PRQNumber.Contains(filter.SearchValue)) ||
                        (pr.PayeeName != null && pr.PayeeName.Contains(filter.SearchValue)) ||
                        (pr.CreatedByUser != null && pr.CreatedByUser.UserName.Contains(filter.SearchValue)) ||
                        (pr.GoodsReceiptNote != null && pr.GoodsReceiptNote.GRNNumber.Contains(filter.SearchValue)) ||
                        pr.Details.Any(d => d.InvoiceNo != null && d.InvoiceNo.Contains(filter.SearchValue))
                    );
                }

                var filteredRecords = await query.CountAsync();
                _logger.LogInformation($"Records after basic filtering: {filteredRecords}");

                // Amount filtering
                if (filter.MinAmount.HasValue || filter.MaxAmount.HasValue)
                {
                    // Note: Agar yahan aap dobara database call kar rahe hain (ToListAsync), toh yeh query ko execute kar deta hai.
                    // Isliye yeh logic theek hai, lekin iske baad 'query' ko updated list of IDs par filter karna zaruri hai.
                    var matchingIds = await query
                        .Select(pr => new { pr.Id, TotalAmount = pr.Details.Sum(d => d.TotalAmount) })
                        .Where(x =>
                            (!filter.MinAmount.HasValue || x.TotalAmount >= filter.MinAmount.Value) &&
                            (!filter.MaxAmount.HasValue || x.TotalAmount <= filter.MaxAmount.Value))
                        .Select(x => x.Id)
                        .ToListAsync();

                    query = query.Where(pr => matchingIds.Contains(pr.Id));
                    filteredRecords = matchingIds.Count;
                    _logger.LogInformation($"Records after amount filtering: {filteredRecords}");
                }

                // =======================================================================================
                // FIX: Ab sortedQuery ko update kiya jaega TAAKE woh amount filter ko shamil kare
                // =======================================================================================
                var sortedQuery = ApplySortingToPaymentRequests(query, filter.SortColumnIndex, filter.SortDirection);

                // Apply paging
                var pagedData = await sortedQuery
                .Skip(filter.PageIndex * filter.PageSize) // For Page 2: 1 * 10 = 10 (Sahi!)
                .Take(filter.PageSize) // 10 records uthao
                .ToListAsync();

                // ... (Baqi lookup aur mapping code theek hai) ...

                _logger.LogInformation($"Retrieved {pagedData.Count} records");

                var departmentIds = pagedData.Where(pr => pr.DepartmentId.HasValue).Select(pr => pr.DepartmentId.Value).Distinct().ToList();
                var departments = await _context.Departments
                     .Where(d => departmentIds.Contains(d.DepartmentId))
                     .ToDictionaryAsync(d => d.DepartmentId, d => d.DepartmentName);

                // Branch Lookup (using ID as key)
                var branchIds = pagedData.Where(pr => pr.BranchId.HasValue).Select(pr => pr.BranchId.Value).Distinct().ToList();
                var branches = await _context.Branches
                     .Where(b => branchIds.Contains(b.BranchId))
                     .ToDictionaryAsync(b => b.BranchId, b => b.BranchName);

                // ... (paymentNatures, paymentSubNatures, suppliers, paymentTypes lookup code) ...
                var paymentNatures = await _context.PaymentNatures
                   .Where(pn => pn.IsActive)
                   .GroupBy(pn => pn.PaymentNatureId)
                   .Select(g => g.First())
                   .ToDictionaryAsync(pn => pn.PaymentNatureId, pn => pn.PaymentNatureName);

                var paymentSubNatures = await _context.SubNatures
                   .Where(psn => psn.IsActive)
                   .GroupBy(psn => psn.SubNatureId)
                   .Select(g => g.First())
                   .ToDictionaryAsync(psn => psn.SubNatureId, psn => psn.SubNatureName);

                var paymentTypes = await _context.PaymentTypes
                   .Where(pt => pt.IsActive)
                   .GroupBy(pt => pt.PaymentTypeId)
                   .Select(g => g.First())
                   .ToDictionaryAsync(pt => pt.PaymentTypeId, pt => pt.PaymentTypeName);


                var reportItems = pagedData.Select(pr => new PaymentRequestReportData
                {
                    Id = pr.Id,
                    PRQNumber = pr.PRQNumber ?? "",
                    PayeeName = pr.PayeeName ?? "",
                    RequiredDate = pr.RequiredDate,
                    RequestedByUserName = pr.CreatedByUser?.UserName ?? "",
                    DepartmentName = pr.DepartmentId.HasValue && departments.ContainsKey(pr.DepartmentId.Value)
                     ? departments[pr.DepartmentId.Value] : pr.DepartmentCode ?? "",
                             BranchName = pr.BranchId.HasValue && branches.ContainsKey(pr.BranchId.Value)
                     ? branches[pr.BranchId.Value] : pr.BranchCode ?? "",
                    PaymentNature = pr.PaymentNatureId.HasValue && paymentNatures.ContainsKey(pr.PaymentNatureId.Value)
                    ? paymentNatures[pr.PaymentNatureId.Value] : "",
                    PaymentSubNature = pr.PaymentSubNatureId.HasValue && paymentSubNatures.ContainsKey(pr.PaymentSubNatureId.Value)
                    ? paymentSubNatures[pr.PaymentSubNatureId.Value] : "",
                    PaymentType = pr.PaymentTypeId.HasValue && paymentTypes.ContainsKey(pr.PaymentTypeId.Value)
                    ? paymentTypes[pr.PaymentTypeId.Value] : "",
                    PaymentMode = pr.PaymentModeId?.ToString() ?? "",
                    StatusName = GetStatusName(pr.StateId),
                    TotalAmount = pr.Details?.Sum(d => d.TotalAmount) ?? 0,
                    IsGRNBased = pr.GoodsReceiptNoteId.HasValue,
                    GRNNumber = pr.GoodsReceiptNote?.GRNNumber,
                    PIVNo = pr.PIVNo,
                    CSNo = pr.CSNo,
                    CreatedOn = pr.CreatedOn,
                    InvoiceDisplay = !string.IsNullOrEmpty(filter.InvoiceNumber)
                    ? string.Join(", ", pr.Details.Select(d => d.InvoiceNo).Where(i => !string.IsNullOrEmpty(i)))
                    : (pr.Details?.Count(d => !string.IsNullOrEmpty(d.InvoiceNo)) > 1
                       ? pr.Details.Count(d => !string.IsNullOrEmpty(d.InvoiceNo)).ToString()
                       : pr.Details.FirstOrDefault(d => !string.IsNullOrEmpty(d.InvoiceNo))?.InvoiceNo ?? "N/A")
                }).ToList();


                _logger.LogInformation($"=== Payment Request Report Complete: {reportItems.Count} items ===");

                return new ReportResponse<PaymentRequestReportData>
                {
                    Data = reportItems,
                    TotalRecords = totalRecords,
                    FilteredRecords = filteredRecords,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaymentRequestsReportAsync");
                return new ReportResponse<PaymentRequestReportData>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Data = new List<PaymentRequestReportData>(),
                    TotalRecords = 0,
                    FilteredRecords = 0
                };
            }
        }
        public async Task<byte[]> ExportPaymentRequestsToExcelAsync(List<PaymentRequestReportData> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Payment Requests Report");

            // Add title
            worksheet.Cell(1, 1).Value = "Payment Requests Report";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, 13).Merge();

            // Add generation date
            worksheet.Cell(2, 1).Value = $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            worksheet.Range(2, 1, 2, 13).Merge();

            // Add headers
            var headers = new[]
            {
        "PRQ Number",
        "Payee Name",
        "Required Date",
        "Created By",
        "Department",
        "Branch",
        "Payment Nature",
        "Payment Type",
        "Payment Mode",
        "Status",
        "Total Amount",
        "Days in State",
        "GRN Based",
        "GRN Number",
        "PIV No",
        "CS No",
        "Created On"
    };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(4, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#092963");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Add data
            int row = 5;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.PRQNumber;
                worksheet.Cell(row, 2).Value = item.PayeeName;
                worksheet.Cell(row, 3).Value = item.RequiredDate.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 4).Value = item.RequestedByUserName;
                worksheet.Cell(row, 5).Value = item.DepartmentName;
                worksheet.Cell(row, 6).Value = item.BranchName;
                worksheet.Cell(row, 7).Value = item.PaymentNature;
                worksheet.Cell(row, 8).Value = item.PaymentType;
                worksheet.Cell(row, 9).Value = item.PaymentMode;
                worksheet.Cell(row, 10).Value = item.StatusName;

                var amountCell = worksheet.Cell(row, 11);
                amountCell.Value = item.TotalAmount;
                amountCell.Style.NumberFormat.Format = "#,##0.00";

                worksheet.Cell(row, 12).Value = item.DaysInCurrentState;
                worksheet.Cell(row, 13).Value = item.IsGRNBased ? "Yes" : "No";
                worksheet.Cell(row, 14).Value = item.GRNNumber ?? "";
                worksheet.Cell(row, 15).Value = item.PIVNo ?? "";
                worksheet.Cell(row, 16).Value = item.CSNo ?? "";
                worksheet.Cell(row, 17).Value = item.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss");

                // Apply borders
                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                row++;
            }

            // Add summary
            row += 2;
            worksheet.Cell(row, 1).Value = "Summary";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Font.FontSize = 14;

            row++;
            worksheet.Cell(row, 1).Value = "Total Records:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = data.Count;

            row++;
            worksheet.Cell(row, 1).Value = "Total Amount:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            var totalAmountCell = worksheet.Cell(row, 2);
            totalAmountCell.Value = data.Sum(x => x.TotalAmount);
            totalAmountCell.Style.NumberFormat.Format = "#,##0.00";

            row++;
            worksheet.Cell(row, 1).Value = "Average Amount:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            var avgAmountCell = worksheet.Cell(row, 2);
            avgAmountCell.Value = data.Any() ? data.Average(x => x.TotalAmount) : 0;
            avgAmountCell.Style.NumberFormat.Format = "#,##0.00";

            row++;
            worksheet.Cell(row, 1).Value = "GRN Based Requests:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = data.Count(x => x.IsGRNBased);

            row++;
            worksheet.Cell(row, 1).Value = "Non-GRN Requests:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = data.Count(x => !x.IsGRNBased);

            // Status distribution
            row += 2;
            worksheet.Cell(row, 1).Value = "Status Distribution";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Font.FontSize = 12;

            row++;
            var statusGroups = data.GroupBy(x => x.StatusName)
                .Select(g => new { Status = g.Key, Count = g.Count(), Amount = g.Sum(x => x.TotalAmount) })
                .OrderByDescending(x => x.Count);

            worksheet.Cell(row, 1).Value = "Status";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = "Count";
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            worksheet.Cell(row, 3).Value = "Total Amount";
            worksheet.Cell(row, 3).Style.Font.Bold = true;

            row++;
            foreach (var group in statusGroups)
            {
                worksheet.Cell(row, 1).Value = group.Status;
                worksheet.Cell(row, 2).Value = group.Count;
                var statusAmountCell = worksheet.Cell(row, 3);
                statusAmountCell.Value = group.Amount;
                statusAmountCell.Style.NumberFormat.Format = "#,##0.00";
                row++;
            }

            // Payment Nature distribution
            row += 2;
            worksheet.Cell(row, 1).Value = "Payment Nature Distribution";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Font.FontSize = 12;

            row++;
            var natureGroups = data.GroupBy(x => x.PaymentNature)
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .Select(g => new { Nature = g.Key, Count = g.Count(), Amount = g.Sum(x => x.TotalAmount) })
                .OrderByDescending(x => x.Count);

            worksheet.Cell(row, 1).Value = "Payment Nature";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = "Count";
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            worksheet.Cell(row, 3).Value = "Total Amount";
            worksheet.Cell(row, 3).Style.Font.Bold = true;

            row++;
            foreach (var group in natureGroups)
            {
                worksheet.Cell(row, 1).Value = group.Nature;
                worksheet.Cell(row, 2).Value = group.Count;
                var natureAmountCell = worksheet.Cell(row, 3);
                natureAmountCell.Value = group.Amount;
                natureAmountCell.Style.NumberFormat.Format = "#,##0.00";
                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Save to memory stream
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        #endregion

        #region Workflow Analysis Reports

        public async Task<WorkflowAnalysisData> GetWorkflowAnalysisReportAsync(WorkflowAnalysisFilter filter, int currentUserId)
        {
            var result = new WorkflowAnalysisData
            {
                Metrics = new List<WorkflowMetric>(),
                StateDistribution = new List<StateDistributionItem>(),
                UserProductivity = new List<UserProductivityItem>(),
                Timeline = new List<WorkflowTimelineItem>()
            };

            try
            {
                _logger.LogInformation("=== Starting Workflow Analysis Report ===");

                // Using FormHistories as defined in DbContext
                var query = _context.FormHistories.AsQueryable();

                var totalFormHistories = await _context.FormHistories.CountAsync();
                _logger.LogInformation($"Total Form History records in database: {totalFormHistories}");

                if (totalFormHistories == 0)
                {
                    _logger.LogWarning("No form history records found in database");
                    return result;
                }

                // Apply filters
                if (filter.FromDate.HasValue)
                {
                    _logger.LogInformation($"Filtering FromDate: {filter.FromDate.Value}");
                    query = query.Where(fh => fh.ActionDate >= filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    _logger.LogInformation($"Filtering ToDate: {filter.ToDate.Value}");
                    query = query.Where(fh => fh.ActionDate <= filter.ToDate.Value);
                }

                if (filter.WorkFlowTypeId.HasValue)
                {
                    _logger.LogInformation($"Filtering WorkFlowTypeId: {filter.WorkFlowTypeId.Value}");
                    query = query.Where(fh => fh.WorkFlowTypeId == filter.WorkFlowTypeId.Value);
                }

                if (filter.UserId.HasValue)
                {
                    _logger.LogInformation($"Filtering UserId: {filter.UserId.Value}");
                    query = query.Where(fh => fh.ActionByUserId == filter.UserId.Value);
                }

                if (!string.IsNullOrEmpty(filter.Action))
                {
                    _logger.LogInformation($"Filtering Action: {filter.Action}");
                    query = query.Where(fh => fh.Action.Contains(filter.Action));
                }

                var filteredCount = await query.CountAsync();
                _logger.LogInformation($"Records after filtering: {filteredCount}");

                if (filteredCount == 0)
                {
                    _logger.LogWarning("No records match the filter criteria");
                    return result;
                }

                // Calculate metrics
                var totalActions = await query.CountAsync();
                var uniqueForms = await query
                    .Select(fh => new { fh.WorkFlowTypeId, fh.FormId })
                    .Distinct()
                    .CountAsync();

                // Calculate average processing days in memory to avoid translation issues
                var formHistoryData = await query
                    .Select(fh => new
                    {
                        fh.WorkFlowTypeId,
                        fh.FormId,
                        fh.ActionDate
                    })
                    .ToListAsync();

                var formGroups = formHistoryData
                    .GroupBy(fh => new { fh.WorkFlowTypeId, fh.FormId })
                    .Where(g => g.Count() > 1)
                    .Select(g => new
                    {
                        g.Key.WorkFlowTypeId,
                        g.Key.FormId,
                        MinDate = g.Min(x => x.ActionDate),
                        MaxDate = g.Max(x => x.ActionDate),
                        DaysDiff = (g.Max(x => x.ActionDate) - g.Min(x => x.ActionDate)).TotalDays
                    })
                    .ToList();

                var avgProcessingDays = formGroups.Any()
                    ? formGroups.Average(x => x.DaysDiff)
                    : 0;

                _logger.LogInformation($"Metrics - Actions: {totalActions}, Unique Forms: {uniqueForms}, Avg Days: {avgProcessingDays:F1}");

                result.Metrics = new List<WorkflowMetric>
        {
            new WorkflowMetric
            {
                Name = "Total Actions",
                Value = totalActions.ToString(),
                Description = "Total workflow actions performed",
                Color = "#007bff"
            },
            new WorkflowMetric
            {
                Name = "Unique Forms",
                Value = uniqueForms.ToString(),
                Description = "Number of forms processed",
                Color = "#28a745"
            },
            new WorkflowMetric
            {
                Name = "Avg Processing Days",
                Value = $"{avgProcessingDays:F1}",
                Description = "Average days to complete workflow",
                Color = "#ffc107"
            }
        };

                // Get timeline data
                result.Timeline = await query
                    .OrderByDescending(fh => fh.ActionDate)
                    .Take(50)
                    .Select(fh => new WorkflowTimelineItem
                    {
                        ActionDate = fh.ActionDate,
                        Action = fh.Action,
                        UserName = fh.ActionByUserName,
                        FormType = fh.WorkFlowTypeId == 1 ? "Purchase Request" :
                                  fh.WorkFlowTypeId == 2 ? "Purchase Order" :
                                  fh.WorkFlowTypeId == 3 ? "Payment Request" : "Unknown",
                        FormId = fh.FormId,
                        Comments = fh.Comments ?? ""
                    })
                    .ToListAsync();

                _logger.LogInformation($"Timeline items: {result.Timeline.Count}");

                // Get user productivity - simplified to avoid translation issues
                var userHistoryData = await query
                    .Select(fh => new
                    {
                        fh.ActionByUserId,
                        fh.ActionByUserName,
                        fh.ActionDate,
                        fh.WorkFlowTypeId,
                        fh.FormId
                    })
                    .ToListAsync();

                // Calculate in memory
                var userProductivityData = userHistoryData
                    .GroupBy(fh => new { fh.ActionByUserId, fh.ActionByUserName })
                    .Select(g => new
                    {
                        g.Key.ActionByUserId,
                        g.Key.ActionByUserName,
                        TotalActions = g.Count(),
                        LastActivity = g.Max(x => x.ActionDate),
                        FormGroups = g.GroupBy(x => new { x.WorkFlowTypeId, x.FormId })
                                      .Where(fg => fg.Count() > 1)
                                      .Select(fg => new
                                      {
                                          MinDate = fg.Min(x => x.ActionDate),
                                          MaxDate = fg.Max(x => x.ActionDate),
                                          Days = (fg.Max(x => x.ActionDate) - fg.Min(x => x.ActionDate)).TotalDays
                                      })
                                      .ToList()
                    })
                    .ToList();

                result.UserProductivity = userProductivityData
                    .Select(u => new UserProductivityItem
                    {
                        UserName = u.ActionByUserName ?? "Unknown",
                        TotalActions = u.TotalActions,
                        PendingItems = 0, // This would need additional query based on current state
                        AvgProcessingDays = u.FormGroups.Any()
                            ? (decimal)u.FormGroups.Average(fg => fg.Days)
                            : 0,
                        LastActivity = u.LastActivity
                    })
                    .OrderByDescending(x => x.TotalActions)
                    .Take(10)
                    .ToList();

                _logger.LogInformation($"User productivity items: {result.UserProductivity.Count}");

                // Get state distribution
                var prStateDistribution = await _context.PurchaseRequests
                    .GroupBy(pr => pr.StateId)
                    .Select(g => new { StateId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var poStateDistribution = await _context.PurchaseOrders
                    .GroupBy(po => po.StateId)
                    .Select(g => new { StateId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var paymentStateDistribution = await _context.PaymentRequests
                    .GroupBy(pr => pr.StateId)
                    .Select(g => new { StateId = g.Key, Count = g.Count() })
                    .ToListAsync();

                // Combine all state distributions
                var allStates = prStateDistribution
                    .Concat(poStateDistribution)
                    .Concat(paymentStateDistribution)
                    .GroupBy(x => x.StateId)
                    .Select(g => new { StateId = g.Key, Count = g.Sum(x => x.Count) })
                    .ToList();

                var totalForms = allStates.Sum(x => x.Count);

                result.StateDistribution = allStates
                    .Select(sd => new StateDistributionItem
                    {
                        StateName = GetStatusName(sd.StateId),
                        Count = sd.Count,
                        Percentage = totalForms > 0 ? (decimal)sd.Count / totalForms * 100 : 0,
                        Color = GetStateColor(sd.StateId)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                _logger.LogInformation($"State distribution items: {result.StateDistribution.Count}");
                _logger.LogInformation($"=== Workflow Analysis Report Complete ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetWorkflowAnalysisReportAsync");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
            }

            return result;
        }
        #endregion

        #region PO Reorder Reports

        public async Task<ReportResponse<POReorderReportItem>> GetPOReordersReportAsync(POReorderReportFilter filter, int currentUserId)
        {
            try
            {
                _logger.LogInformation("=== Starting PO Reorder Report ===");

                // Get all POs that have "Reorder" in their description or title
                var reorderQuery = _context.PurchaseOrders
                    .Include(po => po.CreatedByUser)
                    .Include(po => po.Department)
                    .Include(po => po.Branch)
                    .Include(po => po.Supplier)
                    .Include(po => po.Items)
                    .Where(po => po.Description != null &&
                           (po.Description.Contains("Reorder from PO:") ||
                            po.Title != null && po.Title.Contains("Reorder of")))
                    .AsQueryable();

                var totalRecords = await reorderQuery.CountAsync();
                _logger.LogInformation($"Total reorder POs found: {totalRecords}");

                // Apply filters
                if (filter.OnlyMyReorders)
                {
                    reorderQuery = reorderQuery.Where(po => po.CreatedByUserId == currentUserId);
                }

                if (filter.FromDate.HasValue)
                    reorderQuery = reorderQuery.Where(po => po.PODate >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    reorderQuery = reorderQuery.Where(po => po.PODate <= filter.ToDate.Value);

                if (filter.DepartmentId.HasValue)
                    reorderQuery = reorderQuery.Where(po => po.DepartmentId == filter.DepartmentId.Value);

                if (filter.BranchId.HasValue)
                    reorderQuery = reorderQuery.Where(po => po.BranchId == filter.BranchId.Value);

                if (filter.SupplierId.HasValue)
                    reorderQuery = reorderQuery.Where(po => po.VendorId == filter.SupplierId.Value);

                if (filter.ReorderedByUserId.HasValue)
                    reorderQuery = reorderQuery.Where(po => po.CreatedByUserId == filter.ReorderedByUserId.Value);

                if (!string.IsNullOrEmpty(filter.OriginalPONumber))
                {
                    string searchNumber = filter.OriginalPONumber;

                    // ✅ FIX: Filter to search in Reordered PO Number (PONumber) OR Original PO Number (in Description)
                    reorderQuery = reorderQuery.Where(po =>
                        (po.PONumber != null && po.PONumber.Contains(searchNumber)) ||
                        (po.Description != null && po.Description.Contains(searchNumber))
                    );
                }

                if (!string.IsNullOrEmpty(filter.SearchValue))
                {
                    reorderQuery = reorderQuery.Where(po =>
                        (po.PONumber != null && po.PONumber.Contains(filter.SearchValue)) ||
                        (po.Title != null && po.Title.Contains(filter.SearchValue)) ||
                        (po.CreatedByUser != null && po.CreatedByUser.UserName.Contains(filter.SearchValue)));
                }

                var filteredRecords = await reorderQuery.CountAsync();
                _logger.LogInformation($"Records after filtering: {filteredRecords}");

                // Apply sorting and paging
                var pagedData = await reorderQuery
                    .OrderByDescending(po => po.PODate)
                    .Skip(filter.PageIndex * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {pagedData.Count} reorder records");

                // Build report items
                var reportItems = new List<POReorderReportItem>();

                foreach (var reorderedPO in pagedData)
                {
                    // Extract original PO number from description
                    string originalPONumber = ExtractOriginalPONumber(reorderedPO.Description ?? "", reorderedPO.Title ?? "");

                    // Find the original PO
                    var originalPO = await _context.PurchaseOrders
                        .Include(po => po.Items)
                        .FirstOrDefaultAsync(po => po.PONumber == originalPONumber);

                    decimal originalTotalQty = originalPO?.Items?.Sum(i => i.Quantity) ?? 0;
                    decimal reorderedTotalQty = reorderedPO.Items?.Sum(i => i.Quantity) ?? 0;

                    int reorderCount = 0;
                    if (!string.IsNullOrEmpty(originalPONumber) && originalPO != null)
                    {
                        reorderCount = await _context.PurchaseOrders
                           .Where(po => po.Description != null &&
                                        po.Description.Contains($"Reorder from PO: {originalPONumber}")
                           )
                           .CountAsync();
                    }


                    // Skip if frequency filter doesn't match
                    if (filter.Frequency.HasValue)
                    {
                        bool shouldInclude = filter.Frequency.Value switch
                        {
                            ReorderFrequency.FirstTime => reorderCount == 1,
                            ReorderFrequency.Multiple => reorderCount >= 2 && reorderCount <= 3,
                            ReorderFrequency.Frequent => reorderCount >= 4,
                            _ => true
                        };

                        if (!shouldInclude) continue;
                    }

                    var item = new POReorderReportItem
                    {
                        ReorderedPOId = reorderedPO.Id,
                        ReorderedPONumber = reorderedPO.PONumber ?? "",
                        OriginalPONumber = originalPONumber,
                        OriginalPOId = originalPO?.Id ?? 0,
                        Title = reorderedPO.Title ?? "",
                        ReorderDate = reorderedPO.PODate,
                        ReorderedByUserName = reorderedPO.CreatedByUser?.UserName ?? "",
                        SupplierName = reorderedPO.Supplier?.SupplierName ?? "",
                        DepartmentName = reorderedPO.Department?.DepartmentName ?? "",
                        BranchName = reorderedPO.Branch?.BranchName ?? "",
                        OriginalAmount = originalPO?.TotalAmount ?? 0,
                        ReorderedAmount = reorderedPO.TotalAmount,
                        AmountDifference = reorderedPO.TotalAmount - (originalPO?.TotalAmount ?? 0),
                        PercentageChange = originalPO != null && originalPO.TotalAmount > 0
                            ? ((reorderedPO.TotalAmount - originalPO.TotalAmount) / originalPO.TotalAmount) * 100
                            : 0,
                        ItemCount = reorderedPO.Items?.Count ?? 0,
                        ReorderCount = reorderCount,
                        StatusName = GetStatusName(reorderedPO.StateId),
                        OriginalQty = originalTotalQty,
                        ReorderQty = reorderedTotalQty
                    };

                    reportItems.Add(item);
                }

                _logger.LogInformation($"=== PO Reorder Report Complete: {reportItems.Count} items ===");

                return new ReportResponse<POReorderReportItem>
                {
                    Data = reportItems,
                    TotalRecords = totalRecords,
                    FilteredRecords = filteredRecords,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPOReordersReportAsync");
                return new ReportResponse<POReorderReportItem>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Data = new List<POReorderReportItem>(),
                    TotalRecords = 0,
                    FilteredRecords = 0
                };
            }
        }

        public async Task<POReorderSummaryData> GetPOReorderSummaryAsync(POReorderReportFilter filter, int currentUserId)
        {
            var result = new POReorderSummaryData();

            try
            {
                var fromDate = filter.FromDate ?? DateTime.Now.AddMonths(-6);
                var toDate = filter.ToDate ?? DateTime.Now;

                // Get all reorder POs
                var reorderPOs = await _context.PurchaseOrders
                    .Include(po => po.Items)
                    .Include(po => po.Supplier)
                    .Include(po => po.CreatedByUser)
                    .Where(po => po.Description != null &&
                                (po.Description.Contains("Reorder from PO:") ||
                                 po.Title != null && po.Title.Contains("Reorder of")) &&
                                po.PODate >= fromDate && po.PODate <= toDate)
                    .ToListAsync();

                // Summary Cards
                var totalReorders = reorderPOs.Count;
                var totalReorderAmount = reorderPOs.Sum(po => po.TotalAmount);
                var avgReorderAmount = totalReorders > 0 ? totalReorderAmount / totalReorders : 0;
                var uniqueSuppliers = reorderPOs.Select(po => po.VendorId).Distinct().Count();

                result.Cards = new List<ReorderSummaryCard>
        {
            new ReorderSummaryCard
            {
                Title = "Total Reorders",
                Value = totalReorders.ToString(),
                SubValue = $"in last {(toDate - fromDate).Days} days",
                Icon = "fas fa-redo",
                Color = "primary"
            },
            new ReorderSummaryCard
            {
                Title = "Total Amount",
                Value = $"{totalReorderAmount:C}",
                SubValue = "All reorder POs",
                Icon = "fas fa-dollar-sign",
                Color = "success"
            },
            new ReorderSummaryCard
            {
                Title = "Average Amount",
                Value = $"{avgReorderAmount:C}",
                SubValue = "Per reorder",
                Icon = "fas fa-calculator",
                Color = "info"
            },
            new ReorderSummaryCard
            {
                Title = "Unique Suppliers",
                Value = uniqueSuppliers.ToString(),
                SubValue = "Reordered from",
                Icon = "fas fa-truck",
                Color = "warning"
            }
        };

                // Top Reordered Items
                result.TopReorderedItems = reorderPOs
                    .SelectMany(po => po.Items.Select(item => new
                    {
                        po.VendorId,
                        po.Supplier,
                        item.ItemName,
                        po.PODate,
                        po.TotalAmount
                    }))
                    .GroupBy(x => new { x.ItemName, x.VendorId })
                    .Select(g => new TopReorderedItem
                    {
                        ItemName = g.Key.ItemName ?? "",
                        SupplierName = g.First().Supplier?.SupplierName ?? "",
                        ReorderCount = g.Count(),
                        TotalAmount = g.Sum(x => x.TotalAmount),
                        LastReorderDate = g.Max(x => x.PODate)
                    })
                    .OrderByDescending(x => x.ReorderCount)
                    .Take(10)
                    .ToList();

                // Monthly Trends
                result.Trends = reorderPOs
                    .GroupBy(po => new { po.PODate.Year, po.PODate.Month })
                    .Select(g => new ReorderTrendItem
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                        ReorderCount = g.Count(),
                        TotalAmount = g.Sum(po => po.TotalAmount),
                        AverageAmount = g.Average(po => po.TotalAmount)
                    })
                    .OrderBy(x => x.Period)
                    .ToList();

                // Charts
                var supplierChart = new ReorderChartData
                {
                    Title = "Reorders by Supplier",
                    Type = "bar",
                    Labels = reorderPOs.GroupBy(po => po.Supplier?.SupplierName ?? "Unknown")
                                       .OrderByDescending(g => g.Count())
                                       .Take(10)
                                       .Select(g => g.Key)
                                       .ToList(),
                    Data = reorderPOs.GroupBy(po => po.Supplier?.SupplierName ?? "Unknown")
                                    .OrderByDescending(g => g.Count())
                                    .Take(10)
                                    .Select(g => (decimal)g.Count())
                                    .ToList(),
                    Colors = new List<string> { "#092963", "#1e458e", "#2a5aa8", "#3670c2", "#4286dc" }
                };

                result.Charts.Add(supplierChart);

                _logger.LogInformation($"Generated reorder summary with {totalReorders} reorders");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PO reorder summary");
            }

            return result;
        }

        public async Task<byte[]> ExportPOReordersToExcelAsync(List<POReorderReportItem> data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("PO Reorders Report");

            // Title
            worksheet.Cell(1, 1).Value = "Purchase Order Reorders Report";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, 15).Merge();

            worksheet.Cell(2, 1).Value = $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            worksheet.Range(2, 1, 2, 15).Merge();

            // Headers
            var headers = new[]
            {
        "Reordered PO#", "Original PO#", "Title", "Reorder Date", "Reordered By",
        "Supplier", "Department", "Branch", "Original Amount", "Reordered Amount",
        "Amount Difference", "% Change", "Item Count", "Days Since Original",
        "Reorder Count", "Status"
    };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(4, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#092963");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Data
            int row = 5;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.ReorderedPONumber;
                worksheet.Cell(row, 2).Value = item.OriginalPONumber;
                worksheet.Cell(row, 3).Value = item.Title;
                worksheet.Cell(row, 4).Value = item.ReorderDate.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 5).Value = item.ReorderedByUserName;
                worksheet.Cell(row, 6).Value = item.SupplierName;
                worksheet.Cell(row, 7).Value = item.DepartmentName;
                worksheet.Cell(row, 8).Value = item.BranchName;

                worksheet.Cell(row, 9).Value = item.OriginalAmount;
                worksheet.Cell(row, 9).Style.NumberFormat.Format = "#,##0.00";

                worksheet.Cell(row, 10).Value = item.ReorderedAmount;
                worksheet.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00";

                worksheet.Cell(row, 11).Value = item.AmountDifference;
                worksheet.Cell(row, 11).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(row, 11).Style.Font.FontColor = item.AmountDifference >= 0
                    ? XLColor.Green : XLColor.Red;

                worksheet.Cell(row, 12).Value = item.PercentageChange;
                worksheet.Cell(row, 12).Style.NumberFormat.Format = "0.00%";

                worksheet.Cell(row, 13).Value = item.ItemCount;
                worksheet.Cell(row, 14).Value = item.DaysSinceOriginal;
                worksheet.Cell(row, 15).Value = item.ReorderCount;
                worksheet.Cell(row, 16).Value = item.StatusName;

                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                row++;
            }

            // Summary
            row += 2;
            worksheet.Cell(row, 1).Value = "Summary";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Font.FontSize = 14;

            row++;
            worksheet.Cell(row, 1).Value = "Total Reorders:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = data.Count;

            row++;
            worksheet.Cell(row, 1).Value = "Total Reordered Amount:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            var totalReorderedCell = worksheet.Cell(row, 2);
            totalReorderedCell.Value = data.Sum(x => x.ReorderedAmount);
            totalReorderedCell.Style.NumberFormat.Format = "#,##0.00";

            row++;
            worksheet.Cell(row, 1).Value = "Average Reorder Amount:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            var avgCell = worksheet.Cell(row, 2);
            avgCell.Value = data.Any() ? data.Average(x => x.ReorderedAmount) : 0;
            avgCell.Style.NumberFormat.Format = "#,##0.00";

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private string ExtractOriginalPONumber(string description, string title)
        {
            // Try from description first: "Reorder from PO: PO20250101001"
            if (description.Contains("Reorder from PO:"))
            {
                var start = description.IndexOf("Reorder from PO:") + "Reorder from PO:".Length;
                var end = description.IndexOf(".", start);
                if (end == -1) end = description.IndexOf(" ", start);
                if (end == -1) end = description.Length;

                return description.Substring(start, end - start).Trim();
            }

            // Try from title: "Reorder of PO20250101001"
            if (title.Contains("Reorder of "))
            {
                return title.Replace("Reorder of ", "").Trim();
            }

            return "Unknown";
        }

        #endregion
        #region Procurement Summary Reports

        public async Task<ProcurementSummaryData> GetProcurementSummaryReportAsync(ProcurementSummaryFilter filter, int currentUserId)
        {
            var result = new ProcurementSummaryData();

            try
            {
                var fromDate = filter.FromDate ?? DateTime.Now.AddMonths(-3);
                var toDate = filter.ToDate ?? DateTime.Now;

                // Get summary cards
                var totalPRs = await _context.PurchaseRequests
                    .Where(pr => pr.CreatedOn >= fromDate && pr.CreatedOn <= toDate)
                    .CountAsync();

                var totalPOs = await _context.PurchaseOrders
                    .Where(po => po.CreatedOn >= fromDate && po.CreatedOn <= toDate)
                    .CountAsync();

                var totalPRAmount = await _context.PurchaseRequests
                    .Where(pr => pr.CreatedOn >= fromDate && pr.CreatedOn <= toDate)
                    .SumAsync(pr => pr.Items.Sum(i => i.Quantity * i.UnitPrice));

                var totalPOAmount = await _context.PurchaseOrders
                    .Where(po => po.CreatedOn >= fromDate && po.CreatedOn <= toDate)
                    .SumAsync(po => po.TotalAmount);

                result.Cards = new List<SummaryCard>
                {
                    new SummaryCard { Title = "Total PRs", Value = totalPRs.ToString(), Icon = "fas fa-file-alt", Color = "primary" },
                    new SummaryCard { Title = "Total POs", Value = totalPOs.ToString(), Icon = "fas fa-shopping-cart", Color = "success" },
                    new SummaryCard { Title = "PR Amount", Value = $"${totalPRAmount:N2}", Icon = "fas fa-dollar-sign", Color = "info" },
                    new SummaryCard { Title = "PO Amount", Value = $"${totalPOAmount:N2}", Icon = "fas fa-money-bill-wave", Color = "warning" }
                };

                // Generate charts based on GroupBy selection
                await GenerateChartData(result, filter, fromDate, toDate);

                // Generate table data
                await GenerateTableData(result, filter, fromDate, toDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Procurement Summary report");
            }

            return result;
        }

        #endregion

        #region Excel Export Methods

        public async Task<byte[]> ExportPurchaseRequestsToExcelAsync(List<PurchaseRequestReportItem> data)
{
    using var package = new ExcelPackage();
    var worksheet = package.Workbook.Worksheets.Add("Purchase Requests Report");

    // Headers
    var headers = new string[]
    {
        "Request Number", "Title", "Description", "Request Date", "Required Date",
        "Requested By", "Department", "Branch", "Status", "Purchase Nature",
        "Purchase Type", "Total Amount", "Owner", "Created On", "Days in State", "Has POs"
    };

    for (int i = 0; i < headers.Length; i++)
    {
        worksheet.Cells[1, i + 1].Value = headers[i];
        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
        worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
    }

            // Data
            for (int row = 0; row < data.Count; row++)
            {
                var item = data[row];
                worksheet.Cells[row + 2, 1].Value = item.RequestNumber;
                worksheet.Cells[row + 2, 2].Value = item.Title;
                worksheet.Cells[row + 2, 3].Value = item.Description;

                // Corrected: Directly call ToShortDateString() on DateTime
                worksheet.Cells[row + 2, 4].Value = item.RequestDate.ToShortDateString();

                // Corrected: Check for nullability on RequiredDate and CreatedOn
                worksheet.Cells[row + 2, 5].Value = item.RequiredDate?.ToShortDateString();

                worksheet.Cells[row + 2, 6].Value = item.RequestedByUserName;
                worksheet.Cells[row + 2, 7].Value = item.DepartmentName;
                worksheet.Cells[row + 2, 8].Value = item.BranchName;
                worksheet.Cells[row + 2, 9].Value = item.StatusName;
                worksheet.Cells[row + 2, 10].Value = item.PurchaseNature;
                worksheet.Cells[row + 2, 11].Value = item.PurchaseType;

                // Set value and number format for currency
                worksheet.Cells[row + 2, 12].Value = item.TotalAmount;
                worksheet.Cells[row + 2, 12].Style.Numberformat.Format = "$#,##0.00";

                worksheet.Cells[row + 2, 13].Value = item.Owner;

                // Corrected: Check for nullability on CreatedOn
                worksheet.Cells[row + 2, 14].Value = item.CreatedOn.ToShortDateString();

                worksheet.Cells[row + 2, 15].Value = item.DaysInCurrentState;
                worksheet.Cells[row + 2, 16].Value = item.HasPurchaseOrders ? "Yes" : "No";
            }


            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

    return await Task.FromResult(package.GetAsByteArray());
}

        public async Task<byte[]> ExportPurchaseOrdersToExcelAsync(List<PurchaseOrderReportItem> data)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Purchase Orders Report");

            // Headers
            var headers = new string[]
            {
                "PO Number", "Title", "Description", "PO Date", "Expected Delivery", "Supplier",
                "Created By", "Department", "Branch", "Status", "Total Amount", "Tax Amount",
                "Grand Total", "Owner", "Created On", "Days in State", "Related PRs", "Item Count"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            }

            // Data
            for (int row = 0; row < data.Count; row++)
            {
                var item = data[row];
                worksheet.Cells[row + 2, 1].Value = item.PONumber;
                worksheet.Cells[row + 2, 2].Value = item.Title;
                worksheet.Cells[row + 2, 3].Value = item.Description;
                worksheet.Cells[row + 2, 4].Value = item.PODate;
                worksheet.Cells[row + 2, 5].Value = item.ExpectedDeliveryDate;
                worksheet.Cells[row + 2, 6].Value = item.SupplierName;
                worksheet.Cells[row + 2, 7].Value = item.CreatedByUserName;
                worksheet.Cells[row + 2, 8].Value = item.DepartmentName;
                worksheet.Cells[row + 2, 9].Value = item.BranchName;
                worksheet.Cells[row + 2, 10].Value = item.StatusName;
                worksheet.Cells[row + 2, 11].Value = item.TotalAmount;
                worksheet.Cells[row + 2, 12].Value = item.TaxAmount;
                worksheet.Cells[row + 2, 13].Value = item.GrandTotal;
                worksheet.Cells[row + 2, 14].Value = item.Owner;
                worksheet.Cells[row + 2, 15].Value = item.CreatedOn;
                worksheet.Cells[row + 2, 16].Value = item.DaysInCurrentState;
                worksheet.Cells[row + 2, 17].Value = item.RelatedPRNumbers;
                worksheet.Cells[row + 2, 18].Value = item.ItemCount;
            }

            worksheet.Cells.AutoFitColumns();
            return await Task.FromResult(package.GetAsByteArray());
        }

        public async Task<byte[]> ExportProcurementSummaryToExcelAsync(ProcurementSummaryData data)
        {
            using var package = new ExcelPackage();

            // Summary sheet
            var summarySheet = package.Workbook.Worksheets.Add("Summary");
            summarySheet.Cells[1, 1].Value = "Procurement Summary Report";
            summarySheet.Cells[1, 1].Style.Font.Size = 16;
            summarySheet.Cells[1, 1].Style.Font.Bold = true;

            int row = 3;
            foreach (var card in data.Cards)
            {
                summarySheet.Cells[row, 1].Value = card.Title;
                summarySheet.Cells[row, 2].Value = card.Value;
                summarySheet.Cells[row, 1].Style.Font.Bold = true;
                row++;
            }

            // Table data sheet
            if (data.TableData.Any())
            {
                var tableSheet = package.Workbook.Worksheets.Add("Details");
                var tableHeaders = new string[] { "Category", "Request Count", "Request Amount", "Order Count", "Order Amount", "Conversion Rate", "Avg Processing Days" };

                for (int i = 0; i < tableHeaders.Length; i++)
                {
                    tableSheet.Cells[1, i + 1].Value = tableHeaders[i];
                    tableSheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                for (int i = 0; i < data.TableData.Count; i++)
                {
                    var item = data.TableData[i];
                    tableSheet.Cells[i + 2, 1].Value = item.Category;
                    tableSheet.Cells[i + 2, 2].Value = item.RequestCount;
                    tableSheet.Cells[i + 2, 3].Value = item.RequestAmount;
                    tableSheet.Cells[i + 2, 4].Value = item.OrderCount;
                    tableSheet.Cells[i + 2, 5].Value = item.OrderAmount;
                    tableSheet.Cells[i + 2, 6].Value = item.ConversionRate;
                    tableSheet.Cells[i + 2, 7].Value = item.AvgProcessingDays;
                }
                tableSheet.Cells.AutoFitColumns();
            }

            return await Task.FromResult(package.GetAsByteArray());
        }

        #endregion

        #region Private Helper Methods
        private IOrderedQueryable<PurchaseRequest> ApplySortingToPurchaseRequests(IQueryable<PurchaseRequest> query, int columnIndex, string direction)
        {
            // Default sort column index for Purchase Requests is 2 (Request Date)
            // The columns are:
            // 0: Request Number
            // 1: Title
            // 2: Request Date
            // 3: Requested By
            // 4: Department
            // 5: Status
            // 6: Total Amount (Special case, not directly sortable via IQueryable)
            // 7: Days in State

            var sortQuery = query.OrderBy(pr => pr.CreatedOn); // A default sort to start with

            switch (columnIndex)
            {
                case 0:
                    sortQuery = direction == "asc" ? query.OrderBy(pr => pr.RequestNumber) : query.OrderByDescending(pr => pr.RequestNumber);
                    break;
                case 1:
                    sortQuery = direction == "asc" ? query.OrderBy(pr => pr.Title) : query.OrderByDescending(pr => pr.Title);
                    break;
                case 2:
                    sortQuery = direction == "asc" ? query.OrderBy(pr => pr.RequestDate) : query.OrderByDescending(pr => pr.RequestDate);
                    break;
                case 3:
                    sortQuery = direction == "asc" ? query.OrderBy(pr => pr.RequestedByUser.UserName) : query.OrderByDescending(pr => pr.RequestedByUser.UserName);
                    break;
                case 4:
                    sortQuery = direction == "asc" ? query.OrderBy(pr => pr.Department.DepartmentName) : query.OrderByDescending(pr => pr.Department.DepartmentName);
                    break;
                case 5:
                    sortQuery = direction == "asc" ? query.OrderBy(pr => pr.StateId) : query.OrderByDescending(pr => pr.StateId);
                    break;
                case 6:
                    // Sorting by TotalAmount is not directly translatable to SQL by EF Core because it's a computed property.
                    // The logic for this must be performed in-memory after the query is executed.
                    // For server-side processing, it's best to sort by another column to avoid errors.
                    // We'll fall through to the default sorting by RequestDate.
                    sortQuery = direction == "asc" ? query.OrderBy(pr => pr.RequestDate) : query.OrderByDescending(pr => pr.RequestDate);
                    break;
                case 7:
                    sortQuery = direction == "asc" ? query.OrderBy(pr => pr.CreatedOn) : query.OrderByDescending(pr => pr.CreatedOn);
                    break;
                default:
                    sortQuery = direction == "asc" ? query.OrderBy(pr => pr.CreatedOn) : query.OrderByDescending(pr => pr.CreatedOn);
                    break;
            }

            return sortQuery;
        }
        //private IOrderedQueryable<PurchaseRequest> ApplySortingToPurchaseRequests(IQueryable<PurchaseRequest> query, int columnIndex, string direction)
        //{
        //    return columnIndex switch
        //    {
        //        0 => direction == "asc" ? query.OrderBy(pr => pr.RequestNumber) : query.OrderByDescending(pr => pr.RequestNumber),
        //        1 => direction == "asc" ? query.OrderBy(pr => pr.RequestDate) : query.OrderByDescending(pr => pr.RequestDate),
        //        2 => direction == "asc" ? query.OrderBy(pr => pr.RequestedByUser.UserName) : query.OrderByDescending(pr => pr.RequestedByUser.UserName),
        //        3 => direction == "asc" ? query.OrderBy(pr => pr.StateId) : query.OrderByDescending(pr => pr.StateId),
        //        _ => direction == "asc" ? query.OrderBy(pr => pr.CreatedOn) : query.OrderByDescending(pr => pr.CreatedOn)
        //    };
        //}

        private IOrderedQueryable<PurchaseOrder> ApplySortingToPurchaseOrders(IQueryable<PurchaseOrder> query, int columnIndex, string direction)
        {
            return columnIndex switch
            {
                0 => direction == "asc" ? query.OrderBy(po => po.PONumber) : query.OrderByDescending(po => po.PONumber),
                1 => direction == "asc" ? query.OrderBy(po => po.PODate) : query.OrderByDescending(po => po.PODate),
                2 => direction == "asc" ? query.OrderBy(po => po.Supplier.SupplierName) : query.OrderByDescending(po => po.Supplier.SupplierName),
                3 => direction == "asc" ? query.OrderBy(po => po.TotalAmount) : query.OrderByDescending(po => po.TotalAmount),
                _ => direction == "asc" ? query.OrderBy(po => po.CreatedOn) : query.OrderByDescending(po => po.CreatedOn)
            };
        }

        private IQueryable<PaymentRequest> ApplySortingToPaymentRequests(
            IQueryable<PaymentRequest> query,
            int sortColumnIndex,
            string sortDirection)
        {
            var isAscending = sortDirection.ToLower() == "asc";

            return sortColumnIndex switch
            {
                0 => isAscending ? query.OrderBy(pr => pr.PRQNumber) : query.OrderByDescending(pr => pr.PRQNumber),
                1 => isAscending ? query.OrderBy(pr => pr.PayeeName) : query.OrderByDescending(pr => pr.PayeeName),
                2 => isAscending ? query.OrderBy(pr => pr.RequiredDate) : query.OrderByDescending(pr => pr.RequiredDate),
                3 => isAscending ? query.OrderBy(pr => pr.CreatedByUser.UserName) : query.OrderByDescending(pr => pr.CreatedByUser.UserName),
                4 => isAscending ? query.OrderBy(pr => pr.DepartmentCode) : query.OrderByDescending(pr => pr.DepartmentCode),
                5 => isAscending ? query.OrderBy(pr => pr.PaymentNatureId) : query.OrderByDescending(pr => pr.PaymentNatureId),
                6 => isAscending ? query.OrderBy(pr => pr.StateId) : query.OrderByDescending(pr => pr.StateId),
                7 => isAscending ? query.OrderBy(pr => pr.Details.Sum(d => d.TotalAmount)) : query.OrderByDescending(pr => pr.Details.Sum(d => d.TotalAmount)),
                8 => isAscending ? query.OrderBy(pr => pr.UpdatedOn ?? pr.CreatedOn) : query.OrderByDescending(pr => pr.UpdatedOn ?? pr.CreatedOn),
                _ => query.OrderByDescending(pr => pr.CreatedOn)
            };
        }
        private async Task GenerateChartData(ProcurementSummaryData result, ProcurementSummaryFilter filter, DateTime fromDate, DateTime toDate)
        {
            switch (filter.GroupBy)
            {
                case ProcurementGroupBy.Department:
                    await GenerateDepartmentChart(result, fromDate, toDate);
                    break;
                case ProcurementGroupBy.Month:
                    await GenerateMonthlyChart(result, fromDate, toDate);
                    break;
                case ProcurementGroupBy.Status:
                    await GenerateStatusChart(result, fromDate, toDate);
                    break;
            }
        }

        private async Task GenerateDepartmentChart(ProcurementSummaryData result, DateTime fromDate, DateTime toDate)
        {
            
            var departmentData = await _context.PurchaseRequests
                .Include(pr => pr.Department)
                .Where(pr => pr.CreatedOn >= fromDate && pr.CreatedOn <= toDate && pr.Department != null)
                .GroupBy(pr => pr.Department.DepartmentName)
                .Select(g => new {
                    Department = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(pr => pr.Items.Sum(i => i.Quantity * i.UnitPrice))
                })
                .ToListAsync();

            var chartData = new ChartData
            {
                Title = "Requests by Department",
                Type = "bar",
                Labels = departmentData.Select(d => d.Department).ToList(),
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset
                    {
                        Label = "Request Count",
                        Data = departmentData.Select(d => (decimal)d.Count).ToList(),
                        BackgroundColor = new List<string> { "#007bff" }
                    }
                }
            };

            result.Charts.Add(chartData);
        }

        private async Task GenerateMonthlyChart(ProcurementSummaryData result, DateTime fromDate, DateTime toDate)
        {
            var monthlyData = await _context.PurchaseRequests
                .Where(pr => pr.CreatedOn >= fromDate && pr.CreatedOn <= toDate)
                .GroupBy(pr => new { pr.CreatedOn.Year, pr.CreatedOn.Month })
                .Select(g => new {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count(),
                    Amount = g.Sum(pr => pr.Items.Sum(i => i.Quantity * i.UnitPrice))
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            var chartData = new ChartData
            {
                Title = "Monthly Trends",
                Type = "line",
                Labels = monthlyData.Select(d => d.Month).ToList(),
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset
                    {
                        Label = "Requests",
                        Data = monthlyData.Select(d => (decimal)d.Count).ToList(),
                        BackgroundColor = new List<string> { "#28a745" }
                    }
                }
            };

            result.Charts.Add(chartData);
        }

        private async Task GenerateStatusChart(ProcurementSummaryData result, DateTime fromDate, DateTime toDate)
        {
            var statusData = await _context.PurchaseRequests
                .Where(pr => pr.CreatedOn >= fromDate && pr.CreatedOn <= toDate)
                .GroupBy(pr => pr.StateId)
                .Select(g => new { StateId = g.Key, Count = g.Count() })
                .ToListAsync();

            var chartData = new ChartData
            {
                Title = "Status Distribution",
                Type = "pie",
                Labels = statusData.Select(d => GetStatusName(d.StateId)).ToList(),
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset
                    {
                        Label = "Count",
                        Data = statusData.Select(d => (decimal)d.Count).ToList(),
                        BackgroundColor = statusData.Select(d => GetStateColor(d.StateId)).ToList()
                    }
                }
            };

            result.Charts.Add(chartData);
        }

        
        private async Task GenerateTableData(ProcurementSummaryData result, ProcurementSummaryFilter filter, DateTime fromDate, DateTime toDate)
        {
            var prQuery = _context.PurchaseRequests
                .Where(pr => pr.CreatedOn >= fromDate && pr.CreatedOn <= toDate && pr.DepartmentId != null);

            var poQuery = _context.PurchaseOrders
                .Where(po => po.CreatedOn >= fromDate && po.CreatedOn <= toDate && po.DepartmentId != null);

            // Group PRs by department
            var prData = await prQuery
                .GroupBy(pr => new { pr.DepartmentId, pr.Department.DepartmentName })
                .Select(g => new {
                    g.Key.DepartmentId,
                    DepartmentName = g.Key.DepartmentName ?? "N/A",
                    RequestCount = g.Count(),
                    RequestAmount = g.Sum(pr => pr.Items.Sum(i => i.Quantity * i.UnitPrice))
                })
                .ToListAsync();

            // Group POs by department
            var poData = await poQuery
                .GroupBy(po => new { po.DepartmentId, po.Department.DepartmentName })
                .Select(g => new {
                    g.Key.DepartmentId,
                    DepartmentName = g.Key.DepartmentName ?? "N/A",
                    OrderCount = g.Count(),
                    OrderAmount = g.Sum(po => po.TotalAmount)
                })
                .ToListAsync();

            // Join the results in memory
            result.TableData = prData
                .GroupJoin(poData,
                    pr => pr.DepartmentId,
                    po => po.DepartmentId,
                    (pr, poGroup) => new { pr, poGroup })
                .SelectMany(
                    x => x.poGroup.DefaultIfEmpty(),
                    (x, po) => new SummaryTableItem
                    {
                        Category = x.pr.DepartmentName,
                        RequestCount = x.pr.RequestCount,
                        RequestAmount = x.pr.RequestAmount,
                        OrderCount = po?.OrderCount ?? 0,
                        OrderAmount = po?.OrderAmount ?? 0,
                        ConversionRate = x.pr.RequestCount > 0 ? (decimal)(po?.OrderCount ?? 0) / x.pr.RequestCount * 100 : 0,
                        AvgProcessingDays = 7 
                    })
                .ToList();
        }
        private string GetStatusName(short stateId)
        {
            return stateId switch
            {
                1 => "Draft",
                2 => "Submitted",
                3 => "Approved",
                4 => "Rejected",
                5 => "Returned",
                10001 => "Cancelled",
                _ => "Unknown"
            };
        }

        private string GetStateColor(short stateId)
        {
            return stateId switch
            {
                1 => "#6c757d", // Draft - Gray
                2 => "#007bff", // Submitted - Blue
                3 => "#28a745", // Approved - Green
                4 => "#dc3545", // Rejected - Red
                5 => "#ffc107", // Returned - Yellow
                10001 => "#6f42c1", // Cancelled - Purple
                _ => "#dee2e6"
            };
        }

        #endregion
    }
}