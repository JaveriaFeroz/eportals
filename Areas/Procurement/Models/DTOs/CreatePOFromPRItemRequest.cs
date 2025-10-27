using ProcureToPay.Areas.Master.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Procurement.Models.DTOs
{
    public class CreatePOFromPRItemRequest
    {
        [Required]
        public int PurchaseRequestItemId { get; set; } // The ID of the PR Item this PO Item is linked to

        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? TaxRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TaxAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiscRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? GSTRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? GSTAmount { get; set; }

        [Display(Name = "Unit of Measure")]
        public short? UoMId { get; set; }

        [ForeignKey("UoMId")]
        public virtual UoM? UoM { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
