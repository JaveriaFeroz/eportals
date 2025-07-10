using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Charges")]
    public class Charge : AuditableEntity
    {
        [Key]
        public short ChargeId { get; set; }

        [Required(ErrorMessage = "Charge Name is required")]
        [Display(Name = "Charge Name")]
        [StringLength(100)]
        public string ChargeName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
