using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Currencies")]
    public class Currency : AuditableEntity
    {
        [Key]
        public short CurrencyId { get; set; }

        [Required(ErrorMessage = "Currency Name is required")]
        [Display(Name = "Currency Name")]
        [StringLength(100)]
        public string CurrencyName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}