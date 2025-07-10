using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class PurchaseNatureViewModel
    {
        public short? PurchaseNatureId { get; set; }

        [Required(ErrorMessage = "PurchaseNature Name is required")]
        [Display(Name = "PurchaseNature Name")]
        [StringLength(100)]
        public string PurchaseNatureName { get; set; }

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

    public class PurchaseNatureListViewModel
    {
        public short PurchaseNatureId { get; set; }
        public string PurchaseNatureName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class PurchaseNatureIndexViewModel
    {
        public List<PurchaseNatureListViewModel> PurchaseNatures { get; set; } = new List<PurchaseNatureListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class PurchaseNatureLookupViewModel
    {
        public short PurchaseNatureId { get; set; }
        public string PurchaseNatureName { get; set; }
    }
}