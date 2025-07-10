using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("SupplierTypes")]
    public class SupplierType : AuditableEntity
    {
        [Key]
        public short TypeId { get; set; }

        [Required(ErrorMessage = "Supplier Type Name is required")]
        [Display(Name = "Supplier Type Name")]
        [StringLength(100)]
        public string SupplierTypeName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
