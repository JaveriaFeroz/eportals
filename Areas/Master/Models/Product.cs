using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Products")]
    public class Product : AuditableEntity
    {
        [Key]
        public short ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [Display(Name = "Product Name")]
        [StringLength(200)]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Purchase Price is required")]
        [Display(Name = "Purchase Price")]
        public double UnitPrice { get; set; }

        [Display(Name = "UOM")]
        public short? UoMId { get; set; }

        [Display(Name = "Product Nature")]
        public short? ProductNatureId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [ForeignKey("UoMId")]
        public virtual UoM UoM { get; set; }

        [ForeignKey("ProductNatureId")]
        public virtual ProductNature ProductNature { get; set; }
    }
}