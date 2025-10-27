using Microsoft.AspNetCore.Mvc.Rendering;
//using ProcureToPay.Areas.Inventory.Models;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class ServiceNatureViewModel
    {
        public short? NatureId { get; set; }

        [Required(ErrorMessage = "Nature Name is required")]
        [Display(Name = "Nature Name")]
        [StringLength(100)]
        public string NatureName { get; set; }

        [Display(Name = "Opex")]
        public bool IsOpex { get; set; }

        [Display(Name = "Capex")]
        public bool IsCapex { get; set; }

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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsOpex && !IsCapex)
            {
                yield return new ValidationResult(
                    "At least one of 'Opex' or 'Capex' must be selected.",
                    // Associate with IsOpex for the asp-validation-for span
                    new[] { nameof(IsOpex) }
                );
            }
        }
    }

    public class ServiceNatureCreateEditViewModel
    {
        public ServiceNatureViewModel ServiceNature { get; set; } = new ServiceNatureViewModel();
    }
    public class ServiceNatureListViewModel
    {
        public short NatureId { get; set; }
        public string NatureName { get; set; }
        public bool IsOpex { get; set; }
        public bool IsCapex { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class ServiceNatureIndexViewModel
    {
        public List<ServiceNatureListViewModel> ServiceNatures { get; set; } = new List<ServiceNatureListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }
}