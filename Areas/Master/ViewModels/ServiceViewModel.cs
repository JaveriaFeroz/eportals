using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class ServiceViewModel
    {
        public short? ServiceId { get; set; }

        [Required(ErrorMessage = "Service Name is required")]
        [Display(Name = "Service Name")]
        [StringLength(200)]
        public string ServiceName { get; set; }

        [Required(ErrorMessage = "Unit Price is required")]
        [Display(Name = "Unit Price")]
        public double UnitPrice { get; set; }

        [Display(Name = "UOM")]
        public short? UoMId { get; set; }

        [Display(Name = "Service Nature")]
        public short? ServiceNatureId { get; set; }

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

        // For dropdowns
        public SelectList? UoMs { get; set; }
        public SelectList? ServiceNatures { get; set; }
    }

    public class ServiceListViewModel
    {
        public short ServiceId { get; set; }
        public string ServiceName { get; set; }
        public double UnitPrice { get; set; }
        public string ServiceTypeName { get; set; }
        public string UoMName { get; set; }
        public string ServiceNatureName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class ServiceIndexViewModel
    {
        public List<ServiceListViewModel> Services { get; set; } = new List<ServiceListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class ServiceLookupsViewModel
    {
        public List<UoMListViewModel> UoMs { get; set; } = new List<UoMListViewModel>();
        public List<ServiceNatureListViewModel> ServiceNatures { get; set; } = new List<ServiceNatureListViewModel>();
    }

}