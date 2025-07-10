using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class AssetDocumentViewModel
    {
        public int? DocumentId { get; set; }

        [Required(ErrorMessage = "Asset is required")]
        [Display(Name = "Asset")]
        public short AssetId { get; set; }

        [Required(ErrorMessage = "Document Type is required")]
        [Display(Name = "Document Type")]
        public short DocumentTypeId { get; set; }

        [Required(ErrorMessage = "Document Title is required")]
        [Display(Name = "Document Title")]
        [StringLength(200)]
        public string DocumentTitle { get; set; }

        [Display(Name = "Document Number")]
        [StringLength(100)]
        public string DocumentNumber { get; set; }

        [Display(Name = "Issue Date")]
        [DataType(DataType.Date)]
        public DateTime? IssueDate { get; set; }

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Issuing Authority")]
        [StringLength(200)]
        public string IssuingAuthority { get; set; }

        [Display(Name = "Upload File")]
        public IFormFile UploadedFile { get; set; }

        [Display(Name = "Current File")]
        public string CurrentFileName { get; set; }

        [Display(Name = "Notes")]
        [StringLength(1000)]
        public string Notes { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Read-only audit fields for display
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        public DateTime? CreatedOn { get; set; }

        [Display(Name = "Updated By")]
        public string UpdatedBy { get; set; }

        [Display(Name = "Updated On")]
        public DateTime? UpdatedOn { get; set; }

        // Display names for dropdowns
        public string AssetName { get; set; }
        public string DocumentTypeName { get; set; }

        // For deletion flag
        public bool IsDeleted { get; set; } = false;
    }

    public class AssetDocumentListViewModel
    {
        public int DocumentId { get; set; }
        public short AssetId { get; set; }
        public string AssetName { get; set; }
        public string DocumentTypeName { get; set; }
        public string DocumentTitle { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string FileName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool IsExpiringSoon => ExpiryDate.HasValue && ExpiryDate.Value <= DateTime.Now.AddDays(30);
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Now;
    }

    public class AssetDocumentIndexViewModel
    {
        public List<AssetDocumentListViewModel> Documents { get; set; } = new List<AssetDocumentListViewModel>();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
        public short? AssetFilter { get; set; }
        public short? DocumentTypeFilter { get; set; }
        public bool ShowExpiringOnly { get; set; } = false;
    }

    public class AssetDocumentDetailViewModel
    {
        public short AssetId { get; set; }
        public string AssetName { get; set; }
        public string AssetNumber { get; set; }
        public List<AssetDocumentViewModel> Documents { get; set; } = new List<AssetDocumentViewModel>();

        // For lookups
        public class LookupData
        {
            public List<AssetLookupItem> Assets { get; set; } = new List<AssetLookupItem>();
            public List<DocumentTypeLookupItem> DocumentTypes { get; set; } = new List<DocumentTypeLookupItem>();
        }

        public class AssetLookupItem
        {
            public short AssetId { get; set; }
            public string AssetNo { get; set; }
            public string DisplayText => AssetNo;
        }

        public class DocumentTypeLookupItem
        {
            public short DocumentTypeId { get; set; }
            public string TypeName { get; set; }
            public bool HasExpiry { get; set; }
        }
    }
}