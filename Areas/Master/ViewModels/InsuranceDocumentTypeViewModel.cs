using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class InsuranceDocumentTypeViewModel
    {
        public short? TypeId { get; set; }

        [Required(ErrorMessage = "Insurance Document Type Name is required")]
        [Display(Name = "Insurance Document Type Name")]
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

    public class InsuranceDocumentTypeListViewModel
    {
        public short TypeId { get; set; }
        public string TypeName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class InsuranceDocumentTypeIndexViewModel
    {
        public List<InsuranceDocumentTypeListViewModel> InsuranceDocumentTypes { get; set; } = new List<InsuranceDocumentTypeListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class InsuranceDocumentTypeLookupViewModel
    {
        public short TypeId { get; set; }
        public string TypeName { get; set; }
    }
}