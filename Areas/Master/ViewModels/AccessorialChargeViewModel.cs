using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    public class AccessorialChargeViewModel
    {
        public short? ChargeId { get; set; }

        [Required(ErrorMessage = "Charge Name is required")]
        [Display(Name = "Charge Name")]
        [StringLength(100)]
        public string ChargeName { get; set; }

        [Required(ErrorMessage = "Charge Code is required")]
        [Display(Name = "Charge Code")]
        [StringLength(20)]
        public string ChargeCode { get; set; }

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
    public class AccessorialChargeListViewModel
    {
        public short ChargeId { get; set; }
        public string ChargeName { get; set; }
        public string ChargeCode { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
    public class AccessorialChargeIndexViewModel
    {
        public List<AccessorialChargeListViewModel> AccessorialCharges { get; set; } = new List<AccessorialChargeListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }
}