using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProcureToPay.Areas.Master.Models
{
    [Table("PaymentTypes")]
    public class PaymentType : AuditableEntity
    {
        [Key]
        public short PaymentTypeId { get; set; }

        [Required(ErrorMessage = "PaymentType Name is required")]
        [Display(Name = "PaymentType Name")]
        [StringLength(100)]
        public string PaymentTypeName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}