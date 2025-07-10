using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("VehicleGroup")]
    public class VehicleGroup : AuditableEntity
    {
        [Key]
        public short GroupId { get; set; }

        [Required(ErrorMessage = "Group Name is required")]
        [Display(Name = "Group Name")]
        [StringLength(100)]
        public string GroupName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}