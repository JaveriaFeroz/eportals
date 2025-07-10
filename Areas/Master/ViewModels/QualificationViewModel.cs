using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ProcureToPay.Areas.Master.ViewModels
{
    [DataContract]
    public class QualificationViewModel
    {
        [DataMember(Order = 0)]
        public short? QualificationId { get; set; }

        [Required(ErrorMessage = "Qualification Name is required")]
        [Display(Name = "Qualification Name")]
        [StringLength(100)]
        [DataMember(Order = 1)]
        public string QualificationName { get; set; }

        [Display(Name = "Active")]
        [DataMember(Order = 2)]
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

    public class QualificationListViewModel
    {
        public short QualificationId { get; set; }
        public string QualificationName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class QualificationIndexViewModel
    {
        public List<QualificationListViewModel> Qualifications { get; set; } = new List<QualificationListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    public class QualificationLookupViewModel
    {
        public short QualificationId { get; set; }
        public string QualificationName { get; set; }
    }

}
