using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class SupplierTypeViewModel
    {
        public short? TypeId { get; set; }

        [Required(ErrorMessage = "Supplier Type Name is required")]
        [Display(Name = "Supplier Type Name")]
        [StringLength(100)]
        public string SupplierTypeName { get; set; }

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
    }

    public class SupplierTypeListViewModel
    {
        public short TypeId { get; set; }
        public string SupplierTypeName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class SupplierTypeIndexViewModel
    {
        public List<SupplierTypeListViewModel> SupplierTypes { get; set; } = new List<SupplierTypeListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }
}
