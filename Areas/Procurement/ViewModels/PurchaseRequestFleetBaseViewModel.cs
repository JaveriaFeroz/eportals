using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ProcureToPay.Areas.Procurement.Enums;
using ProcureToPay.Areas.Common.Enums;

namespace ProcureToPay.Areas.Procurement.ViewModels
{
    // Base view model with common properties
    public abstract class PurchaseRequestFleetBaseViewModel
    {
        [Display(Name = "PR No.")]
        public int? PRNo { get; set; }

        [Required(ErrorMessage = "Company Code is required")]
        [Display(Name = "Company")]
        public string CompanyCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Branch Code is required")]
        [Display(Name = "Branch")]
        public string BranchCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department Code is required")]
        [Display(Name = "Department")]
        public string DepartmentCode { get; set; } = string.Empty;

        [Display(Name = "Required By")]
        [Required(ErrorMessage = "Required By date is required")]
        [DataType(DataType.Date)]
        public DateTime? RequiredBy { get; set; }

        [Display(Name = "Product Nature")]
        public short? ProductNatureId { get; set; }

        [Display(Name = "Service Nature")]
        public short? ServiceNatureId { get; set; }

        [Required(ErrorMessage = "Owner is required")]
        [StringLength(100)]
        [Display(Name = "Owner")]
        public string Owner { get; set; } = string.Empty;

        [Required(ErrorMessage = "Justification is required")]
        [StringLength(1000, ErrorMessage = "Justification cannot exceed 1000 characters")]
        [Display(Name = "Justification")]
        [DataType(DataType.MultilineText)]
        public string Justification { get; set; } = string.Empty;

        // Display names for dropdowns
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [Display(Name = "Branch Name")]
        public string? BranchName { get; set; }

        [Display(Name = "Department Name")]
        public string? DepartmentName { get; set; }

        [Display(Name = "Product Nature Name")]
        public string? ProductNatureName { get; set; }

        [Display(Name = "Service Nature Name")]
        public string? ServiceNatureName { get; set; }
    }

    // Create view model for new purchase requests
    public class CreatePurchaseRequestFleetViewModel : PurchaseRequestFleetBaseViewModel
    {
        // File uploads for new attachments
        [Display(Name = "Attachments")]
        public List<IFormFile>? Attachments { get; set; }

        [Display(Name = "Attachment Type")]
        public short? AttachmentTypeId { get; set; }

        // Details collection for creating line items
        public List<CreatePurchaseRequestDetailFleetViewModel> Details { get; set; } = new();

        // Budget information
        [Display(Name = "Budget Amount")]
        [DataType(DataType.Currency)]
        public decimal? BudgetAmount { get; set; }

        [Display(Name = "Budget Remarks")]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string? BudgetRemarks { get; set; }
    }

    // Edit view model for existing purchase requests
    public class EditPurchaseRequestFleetViewModel : PurchaseRequestFleetBaseViewModel
    {
        public int Id { get; set; }

        [Display(Name = "State")]
        public short StateId { get; set; }

        [Display(Name = "State Name")]
        public string StateName { get; set; } = string.Empty;

        [Display(Name = "Workflow")]
        public short WorkFlowId { get; set; }

        [Display(Name = "Is Completed")]
        public bool IsCompleted { get; set; }

        [Display(Name = "Approved")]
        public bool Approved { get; set; }

        [Display(Name = "Rejected")]
        public bool Rejected { get; set; }

        [Display(Name = "Budgeted")]
        public bool Budgeted { get; set; }

        [Display(Name = "Budget Amount")]
        [DataType(DataType.Currency)]
        public decimal? BudgetAmount { get; set; }

        [Display(Name = "Budget Remarks")]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string? BudgetRemarks { get; set; }

        // Current approval sequence for workflow
        [Display(Name = "Current Approval Sequence")]
        public int CurrentApprovalSequence { get; set; }

        // Details collection
        public List<EditPurchaseRequestDetailFleetViewModel> Details { get; set; } = new();

        // File uploads for additional attachments
        [Display(Name = "Add New Attachments")]
        public List<IFormFile>? NewAttachments { get; set; }

        [Display(Name = "Attachment Type")]
        public short? AttachmentTypeId { get; set; }

        // Audit fields
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime? ModifiedDate { get; set; }

        [Display(Name = "Modified By")]
        public string? ModifiedBy { get; set; }
    }

    // Display view model for read-only views
    public class DisplayPurchaseRequestFleetViewModel : PurchaseRequestFleetBaseViewModel
    {
        public int Id { get; set; }

        public short StateId { get; set; }

        [Display(Name = "State")]
        public string StateName { get; set; } = string.Empty;

        public short WorkFlowId { get; set; }
        [Display(Name = "Workflow")]
        public string WorkFlowName { get; set; } = string.Empty;

        [Display(Name = "Is Completed")]
        public bool IsCompleted { get; set; }

        [Display(Name = "Approved")]
        public bool Approved { get; set; }

        [Display(Name = "Rejected")]
        public bool Rejected { get; set; }

        [Display(Name = "Budgeted")]
        public bool Budgeted { get; set; }

        [Display(Name = "Budget Amount")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? BudgetAmount { get; set; }

        [Display(Name = "Budget Remarks")]
        public string? BudgetRemarks { get; set; }

        [Display(Name = "Current Approval Sequence")]
        public int CurrentApprovalSequence { get; set; }

        // Details for display
        public List<DisplayPurchaseRequestDetailFleetViewModel> Details { get; set; } = new();

        // Audit information
        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Modified Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? ModifiedDate { get; set; }

        [Display(Name = "Modified By")]
        public string? ModifiedBy { get; set; }

        // Status indicators
        public string StatusClass => GetStatusClass();
        public string StatusIcon => GetStatusIcon();

        private string GetStatusClass()
        {
            return StateName.ToLower() switch
            {
                "approved" => "success",
                "rejected" => "danger",
                "pending" => "warning",
                "draft" => "secondary",
                _ => "primary"
            };
        }

        private string GetStatusIcon()
        {
            return StateName.ToLower() switch
            {
                "approved" => "fas fa-check-circle",
                "rejected" => "fas fa-times-circle",
                "pending" => "fas fa-clock",
                "draft" => "fas fa-edit",
                _ => "fas fa-file"
            };
        }
    }

    // List view model for grid/table displays
    public class PurchaseRequestFleetListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "PR No.")]
        public int? PRNo { get; set; }

        [Display(Name = "Company")]
        public string CompanyName { get; set; } = string.Empty;

        [Display(Name = "Branch")]
        public string BranchName { get; set; } = string.Empty;

        [Display(Name = "Department")]
        public string DepartmentName { get; set; } = string.Empty;

        [Display(Name = "Owner")]
        public string Owner { get; set; } = string.Empty;

        [Display(Name = "Required By")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? RequiredBy { get; set; }

        [Display(Name = "State")]
        public string StateName { get; set; } = string.Empty;

        [Display(Name = "Budget Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? BudgetAmount { get; set; }

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        // Status indicators for UI
        public string StatusClass { get; set; } = string.Empty;
        public string StatusIcon { get; set; } = string.Empty;

        // Quick action flags
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
    }

    // Summary view model for dashboard/reports
    public class PurchaseRequestFleetSummaryViewModel
    {
        [Display(Name = "Total Requests")]
        public int TotalRequests { get; set; }

        [Display(Name = "Pending Approval")]
        public int PendingApproval { get; set; }

        [Display(Name = "Approved")]
        public int Approved { get; set; }

        [Display(Name = "Rejected")]
        public int Rejected { get; set; }

        [Display(Name = "Draft")]
        public int Draft { get; set; }

        [Display(Name = "Total Budget Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalBudgetAmount { get; set; }

        [Display(Name = "Approved Budget Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal ApprovedBudgetAmount { get; set; }

        // Recent requests for quick access
        public List<PurchaseRequestFleetListViewModel> RecentRequests { get; set; } = new();
    }

    // Detail view models based on PurchaseRequestDetailFleet
    public abstract class PurchaseRequestDetailFleetBaseViewModel
    {
        [Display(Name = "Detail ID")]
        public int? DetailId { get; set; }

        [Display(Name = "Product")]
        public short? ProductId { get; set; }

        [Display(Name = "Product Name")]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Service")]
        public short? ServiceId { get; set; }

        [Display(Name = "Service Name")]
        [StringLength(200)]
        public string ServiceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required")]
        [Display(Name = "Quantity")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public double Quantity { get; set; }

        [Display(Name = "Unit of Measure")]
        public short? UoMId { get; set; }

        [Display(Name = "UoM")]
        [StringLength(50)]
        public string UoMName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public double Price { get; set; }

        [Display(Name = "GST Rate (%)")]
        [Range(0, 100, ErrorMessage = "GST Rate must be between 0 and 100")]
        public double GSTRate { get; set; }

        [Display(Name = "Remarks")]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string? Remarks { get; set; }

        // Calculated properties
        [Display(Name = "Gross Amount")]
        [DataType(DataType.Currency)]
        public double GrossAmount => Math.Round(Quantity * Price, 2);

        [Display(Name = "GST Amount")]
        [DataType(DataType.Currency)]
        public double GSTAmount => Math.Round(GrossAmount * GSTRate / 100.00, 2);

        [Display(Name = "Net Amount")]
        [DataType(DataType.Currency)]
        public double NetAmount => Math.Round(GrossAmount + GSTAmount, 2);
    }

    public class CreatePurchaseRequestDetailFleetViewModel : PurchaseRequestDetailFleetBaseViewModel
    {
        // Additional properties for creation if needed
        public bool IsProductSelected => ProductId.HasValue && !string.IsNullOrEmpty(ProductName);
        public bool IsServiceSelected => ServiceId.HasValue && !string.IsNullOrEmpty(ServiceName);
    }

    public class EditPurchaseRequestDetailFleetViewModel : PurchaseRequestDetailFleetBaseViewModel
    {
        public int Id { get; set; }

        [Required]
        public int PRFleetId { get; set; }

        // Audit fields for tracking changes
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime? ModifiedDate { get; set; }

        [Display(Name = "Modified By")]
        public string? ModifiedBy { get; set; }

        // Flags for UI behavior
        public bool IsNew => Id == 0;
        public bool IsModified { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class DisplayPurchaseRequestDetailFleetViewModel : PurchaseRequestDetailFleetBaseViewModel
    {
        public int Id { get; set; }

        [Display(Name = "PR Fleet ID")]
        public int PRFleetId { get; set; }

        // Override calculated properties with formatting for display
        [Display(Name = "Gross Amount")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public new double GrossAmount { get; set; }

        [Display(Name = "GST Amount")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public new double GSTAmount { get; set; }

        [Display(Name = "Net Amount")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public new double NetAmount { get; set; }

        // Audit information
        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Modified Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? ModifiedDate { get; set; }

        [Display(Name = "Modified By")]
        public string? ModifiedBy { get; set; }

        // Helper properties for UI
        public string ItemType => !string.IsNullOrEmpty(ProductName) ? "Product" : "Service";
        public string ItemName => !string.IsNullOrEmpty(ProductName) ? ProductName : ServiceName;
        public string FormattedGSTRate => $"{GSTRate:F2}%";
    }

    // Summary view model for detail collections
    public class PurchaseRequestDetailFleetSummaryViewModel
    {
        [Display(Name = "Total Items")]
        public int TotalItems { get; set; }

        [Display(Name = "Total Quantity")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double TotalQuantity { get; set; }

        [Display(Name = "Total Gross Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public double TotalGrossAmount { get; set; }

        [Display(Name = "Total GST Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public double TotalGSTAmount { get; set; }

        [Display(Name = "Total Net Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public double TotalNetAmount { get; set; }

        [Display(Name = "Average GST Rate")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public double AverageGSTRate { get; set; }

        public List<DisplayPurchaseRequestDetailFleetViewModel> Details { get; set; } = new();
    }

    // Filter view model for search and filtering
    public class PurchaseRequestFleetFilterViewModel
    {
        [Display(Name = "PR No.")]
        public int? PRNo { get; set; }

        [Display(Name = "Company")]
        public string? CompanyCode { get; set; }

        [Display(Name = "Branch")]
        public string? BranchCode { get; set; }

        [Display(Name = "Department")]
        public string? DepartmentCode { get; set; }

        [Display(Name = "State")]
        public short? StateId { get; set; }

        [Display(Name = "Owner")]
        public string? Owner { get; set; }

        [Display(Name = "Required From")]
        [DataType(DataType.Date)]
        public DateTime? RequiredFromDate { get; set; }

        [Display(Name = "Required To")]
        [DataType(DataType.Date)]
        public DateTime? RequiredToDate { get; set; }

        [Display(Name = "Created From")]
        [DataType(DataType.Date)]
        public DateTime? CreatedFromDate { get; set; }

        [Display(Name = "Created To")]
        [DataType(DataType.Date)]
        public DateTime? CreatedToDate { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortField { get; set; } = "CreatedDate";
        public string SortDirection { get; set; } = "desc";
    }
}