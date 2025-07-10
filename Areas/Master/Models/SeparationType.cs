using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("SeparationTypes")]
    public class SeparationType : AuditableEntity
    {
        [Key]
        public short TypeId { get; set; }

        [Required(ErrorMessage = "Separation Type Name is required")]
        [Display(Name = "SeparationType Name")]
        [StringLength(100)]
        public string TypeName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}