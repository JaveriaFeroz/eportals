using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Insurance.ViewModels
{
    public class InsuranceCompanyViewModel
    {
        public short? CompanyId { get; set; }

        [Required(ErrorMessage = "Insurance Company Name is required")]
        [Display(Name = "Insurance Company Name")]
        [StringLength(100)]
        public string CompanyName { get; set; }

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

    public class InsuranceCompanyListViewModel
    {
        public short CompanyId { get; set; }
        public string CompanyName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class InsuranceCompanyIndexViewModel
    {
        public List<InsuranceCompanyListViewModel> InsuranceCompanies { get; set; } = new List<InsuranceCompanyListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class InsuranceCompanyLookupViewModel
    {
        public short CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
}