using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{

    public class SKUType : AuditableEntity
    {
        [Key]
        public short TypeId { get; set; }

        [Required(ErrorMessage = "Type Name is required")]
        [Display(Name = "Type Name")]
        [StringLength(100)]
        public string TypeName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<SKU> SKUs { get; set; } = new List<SKU>();
    }

}