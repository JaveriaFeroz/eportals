using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Procurement.Enums; // Required for GetDisplayName extension method
using ProcureToPay.Areas.UserManagement.Models;
using System.Collections.Generic; // Required for IValidatableObject
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace ProcureToPay.Areas.Procurement.Models
{
    [Table("PurchaseRequests")]
    public class PurchaseRequest : AuditableEntity, IWorkflowEntity, IValidatableObject // Implemented IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "PR No.")] // Re-adding Display for clarity in model, even if ViewModel handles UI
        [StringLength(50)]
        public string? RequestNumber { get; set; } // Made optional - will be generated

        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Justification is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Purchase Nature Type is required.")] // New property validation
        [Display(Name = "Purchase Nature Type")]
        public PurchaseNatureType PurchaseNatureType { get; set; } // Opex or Capex

        [Required(ErrorMessage = "Purchase Item Type is required.")] // New property validation
        [Display(Name = "Purchase Item Type")]
        public PurchaseItemType PurchaseItemType { get; set; } // Goods or Service

        [Display(Name = "Product Nature")]
        public short? ProductNatureId { get; set; } // Nullable

        [ForeignKey("ProductNatureId")]
        public virtual ProductNature? ProducNature { get; set; }

        [Display(Name = "Service Nature")]
        public short? ServiceNatureId { get; set; } // Nullable

        [ForeignKey("ServiceNatureId")]
        public virtual ServiceNature? ServiceNature { get; set; }

        [Required(ErrorMessage = "Request Date is required")] // Re-adding validation
        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; }

        [Display(Name = "Required Date")]
        [Required(ErrorMessage = "Required Date is required")] // Re-adding validation
        public DateTime? RequiredDate { get; set; }

        [Required(ErrorMessage = "Requested By User is required")] // Re-adding validation
        public int RequestedByUserId { get; set; }

        [ForeignKey("RequestedByUserId")]
        public virtual User? RequestedByUser { get; set; }

        // APPROVAL WORKFLOW INTEGRATION
        public int? ApprovedByUserId { get; set; }

        [ForeignKey("ApprovedByUserId")]
        public virtual User? ApprovedByUser { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [StringLength(1000)]
        public string? ApprovalComments { get; set; }

        // DEPARTMENT CONTEXT (from user's department)
        [Display(Name = "Department")]
        [Required(ErrorMessage = "Department is required")] // Re-adding validation
        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        // BRANCH CONTEXT (from user's branch)
        [Display(Name = "Branch")]
        [Required(ErrorMessage = "Branch is required")] // Re-adding validation
        public int? BranchId { get; set; }

        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        [Required]
        public RequestStatus Status { get; set; }



        // Navigation properties
        // Add this property
        public virtual ICollection<BidEvaluation> BidEvaluations { get; set; } = new List<BidEvaluation>();
        public virtual ICollection<PurchaseRequestItem> Items { get; set; } = new List<PurchaseRequestItem>();
        public virtual ICollection<RequestApproval> Approvals { get; set; } = new List<RequestApproval>();
        public virtual ICollection<PurchaseRequestOrderMapping> PurchaseRequestMappings { get; set; } = new List<PurchaseRequestOrderMapping>();

        public virtual ICollection<PurchaseRequestAttachment> Attachments { get; set; } = new List<PurchaseRequestAttachment>();
        // --- IWorkflowEntity Implementations and additional workflow properties ---

        public short WorkFlowTypeId { get; set; } = (short)ProcureToPay.Areas.Common.Enums.WorkFlowType.PurchaseRequest;

        // Map FormId to Id
        public int FormId => Id;

        public short StateId { get; set; } = 1; // Default to STATE_SAVED

        [Required(ErrorMessage = "Owner is required")] // Re-adding validation
        [StringLength(100)]
        public string Owner { get; set; } = string.Empty;

        public short? RequestNatureId { get; set; }
        public short? RequestTypeId { get; set; }

        // These should be populated based on user's department/branch
        [StringLength(50)]
        public string? CompanyCode { get; set; } = string.Empty;
        public string DepartmentCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;


        // Workflow-specific properties
        public int CurrentApprovalSequence { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
        public bool Approved { get; set; } = false;
        public bool Rejected { get; set; } = false;

        // For backward compatibility with your service
        public int PRNo => Id;

        // Helper method to sync Status with StateId
        public void SyncStatusWithState()
        {
            Status = StateId switch
            {
                1 => RequestStatus.Draft,           // STATE_SAVED
                2 => RequestStatus.Submitted,       // STATE_SUBMITTED
                3 => RequestStatus.Approved,        // STATE_APPROVED
                4 => RequestStatus.Rejected,        // STATE_REJECTED
                5 => RequestStatus.Cancelled,       // STATE_RETURNED
                10001 => RequestStatus.Cancelled,   // STATE_CANCELLED
                _ => RequestStatus.Draft
            };
        }

        // Custom validation method using IValidatableObject
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PurchaseItemType == PurchaseItemType.Goods && (ProductNatureId == null || ProductNatureId <= 0))
            {
                yield return new ValidationResult(
                    "Product Nature is required when Purchase Item Type is Goods.",
                    new[] { nameof(ProductNatureId) }
                );
            }
            else if (PurchaseItemType == PurchaseItemType.Service && (ServiceNatureId == null || ServiceNatureId <= 0))
            {
                yield return new ValidationResult(
                    "Service Nature is required when Purchase Item Type is Service.",
                    new[] { nameof(ServiceNatureId) }
                );
            }
        }
    }
}
