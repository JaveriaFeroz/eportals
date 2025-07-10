// View Models
using System.ComponentModel.DataAnnotations;
namespace ProcureToPay.Areas.Master.ViewModels
{
    public class ComplainantViewModel
    {
        public short? ComplainantId { get; set; }

        [Required(ErrorMessage = "Complainant Name is required")]
        [Display(Name = "Complainant Name")]
        [StringLength(100)]
        public string ComplainantName { get; set; }

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

    public class ComplainantListViewModel
    {
        public short ComplainantId { get; set; }
        public string ComplainantName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class ComplainantIndexViewModel
    {
        public List<ComplainantListViewModel> Complainants { get; set; } = new List<ComplainantListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }
}