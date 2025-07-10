using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Activities")]
    public class Activity : AuditableEntity
    {
        [Key]
        public short ActivityId { get; set; }

        [Required(ErrorMessage = "Activity Name is required")]
        [Display(Name = "Activity Name")]
        [StringLength(100, ErrorMessage = "Activity Name cannot exceed 100 characters")]
        public string ActivityName { get; set; }

        [Required(ErrorMessage = "Estimated Hours Required is required")]
        [Display(Name = "Estimated Hours Required")]
        [Range(0.1, 999.9, ErrorMessage = "Estimated Hours must be between 0.1 and 999.9")]
        public double EstHrsReq { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
