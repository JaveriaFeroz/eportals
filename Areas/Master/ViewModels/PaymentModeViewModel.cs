using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class PaymentModeViewModel
    {
        public short? PaymentModeId { get; set; }

        [Required(ErrorMessage = "PaymentMode Name is required")]
        [Display(Name = "PaymentMode Name")]
        [StringLength(100)]
        public string PaymentModeName { get; set; }

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

    public class PaymentModeListViewModel
    {
        public short PaymentModeId { get; set; }
        public string PaymentModeName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class PaymentModeIndexViewModel
    {
        public List<PaymentModeListViewModel> PaymentModes { get; set; } = new List<PaymentModeListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }
}