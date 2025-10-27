using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class CurrencyViewModel
    {
        public short? CurrencyId { get; set; }

        [Required(ErrorMessage = "Currency Name is required")]
        [Display(Name = "Currency Name")]
        [StringLength(100)]
        public string CurrencyName { get; set; }

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

    public class CurrencyListViewModel
    {
        public short CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class CurrencyIndexViewModel
    {
        public List<CurrencyListViewModel> Currencies { get; set; } = new List<CurrencyListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class CurrencyLookupViewModel
    {
        public short CurrencyId { get; set; }
        public string CurrencyName { get; set; }
    }
}