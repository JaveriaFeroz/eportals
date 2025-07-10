using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("WHTaxExemptions")]
    public class WHTaxExemption : AuditableEntity
    {
        [Key]
        public short ExemptionId { get; set; }

        [Required(ErrorMessage = "Date From is required")]
        [Display(Name = "Date From")]
        [Column(TypeName = "datetime")]
        public DateTime DateFrom { get; set; }

        [Required(ErrorMessage = "Date To is required")]
        [Display(Name = "Date To")]
        [Column(TypeName = "datetime")]
        public DateTime DateTo { get; set; }

        [Required]
        public short CompanyId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // FIX: Correctly defined the navigation property for Company.
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
    }
}