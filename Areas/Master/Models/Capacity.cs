using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Capacities")]
    public class Capacity : AuditableEntity
    {
        [Key]
        public short CapacityId { get; set; }

        [Required(ErrorMessage = "Capacity Name is required")]
        [Display(Name = "Capacity Name")]
        [StringLength(100)]
        public string CapacityName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
