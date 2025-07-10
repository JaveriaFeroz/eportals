using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    /// <summary>
    /// Represents the main data for a single SKU Category. Used for display, create, and edit.
    /// </summary>
    public class SKUCategoryViewModel
    {
        public short? CategoryId { get; set; }

        [Required(ErrorMessage = "Category Name is required.")]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [Required(ErrorMessage = "Please select a company.")]
        [Display(Name = "Company")]
        public short CompanyId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Holds the list of clients associated with this category for create/edit binding.
        public List<SKUCategoryClientViewModel> CategoryClients { get; set; } = new List<SKUCategoryClientViewModel>();

        // Read-only properties for display purposes
        public string? CompanyName { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    /// <summary>
    /// Represents a single client in the checklist for associating with a category.
    /// </summary>
    public class SKUCategoryClientViewModel
    {
        public int? DetailId { get; set; }
        public short ClientId { get; set; }
        public string ClientName { get; set; }
        public bool IsSelected { get; set; }
        public bool IsDeleted { get; set; } // Flag for handling deletes on postback
    }

    /// <summary>
    /// The container ViewModel for the Create and Edit views.
    /// Holds the core category data and the lists for dropdowns.
    /// </summary>
    public class SKUCategoryCreateEditViewModel
    {
        public SKUCategoryViewModel SKUCategory { get; set; } = new SKUCategoryViewModel();

        // Lists for populating dropdowns
        public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
    }

    /// <summary>
    /// Represents a category in the main index list.
    /// </summary>
    public class SKUCategoryListViewModel
    {
        public short CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int ClientCount { get; set; }
    }

    /// <summary>
    /// ViewModel for the main index page, containing the list of categories and search/filter state.
    /// </summary>
    public class SKUCategoryIndexViewModel
    {
        public List<SKUCategoryListViewModel> Categories { get; set; } = new List<SKUCategoryListViewModel>();
        public bool ShowInactiveOnly { get; set; }
        public string SearchTerm { get; set; }
    }

    /// <summary>
    /// A consolidated lookup model for all dropdowns needed by the SKUCategory views.
    /// </summary>
    public class SKUCategoryLookupsViewModel
    {
        public List<ClientLookupViewModel> Clients { get; set; } = new List<ClientLookupViewModel>();
        public List<CompanyLookupViewModel> Companies { get; set; } = new List<CompanyLookupViewModel>();
    }

    // Generic lookup models
    public class CompanyLookupViewModel
    {
        public short CompanyId { get; set; }
        public string CompanyName { get; set; }
    }

    public class ClientLookupViewModel
    {
        public short ClientId { get; set; }
        public string ClientName { get; set; }
    }
}
