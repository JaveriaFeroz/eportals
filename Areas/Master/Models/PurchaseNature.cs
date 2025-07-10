using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("PurchaseNatures")]
    public class PurchaseNature : AuditableEntity
    {
        [Key]
        public short PurchaseNatureId { get; set; }

        [Required(ErrorMessage = "PurchaseNature Name is required")]
        [Display(Name = "PurchaseNature Name")]
        [StringLength(100)]
        public string PurchaseNatureName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}