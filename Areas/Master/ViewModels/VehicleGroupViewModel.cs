using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class VehicleGroupViewModel
    {
        public short? GroupId { get; set; }

        [Required(ErrorMessage = "VehicleGroup Name is required")]
        [Display(Name = "VehicleGroup Name")]
        [StringLength(100)]
        public string GroupName { get; set; }

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

    public class VehicleGroupListViewModel
    {
        public short GroupId { get; set; }
        public string GroupName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class VehicleGroupIndexViewModel
    {
        public List<VehicleGroupListViewModel> VehicleGroups { get; set; } = new List<VehicleGroupListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }
}