using ProcureToPay.Areas.UserManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Procurement.Models
{
    public class PurchaseRequest
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

        // USER INTEGRATION - Replace string Requester with User reference
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