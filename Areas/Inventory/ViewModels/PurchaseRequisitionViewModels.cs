using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Inventory.Models;
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

        public List<PurchaseRequisitionDetailViewModel> PRDetails { get; set; } = new List<PurchaseRequisitionDetailViewModel>();

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

    public class PurchaseRequisitionDetailViewModel
    {
        [Display(Name = "DetailId")]
        public int Id { get; set; } // Maps to DetailId in EF model

        public int PRNo { get; set; }

        [Display(Name = "Product")]
        public short? ProductId { get; set; }

        [Display(Name = "Service")]
        public short? ServiceId { get; set; }

        [Required(ErrorMessage = "Remarks is required.")] // This is `Narration` in your DB model
        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        [Display(Name = "Remarks")] // Display name for the UI, mapping to Narration field
        public string Remarks { get; set; } // Changed to non-nullable if Required, otherwise string?

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.01, (double)99999999999.99, ErrorMessage = "Quantity must be greater than 0.")] // Adjusted range for decimal, using double here. Max double is huge.
        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; } // Changed to decimal for consistency with DB model's new decimal type

        [Display(Name = "Unit of Measure")]
        public short? UoMId { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.00, (double)99999999999.99, ErrorMessage = "Price must be a non-negative value.")] // Adjusted range for decimal, using double here.
        [Display(Name = "Price")]
        public decimal Price { get; set; } // Changed to decimal

        [Range(0.00, 100.00, ErrorMessage = "GST Rate must be between 0 and 100.")]
        [Display(Name = "GST Rate")]
        public decimal GSTRate { get; set; } // Changed to decimal

        [Range(0.00, 100.00, ErrorMessage = "VAT Rate must be between 0 and 100.")] // NEW: VAT Rate
        [Display(Name = "VAT Rate")]
        public decimal VATRate { get; set; } // NEW: VAT Rate

        // Calculated properties (setters are not strictly needed for model binding if calculated on submit)
        [Display(Name = "Gross Amount")]
        public decimal GrossAmount { get { return Math.Round(Quantity * Price, 2); } set { /* for model binding */ } }

        [Display(Name = "GST Amount")]
        public decimal GSTAmount { get { return Math.Round(GrossAmount * GSTRate / 100.00m, 2); } set { /* for model binding */ } }

        [Display(Name = "VAT Amount")]
        public decimal VATAmount { get { return Math.Round(GrossAmount * VATRate / 100.00m, 2); } set { /* for model binding */ } } // NEW: VAT Amount

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get { return Math.Round(GrossAmount + GSTAmount + VATAmount, 2); } set { /* for model binding */ } } // Updated total

        // Properties for display purposes
        public string? ProductName { get; set; }
        public string? ServiceName { get; set; }
        public string? UoMName { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validate that either ProductId or ServiceId is selected, but not both.
            if (ProductId.HasValue && ProductId > 0 && ServiceId.HasValue && ServiceId > 0)
            {
                yield return new ValidationResult("Cannot select both Product and Service for a single detail item.", new[] { nameof(ProductId), nameof(ServiceId) });
            }
            // Validate that at least one of ProductId, ServiceId, OR Narration is provided
            else if (!ProductId.HasValue && !ServiceId.HasValue && string.IsNullOrWhiteSpace(Remarks))
            {
                yield return new ValidationResult("Either Product, Service, or Remarks/Narration is required for each item.", new[] { nameof(ProductId), nameof(ServiceId), nameof(Remarks) });
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

        public List<SelectListItem> Products { get; set; } = new List<SelectListItem>(); 
        public List<SelectListItem> UoMs { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Services { get; set; } = new List<SelectListItem>(); 
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