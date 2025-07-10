using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class ActivityViewModel
    {
        public short? ActivityId { get; set; }

        [Required(ErrorMessage = "Activity Name is required")]
        [Display(Name = "Activity Name")]
        [StringLength(100, ErrorMessage = "Activity Name cannot exceed 100 characters")]
        public string ActivityName { get; set; }

        [Required(ErrorMessage = "Estimated Hours Required is required")]
        [Display(Name = "Estimated Hours Required")]
        [Range(0.1, 999.9, ErrorMessage = "Estimated Hours must be between 0.1 and 999.9")]
        public double EstHrsReq { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Audit display properties
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Updated On")]
        public DateTime? UpdatedOn { get; set; }

        // Constructor - moved inside the class
        public ActivityViewModel()
        {
            IsActive = true;
        }
    }

    public class ActivityListViewModel
    {
        public short ActivityId { get; set; }
        public string ActivityName { get; set; }
        public double EstHrsReq { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class ActivityIndexViewModel
    {
        public List<ActivityListViewModel> Activities { get; set; } = new List<ActivityListViewModel>();
        public bool ShowInactiveOnly { get; set; }
        public string SearchTerm { get; set; }
    }
}