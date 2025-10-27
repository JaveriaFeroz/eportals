using ProcureToPay.Areas.Master.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Reports.Models
{
    public abstract class BaseReportFilter
    {
        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Branch")]
        public int? BranchId { get; set; }

        // DataTables server-side processing parameters
        public int Draw { get; set; } = 1;
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 10;
        public string SearchValue { get; set; } = string.Empty;
        public int SortColumnIndex { get; set; } = 0;
        public string SortDirection { get; set; } = "asc";
    }

    public class PurchaseRequestReportFilter : BaseReportFilter
    {
        [Display(Name = "Request Number")]
        public string? RequestNumber { get; set; }

        [Display(Name = "Requested By")]
        public int? RequestedByUserId { get; set; }

        [Display(Name = "Status")]
        public short? StateId { get; set; }

        [Display(Name = "Min Amount")]
        public decimal? MinAmount { get; set; }

        [Display(Name = "Max Amount")]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "Product Nature")]
        public short? ProductNatureId { get; set; } // Nullable

        [ForeignKey("ProductNatureId")]
        public virtual ProductNature? ProducNature { get; set; }

        [Display(Name = "Service Nature")]
        public short? ServiceNatureId { get; set; } // Nullable

        [ForeignKey("ServiceNatureId")]
        public virtual ServiceNature? ServiceNature { get; set; }

        [Display(Name = "Product")]
        public short? ProductId { get; set; } // Nullable

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; } // Nullable nav prop

        [Display(Name = "Service")]
        public short? ServiceId { get; set; } // Nullable

        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; } // Nullable nav prop

        public bool OnlyMyRequests { get; set; } = false;
    }

    public class PurchaseRequestReportItem
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public string RequestedByUserName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string PurchaseNature { get; set; } = string.Empty;
        public string PurchaseType { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Owner { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public int DaysInCurrentState { get; set; }
        public bool HasPurchaseOrders { get; set; }
        public string? ItemDisplay { get; set; }
    }

    public class PurchaseOrderReportFilter : BaseReportFilter
    {
        [Display(Name = "PO Number")]
        public string? PONumber { get; set; }

        [Display(Name = "Supplier")]
        public short? SupplierId { get; set; }

        [Display(Name = "Created By")]
        public int? CreatedByUserId { get; set; }

        [Display(Name = "Status")]
        public short? StateId { get; set; }

        [Display(Name = "Min Amount")]
        public decimal? MinAmount { get; set; }

        [Display(Name = "Max Amount")]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "Expected Delivery From")]
        [DataType(DataType.Date)]
        public DateTime? ExpectedDeliveryFrom { get; set; }

        [Display(Name = "Expected Delivery To")]
        [DataType(DataType.Date)]
        public DateTime? ExpectedDeliveryTo { get; set; }

        [Display(Name = "Product Nature")]
        public short? ProductNatureId { get; set; } // Nullable

        [ForeignKey("ProductNatureId")]
        public virtual ProductNature? ProducNature { get; set; }

        [Display(Name = "Service Nature")]
        public short? ServiceNatureId { get; set; } // Nullable

        [ForeignKey("ServiceNatureId")]
        public virtual ServiceNature? ServiceNature { get; set; }

        [Display(Name = "Product")]
        public short? ProductId { get; set; } // Nullable

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; } // Nullable nav prop

        [Display(Name = "Service")]
        public short? ServiceId { get; set; } // Nullable

        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; } // Nullable nav prop

        public bool OnlyMyOrders { get; set; } = false;
    }

    public class PurchaseOrderReportItem
    {
        public int Id { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime PODate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string CreatedByUserName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string Owner { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public int DaysInCurrentState { get; set; }
        public string RelatedPRNumbers { get; set; } = string.Empty;
        public string? ItemDisplay { get; set; }
        public int ItemCount { get; set; }
    }

    public class WorkflowAnalysisFilter : BaseReportFilter
    {
        [Display(Name = "Workflow Type")]
        public short? WorkFlowTypeId { get; set; }

        [Display(Name = "User")]
        public int? UserId { get; set; }

        [Display(Name = "Action")]
        public string? Action { get; set; }

        [Display(Name = "Include Completed")]
        public bool IncludeCompleted { get; set; } = true;
    }

    public class POReorderReportFilter : BaseReportFilter
    {
        [Display(Name = "PO Number")]
        public string? OriginalPONumber { get; set; }

        [Display(Name = "Reordered By")]
        public int? ReorderedByUserId { get; set; }

        [Display(Name = "Supplier")]
        public short? SupplierId { get; set; }

        [Display(Name = "Min Amount")]
        public decimal? MinAmount { get; set; }

        [Display(Name = "Max Amount")]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "Reorder Count")]
        public ReorderFrequency? Frequency { get; set; }

        public bool OnlyMyReorders { get; set; } = false;
    }

    public class POReorderReportItem
    {
        public int ReorderedPOId { get; set; }
        public string ReorderedPONumber { get; set; } = string.Empty;
        public string OriginalPONumber { get; set; } = string.Empty;
        public int OriginalPOId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime ReorderDate { get; set; }
        public string ReorderedByUserName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public decimal OriginalAmount { get; set; }
        public decimal ReorderedAmount { get; set; }
        public decimal OriginalQty { get; set; }
        public decimal ReorderQty { get; set; }
        public decimal AmountDifference { get; set; }
        public decimal PercentageChange { get; set; }
        public int ItemCount { get; set; }
        public int DaysSinceOriginal { get; set; }
        public int ReorderCount { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }

    public class WorkflowAnalysisData
    {
        public List<WorkflowMetric> Metrics { get; set; } = new List<WorkflowMetric>();
        public List<WorkflowTimelineItem> Timeline { get; set; } = new List<WorkflowTimelineItem>();
        public List<UserProductivityItem> UserProductivity { get; set; } = new List<UserProductivityItem>();
        public List<StateDistributionItem> StateDistribution { get; set; } = new List<StateDistributionItem>();
    }

    public enum ReorderFrequency
    {
        [Display(Name = "First Time")]
        FirstTime = 1,
        [Display(Name = "2-3 Times")]
        Multiple = 2,
        [Display(Name = "4+ Times")]
        Frequent = 3
    }
    public class WorkflowMetric
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = "#007bff";
    }

    public class POReorderSummaryData
    {
        public List<ReorderSummaryCard> Cards { get; set; } = new();
        public List<ReorderChartData> Charts { get; set; } = new();
        public List<TopReorderedItem> TopReorderedItems { get; set; } = new();
        public List<ReorderTrendItem> Trends { get; set; } = new();
    }

    public class ReorderSummaryCard
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string SubValue { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = "primary";
        public decimal Change { get; set; }
        public bool IsPositive { get; set; }
    }

    public class ReorderChartData
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "bar";
        public List<string> Labels { get; set; } = new();
        public List<decimal> Data { get; set; } = new();
        public List<string> Colors { get; set; } = new();
    }

    public class TopReorderedItem
    {
        public string ItemName { get; set; } = string.Empty;
        public int ReorderCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public DateTime LastReorderDate { get; set; }
    }

    public class ReorderTrendItem
    {
        public string Period { get; set; } = string.Empty;
        public int ReorderCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
    }
    public class WorkflowTimelineItem
    {
        public DateTime ActionDate { get; set; }
        public string Action { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FormType { get; set; } = string.Empty;
        public int FormId { get; set; }
        public string Comments { get; set; } = string.Empty;
    }

    public class UserProductivityItem
    {
        public string UserName { get; set; } = string.Empty;
        public int TotalActions { get; set; }
        public int PendingItems { get; set; }
        public decimal AvgProcessingDays { get; set; }
        public DateTime LastActivity { get; set; }
    }

    public class StateDistributionItem
    {
        public string StateName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class ProcurementSummaryFilter : BaseReportFilter
    {
        [Display(Name = "Supplier")]
        public short? SupplierId { get; set; }

        [Display(Name = "Group By")]
        public ProcurementGroupBy GroupBy { get; set; } = ProcurementGroupBy.Department;

        [Display(Name = "Include Drafts")]
        public bool IncludeDrafts { get; set; } = false;

        [Display(Name = "Summary Type")]
        public ProcurementSummaryType SummaryType { get; set; } = ProcurementSummaryType.Overview;
    }

    public enum ProcurementGroupBy
    {
        Department = 1,
        Branch = 2,
        User = 3,
        Supplier = 4,
        Month = 5,
        Status = 6
    }

    public enum ProcurementSummaryType
    {
        Overview = 1,
        Financial = 2,
        Operational = 3,
        Compliance = 4
    }

    public class ProcurementSummaryData
    {
        public List<SummaryCard> Cards { get; set; } = new List<SummaryCard>();
        public List<ChartData> Charts { get; set; } = new List<ChartData>();
        public List<SummaryTableItem> TableData { get; set; } = new List<SummaryTableItem>();
    }

    public class SummaryCard
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string SubValue { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = "primary";
        public decimal PercentageChange { get; set; }
        public bool IsPositive { get; set; } = true;
    }

    public class ChartData
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "bar"; // bar, line, pie, doughnut
        public List<string> Labels { get; set; } = new List<string>();
        public List<ChartDataset> Datasets { get; set; } = new List<ChartDataset>();
    }

    public class ChartDataset
    {
        public string Label { get; set; } = string.Empty;
        public List<decimal> Data { get; set; } = new List<decimal>();
        public List<string> BackgroundColor { get; set; } = new List<string>();
        public List<string> BorderColor { get; set; } = new List<string>();
    }

    public class SummaryTableItem
    {
        public string Category { get; set; } = string.Empty;
        public int RequestCount { get; set; }
        public decimal RequestAmount { get; set; }
        public int OrderCount { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal AvgProcessingDays { get; set; }
    }

    public class ReportResponse<T> where T : class
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public bool Success { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }

    public class PaymentRequestReportFilter
    {
        [Display(Name = "From Date")]
        public DateTime? FromDate { get; set; }

        [Display(Name = "To Date")]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Branch")]
        public int? BranchId { get; set; }

        [Display(Name = "PRQ Number")]
        public string? PRQNumber { get; set; }

        [Display(Name = "Requested By")]
        public int? RequestedByUserId { get; set; }

        [Display(Name = "State")]
        public short? StateId { get; set; }

        [Display(Name = "Payment Nature")]
        public short? PaymentNatureId { get; set; }

        [Display(Name = "Payment Sub Nature")]
        public short? PaymentSubNatureId { get; set; }

        [Display(Name = "Payment Type")]
        public short? PaymentTypeId { get; set; }

        [Display(Name = "Invoice Number")]
        public string? InvoiceNumber { get; set; }

        [Display(Name = "Is GRN Based?")]
        public bool? IsGRNBased { get; set; }

        [Display(Name = "Min Amount")]
        public decimal? MinAmount { get; set; }

        [Display(Name = "Max Amount")]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "Only My Requests")]
        public bool OnlyMyRequests { get; set; }

        [Display(Name = "Payment Mode")]
        public string? PaymentMode { get; set; }

        // Pagination and sorting
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 10;
        public string? SearchValue { get; set; }
        public int SortColumnIndex { get; set; } = 2; // Default to Request Date
        public string SortDirection { get; set; } = "desc";
        public int Draw { get; set; }
    }

    public class PaymentRequestReportData
    {
        public int Id { get; set; }
        public string PRQNumber { get; set; } = string.Empty;
        public DateTime RequiredDate { get; set; }
        public string PayeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int DaysInCurrentState { get; set; }
        public string RequestedByUserName { get; set; } = string.Empty;
        public string PaymentNature { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string PaymentMode { get; set; } = string.Empty;
        public bool IsGRNBased { get; set; }
        public string? GRNNumber { get; set; }
        public string? PIVNo { get; set; }
        public string? CSNo { get; set; }
        public string? PaymentSubNature { get; set; }
        public string? SupplierName { get; set; }
        public string? InvoiceDisplay { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class PaymentRequestReportResult
    {
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public List<PaymentRequestReportData> Data { get; set; } = new();
    }
}