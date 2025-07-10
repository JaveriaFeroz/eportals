using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class BaseViewModel
    {
        public short? BaseId { get; set; }

        [Required(ErrorMessage = "Base Name is required")]
        [Display(Name = "Base Name")]
        [StringLength(100)]
        public string BaseName { get; set; }

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

    public class BaseListViewModel
    {
        public short BaseId { get; set; }
        public string BaseName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class BaseIndexViewModel
    {
        public List<BaseListViewModel> Bases { get; set; } = new List<BaseListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class BaseLookupViewModel
    {
        public short BaseId { get; set; }
        public string BaseName { get; set; }
    }
}