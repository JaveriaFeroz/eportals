using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class ShipperViewModel
    {
        public int? ShipperId { get; set; }

        [Required(ErrorMessage = "Shipper Name is required")]
        [Display(Name = "Shipper Name")]
        [StringLength(100)]
        public string ShipperName { get; set; }

        [Display(Name = "Address")]
        [StringLength(255)]
        public string Address { get; set; }

        [Display(Name = "City")]
        public short? CityId { get; set; }

        [Display(Name = "Client")]
        public short? ClientId { get; set; }

        [Required(ErrorMessage = "Please select a company.")]
        [Display(Name = "Company")]
        public short CompanyId { get; set; }

        [Required(ErrorMessage = "Please select a contact number.")]
        [Display(Name = "Contact Number")]
        [StringLength(50)]
        public string ContactNo { get; set; }


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

        public string? ClientName { get; set; }
        public string? CompanyName { get; set; }
        public string? CityName { get; set; }


    }

    public class ShipperCreateEditViewModel
    {
        public ShipperViewModel Shipper { get; set; } = new ShipperViewModel();
        public List<SelectListItem> Cities { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Clients { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
    }

    public class ShipperListViewModel
    {
        public int ShipperId { get; set; }
        public string ShipperName { get; set; }
        public string Address { get; set; }
        public short? CityId { get; set; }
        public string CityName { get; set; }
        public short? ClientId { get; set; }
        public string ClientName { get; set; }

        public short? CompanyId { get; set; }
        public string Company { get; set; }
        public string ContactNo { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class ShipperIndexViewModel
    {
        public List<ShipperListViewModel> Shippers { get; set; } = new List<ShipperListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class ShipperLookupsViewModel
    {
        public List<ShipperTypeLookupViewModel> Shippers { get; set; } = new List<ShipperTypeLookupViewModel>();

        public List<CitiesLookupViewModel> Cities { get; set; } = new List<CitiesLookupViewModel>();
        public List<ClientLookupViewModel> Clients { get; set; } = new List<ClientLookupViewModel>();
        public List<CompanyLookupViewModel> Companies { get; set; } = new List<CompanyLookupViewModel>();
    }

    public class ShipperTypeLookupViewModel
    {
        public int ShipperId { get; set; }
        public string ShipperName { get; set; }
    }
    public class CitiesLookupViewModel
    {
        public short CityId { get; set; }
        public string CityName { get; set; }
    }
}