using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Procurement.Models
{
    [Table("PurchaseRequestAttachments")]
    public class PurchaseRequestAttachment : AuditableEntity
    {
        public int Id { get; set; }
        public int PurchaseRequestId { get; set; }
        [ForeignKey("PurchaseRequestId")]
        public virtual PurchaseRequest PurchaseRequest { get; set; } = null!;

        [Display(Name = "Attachment Type")]
        public short AttachmentTypeId { get; set; }
        // Note: Add AttachmentType foreign key relationship

        [Required]
        [StringLength(255)]
        [Display(Name = "File Name")]
        public string FileName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Content Type")]
        public string? FileContentType { get; set; }

        [Display(Name = "File Content")]
        public byte[]? FileContent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "File Size (KB)")]
        public decimal FileSizeKB { get; set; }

    }
}
