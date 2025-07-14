using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.UserManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Procurement.Models
{
    public class PurchaseRequest : AuditableEntity, IWorkflowEntity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string RequestNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        public DateTime? RequiredDate { get; set; }

        [Required]
        public int RequestedByUserId { get; set; }

        [ForeignKey("RequestedByUserId")]
        public virtual User RequestedByUser { get; set; }

        // APPROVAL WORKFLOW INTEGRATION
        public int? ApprovedByUserId { get; set; }

        [ForeignKey("ApprovedByUserId")]
        public virtual User ApprovedByUser { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [StringLength(1000)]
        public string? ApprovalComments { get; set; }

        // DEPARTMENT CONTEXT (from user's department)
        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        // BRANCH CONTEXT (from user's branch)  
        public int? BranchId { get; set; }

        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        [StringLength(500)]
        public string Purpose { get; set; }

        [Required]
        public RequestStatus Status { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<PurchaseRequestItem> Items { get; set; } = new List<PurchaseRequestItem>();
        public virtual ICollection<RequestApproval> Approvals { get; set; } = new List<RequestApproval>();

        // --- IWorkflowEntity Implementations and additional workflow properties ---

        public short WorkFlowTypeId { get; set; } = (short)ProcureToPay.Areas.Common.Enums.WorkFlowType.PurchaseRequest;

        // Map FormId to Id 
        public int FormId => Id;

        public short StateId { get; set; } = 1; // Default to STATE_SAVED

        public string Owner { get; set; } = string.Empty;

        public short? RequestNatureId { get; set; }
        public short? RequestTypeId { get; set; }

        // These should be populated based on user's department/branch
        public string DepartmentCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;

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
    }

    public enum RequestStatus
    {
        Draft = 0,
        Submitted = 1,
        UnderReview = 2,
        Approved = 3,
        Rejected = 4,
        Cancelled = 5
    }
}