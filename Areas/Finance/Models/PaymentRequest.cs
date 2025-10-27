using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.Common.Services;
using ProcureToPay.Areas.Finance.Models;
using ProcureToPay.Areas.Receiving.Models;
using ProcureToPay.Areas.UserManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Finance.Models
{
    [Table("PaymentRequests")]
    public class PaymentRequest : AuditableEntity, IWorkflowEntity
    {
        public int Id { get; set; }

        [StringLength(50)]
        [Display(Name = "PRQ Number")]
        public string? PRQNumber { get; set; } // Payment Request Number, will be generated (PRQ20250719001)

        [Required]
        [Display(Name = "Required Date")]
        public DateTime RequiredDate { get; set; } = DateTime.UtcNow.AddDays(7);


        // GRN Reference (optional - for purchase-related payments)
        [Display(Name = "GRN Number")]
        public int? GoodsReceiptNoteId { get; set; }
        [ForeignKey("GoodsReceiptNoteId")]
        public virtual GoodsReceiptNote? GoodsReceiptNote { get; set; }

        // Supplier Information
        [Display(Name = "Supplier")]
        public int? SupplierId { get; set; }
        // Note: Add Supplier foreign key relationship based on your Supplier model

        // Payee Information
        [StringLength(200)]
        [Display(Name = "Payee Name")]
        public string? PayeeName { get; set; }

        // Payment Details
        [Display(Name = "Payment Mode")]
        public short? PaymentModeId { get; set; } = 2; // Default from legacy
                                                       // Note: Add PaymentMode foreign key relationship

        [Display(Name = "Payment Type")]
        public short? PaymentTypeId { get; set; }
        // Note: Add PaymentType foreign key relationship

        [Display(Name = "Payment Nature")]
        public short? PaymentNatureId { get; set; } = 1;
        // Note: Add PaymentNature foreign key relationship

        [Display(Name = "Payment Sub Nature")]
        public short? PaymentSubNatureId { get; set; }
        // Note: Add PaymentSubNature foreign key relationship

        // Currency
        [Display(Name = "Currency")]
        public short? CurrencyId { get; set; } = 1; // Default to PKR
                                                    // Note: Add Currency foreign key relationship

        // Additional Fields from Legacy Code
        [Display(Name = "Self Applicant")]
        public bool? SelfApplicant { get; set; } = true;

        [StringLength(50)]
        [Display(Name = "PIV Number")]
        public string? PIVNo { get; set; }

        [StringLength(50)]
        [Display(Name = "CS Number")]
        public string? CSNo { get; set; }


        [Required]
        [Display(Name = "Created By")]
        public int CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; } = null!;

        // Navigation Properties for Details and Related Entities
        public virtual ICollection<PaymentRequestDetail> Details { get; set; } = new List<PaymentRequestDetail>();
        public virtual ICollection<PaymentRequestAttachment> Attachments { get; set; } = new List<PaymentRequestAttachment>();
        public virtual ICollection<PaymentRequestCostAllocation> CostAllocations { get; set; } = new List<PaymentRequestCostAllocation>();

        // IWorkflowEntity Implementations
        public short WorkFlowTypeId { get; set; } = (short)ProcureToPay.Areas.Common.Enums.WorkFlowType.PaymentRequest;
        public int FormId => Id;
        public short StateId { get; set; } = WorkflowService.STATE_SAVED;
        public string Owner { get; set; } = string.Empty;

        // Workflow-related properties from GRN or direct assignment
        public short? RequestNatureId { get; set; }
        public short? RequestTypeId { get; set; }

        // DEPARTMENT CONTEXT (from user's department)
        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        // BRANCH CONTEXT (from user's branch)
        [Display(Name = "Branch")]
        public int? BranchId { get; set; }

        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string? CompanyCode { get; set; } = string.Empty;
        public int CurrentApprovalSequence { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
        public bool Approved { get; set; } = false;
        public bool Rejected { get; set; } = false;


        // Computed Properties (similar to legacy StateName)
        [NotMapped]
        public string StateName
        {
            get { return ((ProcureToPay.Areas.Common.Enums.PaymentRequestState)StateId).ToString(); }
        }

        // For attachment upload functionality
        [NotMapped]
        public bool AddNew { get; set; }
        [NotMapped]
        public short? AttachmentTypeId { get; set; }

        // Computed property for total amount
        [NotMapped]
        public decimal TotalAmount => Details?.Sum(d => d.TotalAmount) ?? 0;

        public void SyncStatusWithState()
        {
            // Custom logic for syncing status with workflow state
            // This can be implemented based on your specific workflow requirements
        }

        // Helper method to determine if payment request supports details based on nature/sub-nature
        [NotMapped]
        public bool SupportsDetails
        {
            get
            {
                if (PaymentNatureId == 1)
                {
                    return PaymentSubNatureId.HasValue && new[] { 1, 3, 6 }.Contains(PaymentSubNatureId.Value);
                }
                return PaymentNatureId == 99 || (PaymentSubNatureId.HasValue && new[] { 2, 4, 5, 7 }.Contains(PaymentSubNatureId.Value));
            }
        }

        // Helper method to determine if payment request supports cost allocations
        [NotMapped]
        public bool SupportsCostAllocations
        {
            get
            {
                return PaymentNatureId == 99 || PaymentSubNatureId == 2;
            }
        }

        // Helper method to determine payment type based on nature
        [NotMapped]
        public bool IsJobRelatedPayment
        {
            get
            {
                return PaymentNatureId == 1 && PaymentSubNatureId.HasValue && new[] { 1, 3, 6 }.Contains(PaymentSubNatureId.Value);
            }
        }

        [NotMapped]
        public bool IsOtherPayment
        {
            get
            {
                return PaymentNatureId == 99 || (PaymentSubNatureId.HasValue && new[] { 2, 4, 5, 7 }.Contains(PaymentSubNatureId.Value));
            }
        }
    }
}
