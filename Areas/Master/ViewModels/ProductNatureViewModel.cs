using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class ProductNatureViewModel
    {
        public short? NatureId { get; set; }

        [Required(ErrorMessage = "Nature Name is required")]
        [Display(Name = "Nature Name")]
        [StringLength(100)]
        public string NatureName { get; set; }

        [Required(ErrorMessage = "Please select a purchase nature.")]
        [Display(Name = "PurhcaseNature")]
        public short PurchaseNatureId { get; set; }

        public string? PurchaseNatureName { get; set; }

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

    public class ProductNatureCreateEditViewModel
    {
        public ProductNatureViewModel ProductNature { get; set; } = new ProductNatureViewModel();
        public List<SelectListItem> PurchaseNatures { get; set; } = new List<SelectListItem>();
    }
    public class ProductNatureListViewModel
    {
        public short NatureId { get; set; }
        public string NatureName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class ProductNatureIndexViewModel
    {
        public List<ProductNatureListViewModel> ProductNatures { get; set; } = new List<ProductNatureListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }
    public class ProductNatureLookupsViewModel
    {
        public List<PurchaseNatureLookupViewModel> PurchaseNatures { get; set; } = new List<PurchaseNatureLookupViewModel>();
    }
}