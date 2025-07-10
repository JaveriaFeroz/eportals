using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("RateTypes")]
    public class RateType : AuditableEntity
    {
        [Key]
        public short RateTypeId { get; set; } // Changed from TypeId to match Client model

        [Required(ErrorMessage = "Rate Type Name is required")]
        [Display(Name = "Rate Type Name")]
        [StringLength(100)]
        public string RateTypeName { get; set; } // Changed from TypeName to match Client model

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}