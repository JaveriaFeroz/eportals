using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProcureToPay.Areas.Master.Models
{
    [Table("IndustryVerticals")]
    public class IndustryVertical : AuditableEntity
    {
        [Key]
        public short IndustryVerticalId { get; set; }

        [Required(ErrorMessage = "Industry Name is required")]
        [Display(Name = "Industry Name")]
        [StringLength(100)]
        public string IndustryVerticalName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}