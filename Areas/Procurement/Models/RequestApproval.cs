using ProcureToPay.Areas.UserManagement.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Procurement.Models
{
    public class RequestApproval
    {
        public int Id { get; set; }

        [Required]
        public int PurchaseRequestId { get; set; }

        [ForeignKey("PurchaseRequestId")]
        public virtual PurchaseRequest PurchaseRequest { get; set; }

        [Required]
        public int ApprovalLevelId { get; set; }

        [ForeignKey("ApprovalLevelId")]
        public virtual ApprovalLevel ApprovalLevel { get; set; }

        [Required]
        public int ApproverId { get; set; }

        [ForeignKey("ApproverId")]
        public virtual User Approver { get; set; }

        [Required]
        public ApprovalStatus Status { get; set; }

        [StringLength(1000)]
        public string? Comments { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ActionDate { get; set; }

        public int Order { get; set; } // Order in approval chain
    }
}
