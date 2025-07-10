using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Trailers")]
    public class Trailer : AuditableEntity
    {
        [Key]
        public short TrailerId { get; set; }

        [Required(ErrorMessage = "Trailer Name is required")]
        [Display(Name = "Trailer Name")]
        [StringLength(100)]
        public string TrailerName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}