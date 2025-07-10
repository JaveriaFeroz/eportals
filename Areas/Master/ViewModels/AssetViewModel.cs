using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Master.Models;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class AssetViewModel
    {
        public short? AssetId { get; set; }

        [Required(ErrorMessage = "Asset Number is required")]
        [Display(Name = "Asset Number")]
        [StringLength(50)]
        public string AssetNo { get; set; }

        [Required(ErrorMessage = "Asset Type is required")]
        [Display(Name = "Asset Type")]
        public short AssetTypeId { get; set; }

        [Display(Name = "Capacity")]
        public short? CapacityId { get; set; }

        
        [Display(Name = "Make")]
        public short? MakeId { get; set; }

        [Display(Name = "Model")]
        [StringLength(100)]
        public string? Model { get; set; }

        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseDate { get; set; }

        [Required(ErrorMessage = "Lease Type is required")]
        [Display(Name = "Lease Type")]
        public short LeaseTypeId { get; set; }

        [Display(Name = "Supplier")]
        public short? SupplierId { get; set; }

        [Required(ErrorMessage = "Start KMs is required")]
        [Display(Name = "Start KMs")]
        public decimal StartKMs { get; set; }

        [Required(ErrorMessage = "KMs is required")]
        [Display(Name = "KMs")]
        public decimal KMs { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public short StatusId { get; set; }

        [Display(Name = "Driver 1")]
        public short? DriverId1 { get; set; }

        [Display(Name = "Driver 2")]
        public short? DriverId2 { get; set; }

        [Display(Name = "Trailer")]
        public short? TrailerId { get; set; }

        [Display(Name = "FA Code")]
        [StringLength(50)]
        public string? FACode { get; set; }

        [Required(ErrorMessage = "City is required")]
        [Display(Name = "City")]
        public short CityId { get; set; }

        [Display(Name = "Client")]
        public short? ClientId { get; set; }

        [Display(Name = "Base")]
        public short? BaseId { get; set; }

        [Required(ErrorMessage = "Company is required")]
        [Display(Name = "Company")]
        public short CompanyId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Asset Tyres
        public List<AssetTyreViewModel> AssetTyres { get; set; } = new List<AssetTyreViewModel>();

        // Read-only audit fields for display
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Created On")]
        public DateTime? CreatedOn { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Updated On")]
        public DateTime? UpdatedOn { get; set; }

        // Display names for dropdowns
        public string? AssetTypeName { get; set; }
        public string? CapacityName { get; set; }
        public string? MakeName { get; set; }
        public string? LeaseTypeName { get; set; }
        public string? TrailerName { get; set; }
        public string? SupplierName { get; set; }
        public string? Driver1Name { get; set; }
        public string? Driver2Name { get; set; }
        public string? CityName { get; set; }
        public string? ClientName { get; set; }
        public string? BaseName { get; set; }
        public string? CompanyName { get; set; }
        public string? StatusName { get; set; }

    }

    public class AssetListViewModel

    {
        public short AssetId { get; set; }
        public string AssetNo { get; set; }
        public string AssetTypeName { get; set; }
        public string StatusName { get; set; }
        public decimal KMs { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

    }

    public class AssetIndexViewModel
    {
        public List<AssetListViewModel> Assets { get; set; } = new List<AssetListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }



    public class AssetCreateEditViewModel
    {
        public AssetViewModel Asset { get; set; } = new AssetViewModel();

        // Properties to hold all your dropdown lists
        public IEnumerable<SelectListItem> AssetTypes { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Cities { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Suppliers { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Clients { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Capacities { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Makes { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> LeaseTypes { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Drivers { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Trailers { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> AssetStatuses { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Bases { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Companies { get; set; } = new List<SelectListItem>();

    }


    public class AssetLookupsViewModel
    {
        public List<CompanyLookupViewModel> Companies { get; set; } = new List<CompanyLookupViewModel>();

        public List<AssetTypeLookupViewModel> AssetTypes { get; set; } = new List<AssetTypeLookupViewModel>();
        public List<CityLookupViewModel> Cities { get; set; } = new List<CityLookupViewModel>();
        public List<SupplierLookupViewModel> Suppliers { get; set; } = new List<SupplierLookupViewModel>();
        public List<ClientLookupViewModel> Clients { get; set; } = new List<ClientLookupViewModel>();
        public List<CapacityLookupViewModel> Capacities { get; set; } = new List<CapacityLookupViewModel>();
        public List<MakeLookupViewModel> Makes { get; set; } = new List<MakeLookupViewModel>();

        public List<LeaseTypeLookupViewModel> LeaseTypes { get; set; } = new List<LeaseTypeLookupViewModel>();

        public List<DriverLookupViewModel> Drivers { get; set; } = new List<DriverLookupViewModel>();
        public List<TrailerLookupViewModel> Trailers { get; set; } = new List<TrailerLookupViewModel>();
        public List<AssetStatusLookupViewModel> AssetStatuses { get; set; } = new List<AssetStatusLookupViewModel>();
        public List<BaseLookupViewModel> Bases { get; set; } = new List<BaseLookupViewModel>();
    }
    public class AssetLookupViewModel
    {
        public short AssetId { get; set; }
        public string AssetName { get; set; }
    }

}