using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.Master.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("DocumentTypes")]
public class DocumentType : AuditableEntity
{
    [Key]
    public short DocumentTypeId { get; set; }

    [Required]
    [Display(Name = "Type Name")]
    [StringLength(100)]
    public string TypeName { get; set; }

    [Display(Name = "Description")]
    [StringLength(500)]
    public string Description { get; set; }

    [Display(Name = "Is Required")]
    public bool IsRequired { get; set; } = false;

    [Display(Name = "Has Expiry")]
    public bool HasExpiry { get; set; } = false;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Company")]
    public short? CompanyId { get; set; }

    // Navigation properties
    public virtual ICollection<AssetDocument> AssetDocuments { get; set; } = new List<AssetDocument>();
}