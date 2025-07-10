using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Detentions")]
    public class Detention : AuditableEntity
    {
        [Key]
        public short DetentionId { get; set; }

        [Required(ErrorMessage = "Detention Name is required")]
        [Display(Name = "Detention Name")]
        [StringLength(100)]
        public string DetentionName { get; set; }

        [Display(Name = "Hours Threshold")]
        [Range(0, short.MaxValue, ErrorMessage = "Hours Threshold must be a positive number")]
        public short? HRsThreshold { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}