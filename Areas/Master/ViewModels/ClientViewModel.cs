using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Master.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    public class ClientViewModel
    {
        public short? ClientId { get; set; }

        [Display(Name = "Account ID")]
        public short? AccountId { get; set; }

        [Required(ErrorMessage = "Client Name is required")]
        [Display(Name = "Client Name")]
        [StringLength(200)]
        public string ClientName { get; set; }

        [Display(Name = "Short Name")]
        [StringLength(50)]
        public string ShortName { get; set; }

        [Display(Name = "Address")]
        [StringLength(500)]
        public string Address { get; set; }

        [Display(Name = "Company")]
        public short? CompanyId { get; set; }

        [Display(Name = "City")]
        public short? CityId { get; set; }

        [Display(Name = "Industry Vertical")]
        public short? IndustryVerticalId { get; set; }

        [Display(Name = "Contract Period")]
        public short? ContractPeriod { get; set; }

        [Display(Name = "Contact Number")]
        [StringLength(50)]
        public string ContactNo { get; set; }

        [Display(Name = "Email")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Display(Name = "Website URL")]
        [StringLength(200)]
        [Url(ErrorMessage = "Invalid URL format")]
        public string URL { get; set; }

        [Display(Name = "Contact Person")]
        [StringLength(100)]
        public string ContactPerson { get; set; }

        [Display(Name = "Payment Mode")]
        public short? PaymentModeId { get; set; }

        // --- FIX: Made properties not on the form nullable ---
        [Display(Name = "Credit Limit")]
        public decimal? CreditLimit { get; set; } // Was decimal

        [Display(Name = "Credit Days")]
        public short? CreditDays { get; set; }

        [Display(Name = "Rate Type")]
        public short? RateTypeId { get; set; }

        [Display(Name = "CW Client ID")]
        [StringLength(50)]
        public string CWClientId { get; set; }

        [Display(Name = "NTN")]
        [StringLength(50)]
        public string NTN { get; set; }

        [Display(Name = "STRN")]
        [StringLength(50)]
        public string STRN { get; set; }

        [Display(Name = "Detention Grace Hours")]
        public short? DetGraceHRs { get; set; } // Was short

        [Display(Name = "Tax Rate")]
        public double? TaxRate { get; set; } // Was double
                                             // ---------------------------------------------------

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Category Mandatory")]
        public bool CategoryMandatory { get; set; }

        [Display(Name = "Product Mandatory")]
        public bool ProductMandatory { get; set; }

        public List<ClientInvoiceFormatViewModel> InvoiceFormats { get; set; } = new List<ClientInvoiceFormatViewModel>();

        // Read-only audit fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        // Display names for foreign keys
        public string? CompanyName { get; set; }
        public string? CityName { get; set; }
        public string? IndustryVerticalName { get; set; }
        public string? PaymentModeName { get; set; }
        public string? RateTypeName { get; set; }
    }
    public class ClientListViewModel
    {
        public short ClientId { get; set; }
        public string ClientName { get; set; }
        public string ShortName { get; set; }
        public string CompanyName { get; set; }
        public string CityName { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }
        public bool CategoryMandatory { get; set; }
        public bool ProductMandatory { get; set; }
        public short DetGraceHRs { get; set; }
        public string RateTypeName { get; set; }
        public string PaymentModeName { get; set; }
        public double TaxRate { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class ClientIndexViewModel
    {
        public List<ClientListViewModel> Clients { get; set; } = new List<ClientListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class ClientCreateEditViewModel
    {
        public ClientViewModel Client { get; set; } = new ClientViewModel();

        // This will hold the list for the city dropdown
        public List<SelectListItem> Companies { get; set; } = new ();

        // This will hold the list for the city dropdown
        public List<SelectListItem> Cities { get; set; } = new ();

        // This will hold the list for the city dropdown
        public List<SelectListItem> IndustryVerticals { get; set; } = new ();

        public List<SelectListItem> PaymentModes { get; set; } = new ();

        public List<SelectListItem> RateTypes { get; set; } = new ();
    }

    public class ClientInvoiceFormatViewModel
    {
        public int? DetailId { get; set; }
        public short? FormatId { get; set; }
        public string FormatName { get; set; }
        public bool IsSelected { get; set; }

        // For tracking changes
        public bool IsNew { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsModified { get; set; }
    }

    public class ClientLookupsViewModel
    {
        public List<CompanyLookupViewModel> Companies { get; set; } = new List<CompanyLookupViewModel>();
        public List<CityLookupViewModel> Cities { get; set; } = new List<CityLookupViewModel>();
        public List<PaymentModeLookupViewModel> PaymentModes { get; set; } = new List<PaymentModeLookupViewModel>();
        public List<IndustryVerticalLookupViewModel> IndustryVerticals { get; set; } = new List<IndustryVerticalLookupViewModel>();
        public List<InvoiceFormatLookupViewModel> InvoiceFormats { get; set; } = new List<InvoiceFormatLookupViewModel>();
        public List<RateTypeLookupViewModel> RateTypes { get; set; } = new List<RateTypeLookupViewModel>();
    }

    public class CompanyLookupViewModel
    {
        public short CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
    public class CityLookupViewModel
    {
        public short CityId { get; set; }
        public string CityName { get; set; }
    }

    public class PaymentModeLookupViewModel
    {
        public short PaymentModeId { get; set; }
        public string PaymentModeName { get; set; }
    }

    public class IndustryVerticalLookupViewModel
    {
        public short IndustryVerticalId { get; set; }
        public string IndustryName { get; set; }
    }

    public class InvoiceFormatLookupViewModel
    {
        public short FormatId { get; set; }
        public string FormatName { get; set; }
    }

    public class RateTypeLookupViewModel
    {
        public short RateTypeId { get; set; }
        public string RateTypeName { get; set; }
    }

    // For RWB specific clients (matching your GetForRWB method)
    public class ClientRWBViewModel
    {
        public short ClientId { get; set; }
        public string ClientName { get; set; }
        public bool CategoryMandatory { get; set; }
        public bool ProductMandatory { get; set; }
        public short DetGraceHRs { get; set; }
        public short? RateTypeId { get; set; }
        public short PaymentModeId { get; set; }
    }

    // For workflow clients (matching your WFClients class)
    public class WorkflowClientViewModel
    {
        public short FormId { get; set; }
        public short ClientId { get; set; }
        public string ClientName { get; set; }
        public string StateName { get; set; }
    }
}