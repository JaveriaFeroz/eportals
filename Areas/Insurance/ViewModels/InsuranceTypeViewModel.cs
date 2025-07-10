using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Master.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Insurance.ViewModels
{

    public class InsuranceTypeViewModel
    {
        public short? TypeId { get; set; }

        [Required(ErrorMessage = "Type Name is required.")]
        [StringLength(100)]
        [Display(Name = "Type Name")]
        public string TypeName { get; set; }

        [Required(ErrorMessage = "Please select a company.")]
        [Display(Name = "Company")]
        public short CompanyId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Read-only properties for display purposes
        public string? CompanyName { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        public List<InsuranceDocumentTypeViewModel> RequiredDocuments { get; set; } = new List<InsuranceDocumentTypeViewModel>();
    }

    public class InsuranceTypeCreateEditViewModel
    {
        public InsuranceTypeViewModel InsuranceType { get; set; } = new InsuranceTypeViewModel();

        // Lists for populating dropdowns
        public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();

        public List<InsuranceTypeDocumentViewModel> AvailableDocumentTypes { get; set; } = new List<InsuranceTypeDocumentViewModel>();

        [MinLength(1, ErrorMessage = "At least one document is required.")]
        public List<SelectedInsuranceDocument> SelectedDocuments { get; set; } = new List<SelectedInsuranceDocument>();

    }

    public class InsuranceTypeListViewModel
    {
        public short TypeId { get; set; }
        public string TypeName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        
    }

    public class InsuranceTypeIndexViewModel
    {
        public List<InsuranceTypeListViewModel> InsuranceTypes { get; set; } = new List<InsuranceTypeListViewModel>();
        public bool ShowInactiveOnly { get; set; }
        public string SearchTerm { get; set; }
    }

    public class InsuranceTypeLookupsViewModel
    {
        public List<CompanyLookupViewModel> Companies { get; set; } = new List<CompanyLookupViewModel>();
    }

    public class InsuranceDocumentTypeViewModel
    {
        public int TypeId { get; set; } // Assuming primary key is 'Id' for DocumentType
        public string TypeName { get; set; }
    }

    // NEW: ViewModel for the selected documents from the modal (for POST binding)
    public class SelectedInsuranceDocument
    {
        public int DocumentTypeId { get; set; }
        public bool IsMandatory { get; set; }
    }

    // NEW: ViewModel for representing a document attached to an InsuranceType
    // Useful for Edit scenario, where you pre-populate selected documents
    public class InsuranceTypeDocumentViewModel
    {
        public int DocumentTypeId { get; set; }
        public string DocumentName { get; set; } // To display the name of the document
        public bool IsMandatory { get; set; }
    }
}
