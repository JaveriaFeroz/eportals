using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Procurement.Models
{
    public class ApprovalLevel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } // "Department Manager", "Finance Manager", "General Manager"

        [StringLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string RequiredRole { get; set; } // Maps to your Role.Name

        public int Order { get; set; } // 1, 2, 3... for sequential approvals

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<RequestApproval> RequestApprovals { get; set; } = new List<RequestApproval>();
    }
  

    public enum ApprovalStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Delegated = 3
    }
}
