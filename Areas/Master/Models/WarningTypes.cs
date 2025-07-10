using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("WarningTypes")]
    public class WarningType : AuditableEntity
    {
        [Key]
        public short TypeId { get; set; }

        [Required(ErrorMessage = "Type Name is required")]
        [Display(Name = "Type Name")]
        [StringLength(100)]
        public string TypeName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}