using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class ContractorViewModel
    {
        public short? ContractorId { get; set; }

        [Required(ErrorMessage = "Contractor Name is required")]
        [Display(Name = "Contractor Name")]
        [StringLength(100)]
        public string ContractorName { get; set; }

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

    public class ContractorListViewModel
    {
        public short ContractorId { get; set; }
        public string ContractorName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class ContractorIndexViewModel
    {
        public List<ContractorListViewModel> Contractors { get; set; } = new List<ContractorListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class ContractorLookupViewModel
    {
        public short ContractorId { get; set; }
        public string ContractorName { get; set; }
    }
}