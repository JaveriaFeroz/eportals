using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    public class SubCategoryViewModel
    {
        public short? SubCategoryId { get; set; }

        [Required(ErrorMessage = "Sub Category Name is required")]
        [Display(Name = "Sub Category Name")]
        [StringLength(100)]
        public string SubCategoryName { get; set; }

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

    public class SubCategoryListViewModel
    {
        public short SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class SubCategoryIndexViewModel
    {
        public List<SubCategoryListViewModel> SubCategories { get; set; } = new List<SubCategoryListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }
}