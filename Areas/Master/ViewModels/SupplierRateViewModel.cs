using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class SupplierRateViewModel
    {
        public int? SupplierRateId { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        [Display(Name = "Supplier")]
        public short SupplierId { get; set; }

        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public List<SupplierRateDetailViewModel> Details { get; set; } = new List<SupplierRateDetailViewModel>();

        // Read-only audit fields for display
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        public DateTime? CreatedOn { get; set; }

        [Display(Name = "Updated By")]
        public string UpdatedBy { get; set; }

        [Display(Name = "Updated On")]
        public DateTime? UpdatedOn { get; set; }
    }

    public class SupplierRateDetailViewModel
    {
        public int? DetailId { get; set; }

        [Required(ErrorMessage = "From Date is required")]
        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "To Date is required")]
        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }

        [Required(ErrorMessage = "Fuel Rate is required")]
        [Display(Name = "Fuel Rate")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fuel Rate must be greater than 0")]
        public double FuelRate { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Flags for tracking changes (used in UI)
        public bool IsNew { get; set; } = false;
        public bool IsModified { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
    }

    public class SupplierRateListViewModel
    {
        public int SupplierRateId { get; set; }
        public short SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int DetailCount { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class SupplierRateIndexViewModel
    {
        public List<SupplierRateListViewModel> SupplierRates { get; set; } = new List<SupplierRateListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    // Lookup models
    public class SupplierRateLookupViewModel
    {
        public short SupplierRateId { get; set; }
        public string SupplierRateName { get; set; }
    }
}