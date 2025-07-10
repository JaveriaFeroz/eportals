using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace ProcureToPay.Areas.Master.ViewModels
{

    public class ConsigneeViewModel
    {
        public int? ConsigneeId { get; set; }

        [Required(ErrorMessage = "Consignee Name is required")]
        [Display(Name = "Consignee Name")]
        [StringLength(200)]
        public string ConsigneeName { get; set; }

        [Display(Name = "City")]
        public short? CityId { get; set; } // Standardized to int

        [Display(Name = "Client")]
        public short? ClientId { get; set; } // Standardized to int

        [Display(Name = "Company")]
        public short CompanyId { get; set; } // Standardized to int

        [Display(Name = "Contact Number")]
        [StringLength(50)]
        public string ContactNo { get; set; }

        [Display(Name = "Address")]
        [StringLength(500)]
        public string Address { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Last Delivery KMs")]
        public double LastDeliveryKMs { get; set; }

        [Display(Name = "Last Departure Date")]
        public DateTime? LastDepartureDate { get; set; }

        // LastDepartureTime and LastDepartureDateTime were redundant. Kept the combined one.
        public DateTime? LastDepartureDateTime { get; set; }

        [Display(Name = "Standard KMs")]
        public double? StandardKMs { get; set; }

        // --- Read-only properties for display ---
        [Display(Name = "City")]
        public string? CityName { get; set; }
        [Display(Name = "Client")]
        public string? ClientName { get; set; }

        [Display(Name = "Company")]
        public string? CompanyName { get; set; }
        // --- Audit Fields ---
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Updated On")]
        public DateTime? UpdatedOn { get; set; }
    }

    public class ConsigneeListViewModel
    {
        public int ConsigneeId { get; set; }
        public string ConsigneeName { get; set; }
        public string CityName { get; set; }
        public string ClientName { get; set; }
        public string ContactNo { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class ConsigneeIndexViewModel
    {
        public List<ConsigneeListViewModel> Consignees { get; set; } = new List<ConsigneeListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class ConsigneeCreateEditViewModel
    {
        // Correctly named to 'Consignee' (singular)
        public ConsigneeViewModel Consignee { get; set; } = new ConsigneeViewModel();
        public List<SelectListItem> Cities { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Clients { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
    }

    public class ConsigneeFormLookupsViewModel
    {
        public List<CityLookupViewModel> Cities { get; set; } = new List<CityLookupViewModel>();
        public List<ClientLookupViewModel> Clients { get; set; } = new List<ClientLookupViewModel>();
        public List<CompanyLookupViewModel> Companies { get; set; } = new List<CompanyLookupViewModel>();
    }


    public class CityLookupViewModel
    {
        public short CityId { get; set; } // Standardized to int
        public string CityName { get; set; }
    }

}
