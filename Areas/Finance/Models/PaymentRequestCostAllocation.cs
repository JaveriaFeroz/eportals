using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Finance.Models
{
    [Table("PaymentRequestCostAllocations")]
    public class PaymentRequestCostAllocation : AuditableEntity
    {
        public int Id { get; set; }
        public int PaymentRequestId { get; set; }
        [ForeignKey("PaymentRequestId")]
        public virtual PaymentRequest PaymentRequest { get; set; } = null!;

        [StringLength(50)]
        [Display(Name = "Branch Code")]
        public string? BranchCode { get; set; }

        [StringLength(200)]
        [Display(Name = "Branch Name")]
        public string? BranchName { get; set; }

        [StringLength(50)]
        [Display(Name = "Department Code")]
        public string? DepartmentCode { get; set; }

        [StringLength(200)]
        [Display(Name = "Department Name")]
        public string? DepartmentName { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Rate %")]
        [Range(0, 100, ErrorMessage = "Rate must be between 0 and 100")]
        public decimal? Rate { get; set; }
        
    }
}
