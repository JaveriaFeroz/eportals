using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Makes")]
    public class Make : AuditableEntity
    {
        [Key]
        public short MakeId { get; set; }

        [Required(ErrorMessage = "Make Name is required")]
        [Display(Name = "Make Name")]
        [StringLength(100)]
        public string MakeName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}