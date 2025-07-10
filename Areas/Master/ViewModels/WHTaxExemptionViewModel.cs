using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Master.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    public class WHTaxExemptionViewModel
    {
        public short? ExemptionId { get; set; }

        [Required(ErrorMessage = "Date From is required")]
        [Display(Name = "Date From")]
        [DataType(DataType.Date)]
        public DateTime DateFrom { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Date To is required")]
        [Display(Name = "Date To")]
        [DataType(DataType.Date)]
        public DateTime DateTo { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Please select a company.")]
        [Display(Name = "Company")]
        public short CompanyId { get; set; }

        public string? CompanyName { get; set; }

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

    public class WHTaxExemptionCreateEditViewModel
    {
        public WHTaxExemptionViewModel WHTaxExemptions { get; set; } = new WHTaxExemptionViewModel();
        public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
    }
    public class WHTaxExemptionListViewModel
    {
        public short ExemptionId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class WHTaxExemptionIndexViewModel
    {
        public List<WHTaxExemptionListViewModel> WHTaxExemptions { get; set; } = new List<WHTaxExemptionListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class WHTaxExemptionLookupsViewModel
    {
        public List<CompanyLookupViewModel> Companies { get; set; } = new List<CompanyLookupViewModel>();
    }
}