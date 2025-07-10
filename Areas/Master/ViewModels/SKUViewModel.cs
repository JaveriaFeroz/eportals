using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class SKUViewModel
    {
        public short? SKUId { get; set; }

        [Required(ErrorMessage = "SKU Name is required.")]
        [StringLength(100)]
        [Display(Name = "SKU Name")]
        public string SKUName { get; set; }

        [Display(Name = "SKU Type")]
        public short? SKUTypeId { get; set; }

        [Required(ErrorMessage = "Please select a company.")]
        [Display(Name = "Company")]
        public short CompanyId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Read-only properties for display purposes
        public string? TypeName { get; set; }
        public string? CompanyName { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class SKUCreateEditViewModel
    {
        public SKUViewModel SKU { get; set; } = new SKUViewModel();
        public List<SelectListItem> Types { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
    }

    public class SKUListViewModel
    {
        public short SKUId { get; set; }
        public string SKUName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class SKUIndexViewModel
    {
        // FIX: Renamed 'Categories' to 'SKUs' for correctness.
        public List<SKUListViewModel> SKUs { get; set; } = new List<SKUListViewModel>();
        public bool ShowInactiveOnly { get; set; }
        public string SearchTerm { get; set; }
    }

    public class SKULookupsViewModel
    {
        public List<SKUTypeLookupViewModel> Types { get; set; } = new List<SKUTypeLookupViewModel>();
        public List<CompanyLookupViewModel> Companies { get; set; } = new List<CompanyLookupViewModel>();
    }

   

    public class SKUTypeLookupViewModel
    {
        public short SKUTypeId { get; set; }
        public string SKUTypeName { get; set; }
    }
}