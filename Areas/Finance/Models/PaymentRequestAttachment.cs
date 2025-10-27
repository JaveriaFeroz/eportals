using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Finance.Models
{
    [Table("PaymentRequestAttachments")]
    public class PaymentRequestAttachment : AuditableEntity
    {
        public int Id { get; set; }
        public int PaymentRequestId { get; set; }
        [ForeignKey("PaymentRequestId")]
        public virtual PaymentRequest PaymentRequest { get; set; } = null!;

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

        public AttachmentType AttachmentType { get; set; } = null!;

    }
}
