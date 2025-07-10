using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Priorities")]
    public class Priority : AuditableEntity
    {
        [Key]
        public short PriorityId { get; set; }

        [Required(ErrorMessage = "Priority Name is required")]
        [Display(Name = "Priority Name")]
        [StringLength(100)]
        public string PriorityName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
