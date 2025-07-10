using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class DetentionViewModel
    {
        public short? DetentionId { get; set; }

        [Required(ErrorMessage = "Detention Name is required")]
        [Display(Name = "Detention Name")]
        [StringLength(100)]
        public string DetentionName { get; set; }

        [Display(Name = "Hours Threshold")]
        [Range(0, short.MaxValue, ErrorMessage = "Hours Threshold must be a positive number")]
        public short? HRsThreshold { get; set; }

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

    public class DetentionListViewModel
    {
        public short DetentionId { get; set; }
        public string DetentionName { get; set; }
        public short? HRsThreshold { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class DetentionIndexViewModel
    {
        public List<DetentionListViewModel> Detentions { get; set; } = new List<DetentionListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }
}