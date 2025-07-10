
using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("UoMs")]
    public class UoM : AuditableEntity
    {
        [Key]
        public short UoMId { get; set; }

        [Required(ErrorMessage = "UoM Name is required")]
        [Display(Name = "Unit of Measure")]
        [StringLength(100)]
        public string UoMName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}