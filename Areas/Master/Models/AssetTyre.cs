using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("AssetTyres")]
    public class AssetTyre : AuditableEntity
    {
        [Key]
        public int DetailId { get; set; }

        [Required]
        public short AssetId { get; set; }

        [Required(ErrorMessage = "Serial Number is required")]
        [Display(Name = "Serial Number")]
        [StringLength(50)]
        public string SerialNo { get; set; }

        [Required(ErrorMessage = "Make is required")]
        [Display(Name = "Make")]
        [StringLength(100)]
        public string Make { get; set; }

        [Display(Name = "Start KMs")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal StartKMs { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        [ForeignKey("AssetId")]
        public virtual Asset Asset { get; set; }
    }
}
