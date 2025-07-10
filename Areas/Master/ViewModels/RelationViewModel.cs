using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class RelationViewModel
    {
        public short? RelationId { get; set; }

        [Required(ErrorMessage = "Relation Name is required")]
        [Display(Name = "Relation Name")]
        [StringLength(100)]
        public string RelationName { get; set; }

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

    public class RelationListViewModel
    {
        public short RelationId { get; set; }
        public string RelationName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class RelationIndexViewModel
    {
        public List<RelationListViewModel> Relations { get; set; } = new List<RelationListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class RelationLookupViewModel
    {
        public short RelationId { get; set; }
        public string RelationName { get; set; }
    }
}