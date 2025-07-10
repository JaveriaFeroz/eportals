using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class AssetTyreViewModel
    {
        public int? DetailId { get; set; }

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
        public decimal StartKMs { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Properties for tracking changes
        public bool IsNew { get; set; } = false;
        public bool IsModified { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
    }
}
