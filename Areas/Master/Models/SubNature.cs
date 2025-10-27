using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProcureToPay.Areas.Master.Models
{
    [Table("SubNatures")]
    public class SubNature : AuditableEntity
    {
        [Key]
        public short SubNatureId { get; set; }

        [Required(ErrorMessage = "SubNature Name is required")]
        [Display(Name = "SubNature Name")]
        [StringLength(100)]
        public string SubNatureName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}