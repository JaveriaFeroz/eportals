using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.UserManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Procurement.Models
{
    [Table("Bids", Schema = "Procurement")]
    public class Bid
    {
        [Key]
        public int BidId { get; set; }

        [Required]
        public int BidNo { get; set; } // Foreign key to BidEvaluation

        [ForeignKey("BidNo")]
        public virtual BidEvaluation? BidEvaluation { get; set; }

        [Required]
        [Display(Name = "Supplier")]
        public short SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }

        [StringLength(100)]
        [Display(Name = "Quotation Number")]
        public string? QuotationNumber { get; set; }

        [Required]
        [Display(Name = "Submission Date")]
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tax Amount")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Grand Total")]
        public decimal GrandTotal { get; set; }

        [Display(Name = "Delivery Days")]
        public int DeliveryDays { get; set; } = 30;

        [StringLength(200)]
        [Display(Name = "Payment Terms")]
        public string? PaymentTerms { get; set; } = "Net 30";

        [StringLength(50)]
        [Display(Name = "Validity Period")]
        public string? ValidityPeriod { get; set; } = "30 Days";

        // Quick Evaluation Fields
        [Display(Name = "Meets Requirements")]
        public bool MeetsRequirements { get; set; } = true;

        [Display(Name = "Is Responsive")]
        public bool IsResponsive { get; set; } = true;

        [Display(Name = "Rank")]
        public int? Rank { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        // Document
        [StringLength(500)]
        public string? QuotationDocumentPath { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public int CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual User? CreatedByUser { get; set; }

        // Navigation Properties
        public virtual ICollection<BidItem> BidItems { get; set; } = new List<BidItem>();
    }
}
