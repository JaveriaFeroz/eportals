using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class SeparationTypeViewModel
    {
        public short? TypeId { get; set; }

        [Required(ErrorMessage = "SeparationType Name is required")]
        [Display(Name = "SeparationType Name")]
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

    public class SeparationTypeListViewModel
    {
        public short TypeId { get; set; }
        public string TypeName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class SeparationTypeIndexViewModel
    {
        public List<SeparationTypeListViewModel> SeparationTypes { get; set; } = new List<SeparationTypeListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class SeparationTypeLookupViewModel
    {
        public short SeparationTypeId { get; set; }
        public string SeparationTypeName { get; set; }
    }
}