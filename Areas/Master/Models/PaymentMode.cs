using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProcureToPay.Areas.Master.Models
{
    [Table("PaymentModes")]
    public class PaymentMode : AuditableEntity
    {
        [Key]
        public short PaymentModeId { get; set; }

        [Required(ErrorMessage = "PaymentMode Name is required")]
        [Display(Name = "PaymentMode Name")]
        [StringLength(100)]
        public string PaymentModeName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}