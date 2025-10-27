using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class PaymentNatureViewModel
    {
        public short? PaymentNatureId { get; set; }

        [Required(ErrorMessage = "Payment Nature Name is required")]
        [Display(Name = "Payment Nature Name")]
        [StringLength(100)]
        public string PaymentNatureName { get; set; }

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

    public class PaymentNatureListViewModel
    {
        public short PaymentNatureId { get; set; }
        public string PaymentNatureName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class PaymentNatureIndexViewModel
    {
        public List<PaymentNatureListViewModel> PaymentNatures { get; set; } = new List<PaymentNatureListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }
}