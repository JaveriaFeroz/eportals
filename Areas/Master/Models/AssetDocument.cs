using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("AssetDocuments")]
    public class AssetDocument : AuditableEntity
    {
        [Key]
        public int DocumentId { get; set; }

        [Required]
        [Display(Name = "Asset")]
        public short AssetId { get; set; }

        [Required]
        [Display(Name = "Document Type")]
        public short DocumentTypeId { get; set; }

        [Required]
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

        [Display(Name = "File Path")]
        [StringLength(500)]
        public string FilePath { get; set; }

        [Display(Name = "File Name")]
        [StringLength(200)]
        public string FileName { get; set; }

        [Display(Name = "File Size")]
        public long? FileSize { get; set; }

        [Display(Name = "Notes")]
        [StringLength(1000)]
        public string Notes { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Company")]
        public short? CompanyId { get; set; }

        // Navigation properties
        [ForeignKey("AssetId")]
        public virtual Asset Asset { get; set; }

        [ForeignKey("DocumentTypeId")]
        public virtual DocumentType DocumentType { get; set; }
    }

   
}

