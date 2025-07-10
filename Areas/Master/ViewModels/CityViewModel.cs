using Microsoft.AspNetCore.Mvc.ModelBinding; // <--- ADD THIS USING DIRECTIVE
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class CityViewModel
    {
        public short? CityId { get; set; }

        [Required(ErrorMessage = "City Code is required")]
        [Display(Name = "City Code")]
        [StringLength(20)]
        public string CityCode { get; set; }

        [Required(ErrorMessage = "City Name is required")]
        [Display(Name = "City Name")]
        [StringLength(100)]
        public string CityName { get; set; }

        [Required(ErrorMessage = "Region is required")]
        [Display(Name = "Region")]
        public short? RegionId { get; set; }

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

        // For dropdown
        [BindNever] // <--- THIS IS THE FIX
        public string? RegionName { get; set; }
    }

    public class CityListViewModel
    {
        public short CityId { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string RegionName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class CityIndexViewModel
    {
        public List<CityListViewModel> Cities { get; set; } = new List<CityListViewModel>();
        public List<SelectListItem> Regions { get; set; } = new List<SelectListItem>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class CityCreateEditViewModel
    {
        public CityViewModel City { get; set; } = new CityViewModel();
        public List<SelectListItem> Regions { get; set; } = new List<SelectListItem>();
    }
}