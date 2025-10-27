using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProcureToPay.Areas.Master.Models
{
    [Table("PaymentNatures")]
    public class PaymentNature : AuditableEntity
    {
        [Key]
        public short PaymentNatureId { get; set; }

        [Required(ErrorMessage = "Payment Nature Name is required")]
        [Display(Name = "Payment Nature Name")]
        [StringLength(100)]
        public string PaymentNatureName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}