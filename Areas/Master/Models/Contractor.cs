using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Contractors")]
    public class Contractor : AuditableEntity
    {
        [Key]
        public short ContractorId { get; set; }

        [Required(ErrorMessage = "Contractor Name is required")]
        [Display(Name = "Contractor Name")]
        [StringLength(100)]
        public string ContractorName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}