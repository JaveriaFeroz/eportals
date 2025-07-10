using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class CompanyViewModel
    {
        public short? CompanyId { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        [Display(Name = "Company Name")]
        [StringLength(100)]
        public string CompanyName { get; set; }

        [Display(Name = "Company Address")]
        [StringLength(500)]
        public string CompanyAddress { get; set; }

        [Display(Name = "NTN")]
        [StringLength(50)]
        public string NTN { get; set; }

        [Display(Name = "Period Name")]
        [StringLength(100)]
        public string? PeriodName { get; set; }

        [Display(Name = "Distance Threshold")]
        [Range(0, short.MaxValue, ErrorMessage = "Distance Threshold must be a positive number")]
        public short? DistanceThreshold { get; set; }

        [Display(Name = "Report Grace Hours")]
        [Range(0, short.MaxValue, ErrorMessage = "Report Grace Hours must be a positive number")]
        public short? ReportGraceHRs { get; set; }

        [Display(Name = "Bank Account")]
        public short? BankAccountId { get; set; }

        [Display(Name = "AR Period")]
        public short? ARPeriodId { get; set; }

        [Display(Name = "AR Account")]
        public short? ARAccountId { get; set; }

        [Display(Name = "AP Period")]
        public short? APPeriodId { get; set; }

        [Display(Name = "AP Account")]
        public short? APAccountId { get; set; }

        [Display(Name = "GL Period")]
        public short? GLPeriodId { get; set; }

        [Display(Name = "Ops Period")]
        public short? OpsPeriodId { get; set; }

        [Display(Name = "Trip Revenue Account")]
        public short? TripRevenueAccountId { get; set; }

        [Display(Name = "Fuel Expense Account")]
        public short? FuelExpenseAccountId { get; set; }

        [Display(Name = "Advance Account")]
        public short? AdvanceAccountId { get; set; }

        [Display(Name = "Enable GL")]
        public bool EnableGL { get; set; }

        [Display(Name = "Enable Partial Delivery")]
        public bool EnablePartialDelivery { get; set; }

        [Display(Name = "Route By Consignee")]
        public bool RouteByConsignee { get; set; }

        [Display(Name = "Separate Fixed Invoice")]
        public bool SeparateFixedInvoice { get; set; }

        [Display(Name = "Mandatory Driver 2")]
        public bool IsMandatoryDriver2 { get; set; }

        [Display(Name = "Allow Trailer")]
        public bool AllowTrailer { get; set; }

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

    public class CompanyListViewModel
    {
        public short CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string NTN { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class CompanyIndexViewModel
    {
        public List<CompanyListViewModel> Companies { get; set; } = new List<CompanyListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class CompanyConfigViewModel
    {
        public short CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string NTN { get; set; }
        public string PeriodName { get; set; }
        public short? DistanceThreshold { get; set; }
        public short? ReportGraceHRs { get; set; }
        public short? BankAccountId { get; set; }
        public short? ARPeriodId { get; set; }
        public short? ARAccountId { get; set; }
        public short? APPeriodId { get; set; }
        public short? APAccountId { get; set; }
        public short? GLPeriodId { get; set; }
        public short? OpsPeriodId { get; set; }
        public short? TripRevenueAccountId { get; set; }
        public short? FuelExpenseAccountId { get; set; }
        public short? AdvanceAccountId { get; set; }
        public bool EnableGL { get; set; }
        public bool EnablePartialDelivery { get; set; }
        public bool RouteByConsignee { get; set; }
        public bool SeparateFixedInvoice { get; set; }
        public bool IsMandatoryDriver2 { get; set; }
        public bool AllowTrailer { get; set; }
    }

    public class CompanySimpleViewModel
    {
        public short CompanyId { get; set; }
        public string CompanyName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CompanyLookupsViewModel
    {
        public List<AccountViewModel> LstAccount { get; set; } = new List<AccountViewModel>();
    }

    public class AccountViewModel
    {
        public short AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountCode { get; set; }
    }
}