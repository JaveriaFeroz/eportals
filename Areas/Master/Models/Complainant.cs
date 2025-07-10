using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    // Entity Model
    [Table("Complainants")]
    public class Complainant : AuditableEntity
    {
        [Key]
        public short ComplainantId { get; set; }

        [Required(ErrorMessage = "Complainant Name is required")]
        [Display(Name = "Complainant Name")]
        [StringLength(100)]
        public string ComplainantName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }


}