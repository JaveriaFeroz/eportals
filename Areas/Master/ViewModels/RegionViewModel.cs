using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class RegionViewModel
    {
        public short? RegionId { get; set; }

        [Required(ErrorMessage = "Region Name is required")]
        [Display(Name = "Region Name")]
        [StringLength(100)]
        public string RegionName { get; set; }

        [Display(Name = "Tax Rate")]
        [Range(0, 100, ErrorMessage = "Tax Rate must be between 0 and 100")]
        public double TaxRate { get; set; } = 0;

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

    public class RegionListViewModel
    {
        public short RegionId { get; set; }
        public string RegionName { get; set; }
        public double TaxRate { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int CitiesCount { get; set; } // Optional: to show how many cities belong to this region
    }

    public class RegionIndexViewModel
    {
        public List<RegionListViewModel> Regions { get; set; } = new List<RegionListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class RegionCreateEditViewModel
    {
        public RegionViewModel Region { get; set; } = new RegionViewModel();
    }
}
