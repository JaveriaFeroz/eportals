using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class AssetStatusViewModel
    {
        public short? StatusId { get; set; }

        [Required(ErrorMessage = "Status Name is required")]
        [Display(Name = "Status Name")]
        [StringLength(100)]
        public string StatusName { get; set; }

        [Display(Name = "Editable")]
        public bool Editable { get; set; } = true;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Read-only audit fields for display
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Created On")]
        public DateTime? CreatedOn { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Updated On")]
        public DateTime? UpdatedOn { get; set; }
    }

    public class AssetStatusListViewModel
    {
        public short StatusId { get; set; }
        public string StatusName { get; set; }
        public bool Editable { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class AssetStatusIndexViewModel
    {
        public List<AssetStatusListViewModel> AssetStatuses { get; set; } = new List<AssetStatusListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class AssetStatusLookupViewModel
    {
        public short StatusId { get; set; }
        public string StatusName { get; set; }
    }
}
