using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Finance.Models;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Finance.ViewModels
{
    // Main index view model
    public class PaymentRequestIndexViewModel
    {
        public List<PaymentRequestListItemViewModel> PaymentRequests { get; set; } = new List<PaymentRequestListItemViewModel>();
    }

    public class PaymentRequestListItemViewModel
    {
        public int Id { get; set; }

        public int? GRNId { get; set; }
        public string PRQNumber { get; set; } = string.Empty;
        public DateTime RequiredDate { get; set; }
        public string PayeeName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public short StateId { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public string? GRNNumber { get; set; }
        public bool IsGRNBased { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public int DetailsCount { get; set; }
        public int AttachmentsCount { get; set; }
    }

    // Details view model
    public class PaymentRequestDetailsViewModel
    {
        public int Id { get; set; }
        public int GRNId { get; set; }
        public string PRQNumber { get; set; } = string.Empty;
        public DateTime RequiredDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PayeeName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public short StateId { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public int CurrentApprovalSequence { get; set; } = 0;

        public DateTime CreatedOn { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
        public string? Remarks { get; set; }
        public bool SelfApplicant { get; set; }
        public string? PIVNo { get; set; }
        public string? CSNo { get; set; }

        // --- Added properties for descriptive names ---
        public short? PaymentModeId { get; set; }
        public string? PaymentModeName { get; set; } // New property
        public short? PaymentTypeId { get; set; }
        public string? PaymentTypeName { get; set; } // New property
        public short? PaymentNatureId { get; set; }
        public string? PaymentNatureName { get; set; } // New property
        public short? PaymentSubNatureId { get; set; }
        public string? PaymentSubNatureName { get; set; } // New property
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; } // New property
        public string? DepartmentCode { get; set; }
        public string? DepartmentName { get; set; } // New property
                                                    // ---------------------------------------------

        public short? CurrencyId { get; set; }

        // GRN information (if applicable)
        public bool IsGRNBased { get; set; }
        public string? GRNNumber { get; set; }
        public DateTime? GRNReceiptDate { get; set; }
        public string? PONumber { get; set; }
        public DateTime? PODate { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierCode { get; set; }
        public decimal? GRNTotalAmount { get; set; }
        public decimal? RemainingGRNAmount { get; set; }

        [Display(Name = "Attactment Type")]
        public short AttachmentTypeId { get; set; }

        // Details and related data
        public List<PaymentRequestDetailViewModel> Details { get; set; } = new List<PaymentRequestDetailViewModel>();
        public List<PaymentRequestCostAllocationViewModel> CostAllocations { get; set; } = new List<PaymentRequestCostAllocationViewModel>();
        public List<PaymentRequestAttachmentViewModel> Attachments { get; set; } = new List<PaymentRequestAttachmentViewModel>();
        public List<WorkflowHistoryViewModel> WorkflowHistory { get; set; } = new List<WorkflowHistoryViewModel>();

        public SelectList? AttachmentTypes { get; set; }
        public List<SelectListItem> Suppliers { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Branches { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentModes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentNatures { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> SubNatures { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();

        // Helper properties
        public bool SupportsDetails { get; set; }
        public bool SupportsCostAllocations { get; set; }
        public bool IsJobRelatedPayment { get; set; }
        public bool IsOtherPayment { get; set; }
    }

    public class PaymentRequestDetailViewModel
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; } = DateTime.Now;
        public string? Description { get; set; }
        public decimal AmountExTax { get; set; }
        public decimal STRate { get; set; }
        public decimal OtherTax { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PaymentRequestCostAllocationViewModel
    {
        public int Id { get; set; }
        public short? BranchId { get; set; }

        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public short? DepartmentId { get; set; }

        public string? DepartmentCode { get; set; }
        public string? DepartmentName { get; set; }
        public decimal? Rate { get; set; }
    }

    public class PaymentRequestAttachmentViewModel
    {
        public int Id { get; set; }
        public IFormFile File { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? FileContentType { get; set; }
        public decimal FileSizeKB { get; set; }
        public short AttachmentTypeId { get; set; }
        public string AttachmentTypeName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public bool Delete { get; set; }
    }

    public class WorkflowHistoryViewModel
    {
        public string ActionName { get; set; } = string.Empty;
        public string FromStateName { get; set; } = string.Empty;
        public string ToStateName { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public string ActionByUserName { get; set; } = string.Empty;
        public string? ToUserName { get; set; }
        public DateTime ActionDate { get; set; }
    }

    // Create from GRN selection view model
    public class CreatePaymentRequestFromGRNViewModel
    {
        public int Id { get; set; }
        public List<EligibleGRNViewModel> EligibleGRNs { get; set; } = new List<EligibleGRNViewModel>();
    }

    public class EligibleGRNViewModel
    {
        public int Id { get; set; }
        public int POId { get; set; }
        public string GRNNumber { get; set; } = string.Empty;
        public DateTime ReceiptDate { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public DateTime PODate { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public int ItemsCount { get; set; }
    }

    // Create GRN-based payment request view model
    public class CreateGRNBasedPaymentRequestViewModel
    {
        public int Id { get; set; }
        public int GoodsReceiptNoteId { get; set; }

        // GRN Information (read-only display)
        public string GRNNumber { get; set; } = string.Empty;
        public DateTime GRNReceiptDate { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public decimal GRNTotalAmount { get; set; }

        public decimal GRNReceivedQuantity { get; set; }


        // Payment Request fields
        [Required]
        [Display(Name = "Required Date")]
        public DateTime RequiredDate { get; set; } = DateTime.UtcNow.AddDays(7);

        [Display(Name = "Payee Name")]
        [StringLength(200)]
        public string? PayeeName { get; set; }

        [Display(Name = "Supplier")]
        public short SupplierId { get; set; }



        [Display(Name = "Payment Mode")]
        public short PaymentModeId { get; set; } = 2;

        [Display(Name = "Payment Type")]
        public short? PaymentTypeId { get; set; }

        [Display(Name = "Payment Nature")]
        public short PaymentNatureId { get; set; } = 1;

        [Display(Name = "Payment Sub Nature")]
        public short? PaymentSubNatureId { get; set; }

        [Display(Name = "Currency")]
        public short CurrencyId { get; set; } = 1;

        [Display(Name = "Self Applicant")]
        public bool SelfApplicant { get; set; } = true;

        [Display(Name = "Remarks")]
        [StringLength(1000)]
        public string? Remarks { get; set; }

        [Display(Name = "Request Nature")]
        public short? RequestNatureId { get; set; }

        [Display(Name = "Request Type")]
        public short? RequestTypeId { get; set; }


        [Display(Name = "BranchId")]
        public short? BranchId { get; set; }

        [Display(Name = "DepartmentId")]
        public short? DepartmentId { get; set; }

        [Display(Name = "Branch Code")]
        [StringLength(50)]
        public string? BranchCode { get; set; }

        [Display(Name = "Department Code")]
        [StringLength(50)]
        public string? DepartmentCode { get; set; }

        [Display(Name = "Company Code")]
        [StringLength(50)]
        public string CompanyCode { get; set; } = string.Empty;

        [Display(Name = "Attactment Type")]
        public short AttachmentTypeId { get; set; }
        public List<IFormFile>? Attachments { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;

        // Computed properties
        public List<CreatePaymentRequestDetailViewModel> Details { get; set; } = new List<CreatePaymentRequestDetailViewModel>();
        public List<CreatePaymentRequestCostAllocationViewModel> CostAllocations { get; set; } = new List<CreatePaymentRequestCostAllocationViewModel>();

        public List<SelectListItem> AttachmentTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Suppliers { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Branches { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentModes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentNatures { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> SubNatures { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Currencies { get; set; } = new List<SelectListItem>();
        public decimal TotalAmount => Details?.Sum(d => d.TotalAmount) ?? 0;
    }

    public class EditGRNBasedPaymentRequestViewModel
    {
        // Primary key for the Payment Request being edited
        public int Id { get; set; }

        // GRN Information (Read-only)
        public int? GoodsReceiptNoteId { get; set; }
        public string? GRNNumber { get; set; }
        public DateTime GRNReceiptDate { get; set; }
        public string? PONumber { get; set; }
        public string? SupplierName { get; set; }
        public decimal GRNTotalAmount { get; set; }

        // Payment Request Details (Editable)
        [Required(ErrorMessage = "Required Date is required.")]
        public DateTime RequiredDate { get; set; }

        [Required(ErrorMessage = "Payee Name is required.")]
        [StringLength(255)]
        public string? PayeeName { get; set; }

        public string? Remarks { get; set; }

        public bool SelfApplicant { get; set; }
        public string? PIVNo { get; set; }
        public string? CSNo { get; set; }

        // Dropdown/Lookup IDs (Editable)
        [Required]
        public short CurrencyId { get; set; } // Changed to short to match entity
        public int? SupplierId { get; set; }
        public short? PaymentModeId { get; set; } // Changed to short?
        public short? PaymentTypeId { get; set; }
        public short? PaymentNatureId { get; set; }
        public short? PaymentSubNatureId { get; set; }
        public string? CompanyCode { get; set; }
        public string? DepartmentCode { get; set; }
        public string? BranchCode { get; set; }

        public int CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? CurrentUserName { get; set; }

        // Collections for editing (details and allocations)
        public List<PaymentRequestDetailViewModel>? Details { get; set; }
        public List<PaymentRequestCostAllocationViewModel>? CostAllocations { get; set; }
        public List<PaymentRequestAttachmentViewModel> ExistingAttachments { get; set; } = new List<PaymentRequestAttachmentViewModel>();

        // This is for new attachments uploaded from the form
        public List<IFormFile>? NewAttachments { get; set; }

        // Additional fields for new attachments
        [Display(Name = "Attachment Type")]
        public short AttachmentTypeId { get; set; }

        // Dropdown properties (kept IEnumerable<SelectListItem> as they are correct)
        public IEnumerable<SelectListItem>? Suppliers { get; set; }
        public IEnumerable<SelectListItem>? Departments { get; set; }
        public IEnumerable<SelectListItem>? Branches { get; set; }
        public IEnumerable<SelectListItem>? PaymentModes { get; set; }
        public IEnumerable<SelectListItem>? PaymentTypes { get; set; }
        public IEnumerable<SelectListItem>? PaymentNatures { get; set; }
        public IEnumerable<SelectListItem>? SubNatures { get; set; }
        public IEnumerable<SelectListItem>? Companies { get; set; }
        public IEnumerable<SelectListItem>? Currencies { get; set; }
        public IEnumerable<SelectListItem>? AttachmentTypes { get; set; }
        public decimal TotalAmount => Details?.Sum(d => d.TotalAmount) ?? 0;
    }// Create direct payment request view model
    public class CreateDirectPaymentRequestViewModel
    {
        public short? GrnId { get; set; }

        [Required]
        [Display(Name = "Required Date")]
        public DateTime RequiredDate { get; set; } = DateTime.UtcNow.AddDays(7);

        [Display(Name = "Payee Name")]
        [StringLength(200)]
        public string PayeeName { get; set; } = string.Empty;

        [Display(Name = "Supplier")]
        public short SupplierId { get; set; }

        [Display(Name = "Payment Mode")]
        public short PaymentModeId { get; set; } = 2;

        [Display(Name = "Payment Type")]
        public short? PaymentTypeId { get; set; }

        [Required]
        [Display(Name = "Payment Nature")]
        public short PaymentNatureId { get; set; } = 1;

        [Display(Name = "Payment Sub Nature")]
        public short? PaymentSubNatureId { get; set; }

        [Display(Name = "Currency")]
        public short CurrencyId { get; set; } = 1;

        [Display(Name = "Self Applicant")]
        public bool SelfApplicant { get; set; } = true;

        [Display(Name = "Remarks")]
        [StringLength(1000)]
        public string? Remarks { get; set; }

        // Workflow properties
        [Display(Name = "Request Nature")]
        public short? RequestNatureId { get; set; }

        [Display(Name = "Request Type")]
        public short? RequestTypeId { get; set; }

        [Display(Name = "Attactment Type")]
        public short AttachmentTypeId { get; set; }

        [Display(Name = "DepartmentId")]
        public short? DepartmentId { get; set; }

        [Display(Name = "BranchId")]
        public short? BranchId { get; set; }

        [Display(Name = "Department Code")]
        [StringLength(50)]
        public string DepartmentCode { get; set; } = string.Empty;

        [Display(Name = "Branch Code")]
        [StringLength(50)]
        public string BranchCode { get; set; } = string.Empty;

        [Display(Name = "Company Code")]
        [StringLength(50)]
        public string CompanyCode { get; set; } = string.Empty;

        public List<IFormFile>? Attachments { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;

        // Details and allocations
        public List<CreatePaymentRequestDetailViewModel> Details { get; set; } = new List<CreatePaymentRequestDetailViewModel>();
        public List<CreatePaymentRequestCostAllocationViewModel> CostAllocations { get; set; } = new List<CreatePaymentRequestCostAllocationViewModel>();

        public List<SelectListItem> AttachmentTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Suppliers { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Branches { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentModes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentNatures { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> SubNatures { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Currencies { get; set; } = new List<SelectListItem>();
        // Helper properties
        public bool SupportsDetails { get; set; }
        public bool SupportsCostAllocations { get; set; }
        public bool IsJobRelatedPayment { get; set; }
        public bool IsOtherPayment { get; set; }

        // Computed property
        public decimal TotalAmount => Details?.Sum(d => d.TotalAmount) ?? 0;
    }

    public class EditDirectPaymentRequestViewModel
    {
        // Primary key for the Payment Request being edited
        public int Id { get; set; }

        // Payment Request Details (Editable)
        [Required(ErrorMessage = "Required Date is required.")]
        public DateTime RequiredDate { get; set; }

        [Required(ErrorMessage = "Payee Name is required.")]
        [StringLength(255)]
        public string? PayeeName { get; set; }

        public string? Remarks { get; set; }

        public bool SelfApplicant { get; set; }
        public string? PIVNo { get; set; }
        public string? CSNo { get; set; }

        // Dropdown/Lookup IDs (Editable)
        [Required]
        public short CurrencyId { get; set; } // Changed to short to match entity
        public int? SupplierId { get; set; }
        public short? PaymentModeId { get; set; } // Changed to short?
        public short? PaymentTypeId { get; set; }
        public short? PaymentNatureId { get; set; }
        public short? PaymentSubNatureId { get; set; }
        public string? CompanyCode { get; set; }
        public string? DepartmentCode { get; set; }
        public string? BranchCode { get; set; }

        public int CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? CurrentUserName { get; set; }

        // Collections for editing (details and allocations)
        public List<PaymentRequestDetailViewModel>? Details { get; set; }
        public List<PaymentRequestCostAllocationViewModel>? CostAllocations { get; set; }
        public List<PaymentRequestAttachmentViewModel> ExistingAttachments { get; set; } = new List<PaymentRequestAttachmentViewModel>();

        // Dropdown properties (kept IEnumerable<SelectListItem> as they are correct)
        public IEnumerable<SelectListItem>? Suppliers { get; set; }
        public IEnumerable<SelectListItem>? Departments { get; set; }
        public IEnumerable<SelectListItem>? Branches { get; set; }
        public IEnumerable<SelectListItem>? PaymentModes { get; set; }
        public IEnumerable<SelectListItem>? PaymentTypes { get; set; }
        public IEnumerable<SelectListItem>? PaymentNatures { get; set; }
        public IEnumerable<SelectListItem>? SubNatures { get; set; }
        public IEnumerable<SelectListItem>? Companies { get; set; }
        public IEnumerable<SelectListItem>? Currencies { get; set; }
        public IEnumerable<SelectListItem>? AttachmentTypes { get; set; }
        public decimal TotalAmount => Details?.Sum(d => d.TotalAmount) ?? 0;
    }
    public class CreatePaymentRequestDetailViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Invoice Number")]
        [Required(ErrorMessage = "Invoice Number is required.")]
        [StringLength(20, ErrorMessage = "Invoice Number cannot exceed 20 characters.")]
        public string? InvoiceNo { get; set; }

        [Display(Name = "Invoice Date")]
        public DateTime? InvoiceDate { get; set; }

        [Required]
        [Display(Name = "Description")]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Amount (Ex Tax)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal AmountExTax { get; set; } = 0;

        [Display(Name = "ST Rate %")]
        [Range(0, 100)]
        public decimal STRate { get; set; } = 0;

        [Display(Name = "Other Tax")]
        public decimal OtherTax { get; set; } = 0;


        // Computed property
        public decimal TotalAmount => AmountExTax + (AmountExTax * STRate / 100) + OtherTax;
    }

    public class CreatePaymentRequestCostAllocationViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Branch Code")]
        [StringLength(50)]
        public string? BranchCode { get; set; }

        [Display(Name = "Branch Name")]
        [StringLength(200)]
        public string? BranchName { get; set; }

        [Display(Name = "Department Code")]
        [StringLength(50)]
        public string? DepartmentCode { get; set; }

        [Display(Name = "Department Name")]
        [StringLength(200)]
        public string? DepartmentName { get; set; }

        [Display(Name = "Rate %")]
        [Range(0, 100, ErrorMessage = "Rate must be between 0.01 and 100")]
        public decimal? Rate { get; set; } = 0;

    }

    // Payment request type selection view model
    public class PaymentRequestTypeSelectionViewModel
    {
        public string SelectedType { get; set; } = string.Empty; // "GRN" or "Direct"
    }

    public class UploadPaymentRequestAttachmentViewModel
    {
        public int PaymentRequestId { get; set; }

        [Required(ErrorMessage = "Please select an attachment type.")]
        [Display(Name = "Attachment Type")]
        public short AttachmentTypeId { get; set; }

        [Required(ErrorMessage = "Please select a file to upload.")]
        [Display(Name = "File")]
        public IFormFile File { get; set; } = null!;

        public List<SelectListItem> AttachmentTypes { get; set; } = new List<SelectListItem>();
    }

    public class PaymentRequestAttachmentListItemViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? FileContentType { get; set; }
        public decimal FileSizeKB { get; set; }
        public short AttachmentTypeId { get; set; }
        public string AttachmentTypeName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
    }

    public class UpdatePIVNoViewModel
    {
        [Required(ErrorMessage = "PIV Number is required.")]
        // Updated RegularExpression to match a pattern like 'PK' followed by 9 digits
        [RegularExpression(@"^PK\d{9}$", ErrorMessage = "PIV Number must start with 'PK' followed by exactly 9 digits.")]
        [Display(Name = "PIV Number")]
        public string PIVNo { get; set; } = string.Empty;

        public int id { get; set; }
    }

    public class UpdateCSNoViewModel
    {
        [Required(ErrorMessage = "CS Number is required.")]
        // Updated RegularExpression to match a pattern like 'PK' followed by 9 digits
        [RegularExpression(@"^PK\d{9}$", ErrorMessage = "CS Number must start with 'PK' followed by exactly 9 digits.")]
        [Display(Name = "CS Number")]
        public string CSNo { get; set; } = string.Empty;

        public int id { get; set; }
    }
}