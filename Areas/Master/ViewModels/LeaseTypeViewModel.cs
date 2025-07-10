using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class LeaseTypeViewModel
    {
        public short? TypeId { get; set; }

        [Required(ErrorMessage = "Lease Type Name is required")]
        [Display(Name = "Lease Type Name")]
        [StringLength(100)]
        public string TypeName { get; set; }

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

    public class LeaseTypeListViewModel
    {
        public short TypeId { get; set; }
        public string TypeName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class LeaseTypeIndexViewModel
    {
        public List<LeaseTypeListViewModel> LeaseTypes { get; set; } = new List<LeaseTypeListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class LeaseTypeLookupViewModel
    {
        public short TypeId { get; set; }
        public string TypeName { get; set; }
    }
}