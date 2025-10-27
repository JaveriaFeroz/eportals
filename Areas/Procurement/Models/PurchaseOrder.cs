using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.Common.Services;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Procurement.Enums;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Procurement.Models
{
    [Table("PurchaseOrders")]
    public class PurchaseOrder : AuditableEntity, IWorkflowEntity
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string? PONumber { get; set; } // Will be generated (PO20250718001)

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "PO Date")]
        public DateTime PODate { get; set; } = DateTimeHelper.GetPakistanStandardTime(); // Consistent with UtcNow

        [Display(Name = "Expected Delivery Date")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        // CreatedByUserId and CreatedByUser are inherited from AuditableEntity

        // Vendor Information
        [Required]
        [Display(Name = "Vendor")]
        public short VendorId { get; set; } // This is the foreign key to Supplier

        [ForeignKey("VendorId")]
        public virtual Supplier? Supplier { get; set; } // Navigation property to Supplier

        // Added missing property and updated foreign key mapping
        [Display(Name = "Purchase Request")]
        public int? PurchaseRequestId { get; set; }

        [ForeignKey("PurchaseRequestId")]
        public virtual PurchaseRequest? PurchaseRequest { get; set; }

        // Changed 'VendorId' to 'SupplierId' to match the view.
        [Required]
        [Display(Name = "Supplier")]
        public short SupplierId { get; set; }


        [StringLength(100)]
        public string? SupplierAddress { get; set; }

        [StringLength(50)]
        public string? SupplierContact { get; set; }

        // Purchase Nature & Type
        [Required]
        [Display(Name = "Purchase Nature")]
        public PurchaseNatureType PurchaseNature { get; set; }

        [Required]
        [Display(Name = "Purchase Type")]
        public PurchaseItemType PurchaseType { get; set; }

        // Department & Branch Context (inherits from PR or user)
        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [Display(Name = "Branch")]
        public int? BranchId { get; set; }

        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tax Amount")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Grand Total")]
        public decimal GrandTotal { get; set; }

        [Required]
        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

        [StringLength(500)]
        public string? Terms { get; set; }

        public short? PaymentDays { get; set; }

        // ADDED: The missing 'Remarks' property.
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        // Navigation Properties

        // Bid properties
        [Display(Name = "Bid Evaluation")]
        public int? BidEvaluationId { get; set; }

        [ForeignKey("BidEvaluationId")]
        public virtual BidEvaluation? BidEvaluation { get; set; }

        [Display(Name = "Winning Bid")]
        public int? WinningBidId { get; set; }

        [ForeignKey("WinningBidId")]
        public virtual Bid? WinningBid { get; set; }
        public virtual ICollection<FormHistory> FormHistory { get; set; } = new List<FormHistory>();
        public virtual List<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
        public virtual ICollection<PurchaseRequestOrderMapping> PurchaseRequestMappings { get; set; } = new List<PurchaseRequestOrderMapping>();

        // --- IWorkflowEntity Implementation ---
        public short WorkFlowTypeId { get; set; } = (short)ProcureToPay.Areas.Common.Enums.WorkFlowType.PurchaseOrder;
        public int FormId => Id;
        public short StateId { get; set; } = 1; // Default to STATE_SAVED
        public string Owner { get; set; } = string.Empty;
        public short? RequestNatureId { get; set; }
        public short? RequestTypeId { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string? CompanyCode { get; set; } = string.Empty;

        // Workflow-specific properties
        public int CurrentApprovalSequence { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
        public bool Approved { get; set; } = false;
        public bool Rejected { get; set; } = false;

        // Helper methods
        public void SyncStatusWithState()
        {
            Status = StateId switch
            {
                1 => PurchaseOrderStatus.Draft,
                2 => PurchaseOrderStatus.Submitted,
                3 => PurchaseOrderStatus.Approved,
                4 => PurchaseOrderStatus.Rejected,
                5 => PurchaseOrderStatus.Cancelled, // STATE_RETURNED
                WorkflowService.STATE_ISSUED => PurchaseOrderStatus.Issued,
                WorkflowService.STATE_COMPLETED => PurchaseOrderStatus.Closed,
                WorkflowService.STATE_CANCELLED => PurchaseOrderStatus.Cancelled,
                _ => PurchaseOrderStatus.Draft
            };
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Title) &&
                   CreatedByUserId > 0 &&
                   VendorId > 0 &&
                   PODate != default(DateTime) &&
                   Items.Any();
        }

    }
}