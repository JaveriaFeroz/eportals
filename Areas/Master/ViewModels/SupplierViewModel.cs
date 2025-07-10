using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
   
        public class SupplierViewModel
        {
            public short? SupplierId { get; set; }

        [Required(ErrorMessage = "Supplier Type is required")]
        [Display(Name = "Supplier Type")]
        public short SupplierTypeId { get; set; }

        [Required(ErrorMessage = "Supplier Name is required")]
            [Display(Name = "Supplier Name")]
            [StringLength(200)]
            public string SupplierName { get; set; }

            [Display(Name = "SC Rate")]
            [Range(0, double.MaxValue, ErrorMessage = "SC Rate must be a positive number")]
            public double SCRate { get; set; }

            [Required(ErrorMessage = "Address is required")]
            [Display(Name = "Address")]
            [StringLength(500)]
            public string Address { get; set; }

            [Display(Name = "City")]
            public short? CityId { get; set; }

            [Display(Name = "Email")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            [StringLength(100)]
            public string? Email { get; set; }

            [Display(Name = "Phone Number")]
            [StringLength(20)]
            public string? PhoneNo { get; set; }

            [Display(Name = "Fax Number")]
            [StringLength(20)]
            public string? FaxNo { get; set; }

            [Display(Name = "Contact Name")]
            [StringLength(100)]
            public string? ContactName { get; set; }

            [Display(Name = "Mobile Number")]
            [StringLength(20)]
            public string? MobileNo { get; set; }

            [Display(Name = "NTN")]
            [StringLength(50)]
            public string? NTN { get; set; }

            [Display(Name = "URL")]
            [Url(ErrorMessage = "Invalid URL format")]
            [StringLength(200)]
            public string? URL { get; set; }

            [Display(Name = "Control Supplier ID")]
            [StringLength(50)]
            public string? ControlSupplierId { get; set; }

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

            // Display names for related entities
            [Display(Name = "Supplier Type")]
            public string? SupplierTypeName { get; set; }

            [Display(Name = "City")]
            public string? CityName { get; set; }
        }

        public class SupplierListViewModel
        {
            public short SupplierId { get; set; }
            public string SupplierName { get; set; }
            public string SupplierTypeName { get; set; }
            public string? CityName { get; set; }
            public bool IsActive { get; set; }
            public string CreatedBy { get; set; }
            public DateTime CreatedOn { get; set; }
            public string? UpdatedBy { get; set; }
            public DateTime? UpdatedOn { get; set; }
        }

        public class SupplierIndexViewModel
        {
            public List<SupplierListViewModel> Suppliers { get; set; } = new List<SupplierListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

        public class SupplierLookupsViewModel
        {
            public List<SupplierTypeViewModel> SupplierTypes { get; set; } = new List<SupplierTypeViewModel>();
            public List<CityViewModel> Cities { get; set; } = new List<CityViewModel>();
        }
        // Container for the Create/Edit page
        public class SupplierCreateEditViewModel
        {
             public SupplierViewModel Supplier { get; set; } = new();
             public List<SelectListItem> SupplierTypes { get; set; } = new();
             public List<SelectListItem> Cities { get; set; } = new();
        }

    public class SupplierLookupViewModel
    {
        public short SupplierId { get; set; }
        public string SupplierName { get; set; }
    }
}
