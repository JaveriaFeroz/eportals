using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("ServiceNatures")]
    public class ServiceNature : AuditableEntity
    {
        [Key]
        public short NatureId { get; set; }

        [Required(ErrorMessage = "Nature Name is required")]
        [Display(Name = "Nature Name")]
        [StringLength(100)]
        public string NatureName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Opex")]
        public bool IsOpex { get; set; }

        [Display(Name = "Capex")]
        public bool IsCapex { get; set; }

    }
}