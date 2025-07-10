using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class MakeViewModel
    {
        public short? MakeId { get; set; }

        [Required(ErrorMessage = "Make Name is required")]
        [Display(Name = "Make Name")]
        [StringLength(100)]
        public string MakeName { get; set; }

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

    public class MakeListViewModel
    {
        public short MakeId { get; set; }
        public string MakeName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class MakeIndexViewModel
    {
        public List<MakeListViewModel> Makes { get; set; } = new List<MakeListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class MakeLookupViewModel
    {
        public short MakeId { get; set; }
        public string MakeName { get; set; }
    }
}