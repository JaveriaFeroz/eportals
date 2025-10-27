using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class ProductViewModel
    {
        public short? ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [Display(Name = "Product Name")]
        [StringLength(200)]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Unit Price is required")]
        [Display(Name = "Unit Price")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit Price must be greater than 0")]
        public double UnitPrice { get; set; }

        [Display(Name = "UOM")]
        public short? UoMId { get; set; }

        [Display(Name = "Product Nature")]
        public short? ProductNatureId { get; set; }

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
        public SelectList? ProductTypes { get; set; }
        public SelectList? UoMs { get; set; }
        public SelectList? ProductNatures { get; set; }
    }

    public class ProductListViewModel
    {
        public short ProductId { get; set; }
        public string ProductName { get; set; }
        public double UnitPrice { get; set; }
        public string UoMName { get; set; }
        public string ProductNatureName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class ProductIndexViewModel
    {
        public List<ProductListViewModel> Products { get; set; } = new List<ProductListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class ProductLookupsViewModel
    {
        public List<UoMListViewModel> UoMs { get; set; } = new List<UoMListViewModel>();
        public List<ProductNatureListViewModel> ProductNatures { get; set; } = new List<ProductNatureListViewModel>();
    }



    public class ProductPriceUpdateResult
    {
        public bool Success { get; set; }
        public List<ProductPriceChange> Changes { get; set; } = new List<ProductPriceChange>();
        public string Message { get; set; } = string.Empty;
    }
    public class ProductPriceChange
    {
        public short ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }


        public string PriceChangeDisplay => $"{ProductName}: ${OldPrice:N2} ? ${NewPrice:N2}";
        public decimal PriceDifference => NewPrice - OldPrice;
        public decimal PercentageChange => OldPrice > 0 ? ((NewPrice - OldPrice) / OldPrice) * 100 : 0;
    }
}