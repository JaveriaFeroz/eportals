using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Finance.Models
{
    [Table("PaymentRequestDetails")]
    public class PaymentRequestDetail : AuditableEntity
    {
        public int Id { get; set; }
        public int PaymentRequestId { get; set; }
        [ForeignKey("PaymentRequestId")]
        public virtual PaymentRequest PaymentRequest { get; set; } = null!;

        // For job-related payments (from PYDetail.Get)
        [StringLength(50)]
        [Display(Name = "Job Number")]
        public string? JobNo { get; set; }

        [StringLength(50)]
        [Display(Name = "Charge Code")]
        public string? ChargeCode { get; set; }

        [StringLength(200)]
        [Display(Name = "Charge Name")]
        public string? ChargeName { get; set; }

        // For other payments (from PYDetail.GetOtherPayments)
        [StringLength(50)]
        [Display(Name = "Invoice Number")]
        public string InvoiceNo { get; set; }

        [Display(Name = "Invoice Date")]
        public DateTime? InvoiceDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [StringLength(200)]
        [Display(Name = "Payee Name")]
        public string? PayeeName { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Amount (Ex Tax)")]
        public decimal AmountExTax { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "ST Rate %")]
        public decimal STRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Other Tax")]
        public decimal OtherTax { get; set; }

        // Computed total amount
       // public decimal TotalAmount => AmountExTax + (AmountExTax * STRate / 100) + OtherTax;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }
    }
}
