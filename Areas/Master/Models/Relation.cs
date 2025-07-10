using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Relations")]
    public class Relation : AuditableEntity
    {
        [Key]
        public short RelationId { get; set; }

        [Required(ErrorMessage = "Relation Name is required")]
        [Display(Name = "Relation Name")]
        [StringLength(100)]
        public string RelationName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}