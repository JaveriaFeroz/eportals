using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("AccessorialCharges")]
    public class AccessorialCharge : AuditableEntity
    {
        [Key]
        public short ChargeId { get; set; }

        [Required(ErrorMessage = "Charge Name is required")]
        [Display(Name = "Charge Name")]
        [StringLength(100)]
        public string ChargeName { get; set; }

        [Required(ErrorMessage = "Charge Code is required")]
        [Display(Name = "Charge Code")]
        [StringLength(20)]
        public string ChargeCode { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}