using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class TrailerViewModel
    {
        public short? TrailerId { get; set; }

        [Required(ErrorMessage = "Trailer Name is required")]
        [Display(Name = "Trailer Name")]
        [StringLength(100)]
        public string TrailerName { get; set; }

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

    public class TrailerListViewModel
    {
        public short TrailerId { get; set; }
        public string TrailerName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class TrailerIndexViewModel
    {
        public List<TrailerListViewModel> Trailers { get; set; } = new List<TrailerListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class TrailerLookupViewModel
    {
        public short TrailerId { get; set; }
        public string TrailerName { get; set; }
    }
}