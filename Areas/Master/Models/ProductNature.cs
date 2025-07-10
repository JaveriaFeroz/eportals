using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("ProductNatures")]
    public class ProductNature : AuditableEntity
    {
        [Key]
        public short NatureId { get; set; }

        [Required(ErrorMessage = "Nature Name is required")]
        [Display(Name = "Nature Name")]
        [StringLength(100)]
        public string NatureName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Required]
        public short PurchaseNatureId { get; set; }

        [ForeignKey("PurchaseNatureId")]
        public virtual PurchaseNature PurchaseNature { get; set; }

    }
}