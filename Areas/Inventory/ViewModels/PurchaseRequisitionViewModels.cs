using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
// Assuming you have a folder for ViewModels inside Inventory area or a common ViewModels folder
// using ProcureToPay.Areas.Inventory.ViewModels; // If you have specific Inventory ViewModels
// You might need to adjust the namespace based on your project structure
namespace ProcureToPay.Areas.Inventory.Models
{
    public class PurchaseRequisitionViewModel : IValidatableObject
    {
        [Display(Name = "PR No.")]
        public int PRNo { get; set; } // Not nullable, corresponds to the Model's PK

        [Required(ErrorMessage = "Company Code is required")]
        [Display(Name = "Company Code")]
        [StringLength(50)]
        public string CompanyCode { get; set; } // Not nullable

        [Required(ErrorMessage = "Branch is required")] // Required for form input
        [Display(Name = "Branch")]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "Department is required")] // Required for form input
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }


        [Required(ErrorMessage = "Purchase Nature Type is required.")]
        [Display(Name = "Purchase Nature Type")]
        public PurchaseNatureType PurchaseNatureType { get; set; } // Opex or Capex

        [Required(ErrorMessage = "Purchase Item Type is required.")]
        [Display(Name = "Purchase Item Type")]
        public PurchaseItemType PurchaseItemType { get; set; } // Goods or Service

        [Display(Name = "ProductNature")]
        public short? ProductNatureId { get; set; }

        [Display(Name = "ServiceNature")]
        public short? ServiceNatureId { get; set; }

        [Display(Name = "Required By")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Required By Date is required")] // Required for form input
        public DateTime? RequiredBy { get; set; } // Nullable

        [Display(Name = "Current State")]
        public short? StateId { get; set; } // Nullable

        [Required(ErrorMessage = "Owner is required")] // Required for form input
        [Display(Name = "Owner")]
        [StringLength(100)]
        public string? Owner { get; set; } // Made nullable

        [Display(Name = "Completed")]
        public bool? IsCompleted { get; set; } // Made nullable, removed default initializer

        [Display(Name = "Approved")]
        public bool? Approved { get; set; } // Made nullable, removed default initializer

        [Display(Name = "Rejected")]
        public bool? Rejected { get; set; } // Made nullable, removed default initializer

        [Display(Name = "Budgeted")]
        public bool? Budgeted { get; set; } // Made nullable, removed default initializer

        [Display(Name = "Budget Amount")]
        public decimal? BudgetAmount { get; set; } // Nullable

        [Display(Name = "Budget Remarks")]
        [StringLength(500)]
        public string? BudgetRemarks { get; set; } // Nullable

        [Required(ErrorMessage = "Justification is required")]
        [Display(Name = "Justification")]
        [StringLength(1000)]
        public string Justification { get; set; } // Not nullable

        // Removed: public List<PurchaseRequisitionDetailViewModel> Details { get; set; }

        // Read-only audit fields (already nullable)
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        // Display names for foreign keys (already nullable)
        public string? BranchName { get; set; }
        public string? DepartmentName { get; set; }
        public string? StateName { get; set; }
        public string? ProductNatureName { get; set; }
        public string? ServiceNatureName { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Conditional validation based on PurchaseItemType
            if (PurchaseItemType == PurchaseItemType.Goods && (ProductNatureId == null || ProductNatureId <= 0))
            {
                yield return new ValidationResult(
                    "Product Nature is required when Purchase Item Type is Goods.",
                    new[] { nameof(ProductNatureId) }
                );
            }
            else if (PurchaseItemType == PurchaseItemType.Service && (ServiceNatureId == null || ServiceNatureId <= 0))
            {
                yield return new ValidationResult(
                    "Service Nature is required when Purchase Item Type is Service.",
                    new[] { nameof(ServiceNatureId) }
                );
            }
        }
    }

    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .FirstOrDefault()?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .Name ?? enumValue.ToString();
        }
    }
    // PurchaseRequisitionListViewModel (no changes to nullability based on this request)
    public class PurchaseRequisitionListViewModel
    {
        public int PRNo { get; set; }
        public string CompanyCode { get; set; }
        public string BranchName { get; set; }
        public string DepartmentName { get; set; }
        public string? ProductNatureName { get; set; }
        public string? ServiceNatureName { get; set; }
        public DateTime RequiredBy { get; set; }
        public string StateName { get; set; }
        public string Owner { get; set; }
        public bool IsCompleted { get; set; }
        public bool Approved { get; set; }
        public bool Rejected { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    // PurchaseRequisitionIndexViewModel (no changes based on this request)
    public class PurchaseRequisitionIndexViewModel
    {
        public List<PurchaseRequisitionListViewModel> PurchaseRequisitions { get; set; } = new List<PurchaseRequisitionListViewModel>();
        public bool ShowCompletedOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }

    // PurchaseRequisitionCreateEditViewModel (removed Items and Units dropdowns)
    public class PurchaseRequisitionCreateEditViewModel
    {
        public PurchaseRequisitionViewModel PurchaseRequisition { get; set; } = new PurchaseRequisitionViewModel();

        public List<SelectListItem> Branches { get; set; } = new();
        public List<SelectListItem> Departments { get; set; } = new();
        public List<SelectListItem> States { get; set; } = new();
        public List<SelectListItem> ProductNatures { get; set; } = new();
        public List<SelectListItem> ServiceNatures { get; set; } = new();

        public List<SelectListItem> PurchaseNatureOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PurchaseTypeOptions { get; set; } = new List<SelectListItem>();
    }

    // PurchaseRequisitionLookupsViewModel (removed Item and Unit lookups)
    public class PurchaseRequisitionLookupsViewModel
    {
        public List<BranchLookupViewModel> Branches { get; set; } = new List<BranchLookupViewModel>();
        public List<DepartmentLookupViewModel> Departments { get; set; } = new List<DepartmentLookupViewModel>();
        public List<ProductNatureLookupViewModel> ProductNatures { get; set; } = new List<ProductNatureLookupViewModel>();

        public List<ServiceNatureLookupViewModel> ServiceNatures { get; set; } = new List<ServiceNatureLookupViewModel>();
        
    }

    // Example Lookup ViewModels (removed Item and Unit lookups)
    public class BranchLookupViewModel
    {
        public short BranchId { get; set; }
        public string BranchName { get; set; }
    }
    public class DepartmentLookupViewModel
    {
        public short DepartmentId { get; set; }
        public string DepartmentName { get; set; }
    }
    public class ProductNatureLookupViewModel
    {
        public short NatureId { get; set; }
        public string NatureName { get; set; }

        public bool IsOpex { get; set; } 
        public bool IsCapex { get; set; }

    }

    public class ServiceNatureLookupViewModel
    {
        public short NatureId { get; set; }
        public string NatureName { get; set; }
        public bool IsOpex { get; set; } 
        public bool IsCapex { get; set; }
    }
    public class StateLookupViewModel
    {
        public short StateId { get; set; }
        public string StateName { get; set; }
    }
    
    public class RequestNatureLookupViewModel
    {
        public short RequestNatureId { get; set; }
        public string RequestNatureName { get; set; }
    }
    public class RequestTypeLookupViewModel
    {
        public short RequestTypeId { get; set; }
        public string RequestTypeName { get; set; }
    }
    public class WorkflowLookupViewModel
    {
        public short WorkflowId { get; set; }
        public string WorkflowName { get; set; }
    }
    // Removed: public class ItemLookupViewModel { ... }
    // Removed: public class UnitLookupViewModel { ... }

    // WorkflowPurchaseRequisitionViewModel (no changes to nullability)
    public class WorkflowPurchaseRequisitionViewModel
    {
        public int PRNo { get; set; }
        public string CompanyCode { get; set; }
        public string StateName { get; set; }
        public string Owner { get; set; }
    }
}