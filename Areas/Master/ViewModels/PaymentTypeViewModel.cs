using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class PaymentTypeViewModel
    {
        public short? PaymentTypeId { get; set; }

        [Required(ErrorMessage = "Payment Type Name is required")]
        [Display(Name = "Payment Type Name")]
        [StringLength(100)]
        public string PaymentTypeName { get; set; }

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

    public class PaymentTypeListViewModel
    {
        public short PaymentTypeId { get; set; }
        public string PaymentTypeName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class PaymentTypeIndexViewModel
    {
        public List<PaymentTypeListViewModel> PaymentTypes { get; set; } = new List<PaymentTypeListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }
}