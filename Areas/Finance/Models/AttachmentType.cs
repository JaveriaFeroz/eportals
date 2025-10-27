using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Finance.Models
{
    [Table("AttachmentTypes")]
    public class AttachmentType : AuditableEntity
    {
        public short Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Attachment Type Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Is Mandatory")]
        public bool IsMandatory { get; set; } = true;

        [Display(Name = "Allowed Size (KB)")]
        public int AllowedSizeKB { get; set; } = 512;

        [Display(Name = "State")]
        public short? StateId { get; set; }

        [Display(Name = "Payment Nature")]
        public short? PaymentNatureId { get; set; }

        [Display(Name = "Workflow Type")]
        public short WorkFlowTypeId { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<PaymentRequestAttachment> PaymentRequestAttachments { get; set; } = new List<PaymentRequestAttachment>();
    }
}
