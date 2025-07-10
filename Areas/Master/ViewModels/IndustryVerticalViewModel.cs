using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class IndustryVerticalViewModel
    {
        public short? IndustryVerticalId { get; set; }

        [Required(ErrorMessage = "IndustryVertical Name is required")]
        [Display(Name = "IndustryVertical Name")]
        [StringLength(100)]
        public string IndustryVerticalName { get; set; }

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

    public class IndustryVerticalListViewModel
    {
        public short IndustryVerticalId { get; set; }
        public string IndustryVerticalName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class IndustryVerticalIndexViewModel
    {
        public List<IndustryVerticalListViewModel> IndustryVerticals { get; set; } = new List<IndustryVerticalListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }
}