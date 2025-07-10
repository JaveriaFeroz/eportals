using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("AssetStatuses")]
    public class AssetStatus : AuditableEntity
    {
        [Key]
        public short StatusId { get; set; }

        [Required(ErrorMessage = "Status Name is required")]
        [Display(Name = "Status Name")]
        [StringLength(100)]
        public string StatusName { get; set; }

        [Display(Name = "Editable")]
        public bool Editable { get; set; } = true;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
