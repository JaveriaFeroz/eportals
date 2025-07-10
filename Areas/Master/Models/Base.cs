using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Bases")]
    public class Base : AuditableEntity
    {
        [Key]
        public short BaseId { get; set; }

        [Required(ErrorMessage = "Base Name is required")]
        [Display(Name = "Base Name")]
        [StringLength(100)]
        public string BaseName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}