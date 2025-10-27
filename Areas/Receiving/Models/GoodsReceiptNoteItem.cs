using ProcureToPay.Areas.Procurement.Models;
using ProcureToPay.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Receiving.Models
{
    [Table("GoodsReceiptNoteItems")]
    public class GoodsReceiptNoteItem
    {
        public int Id { get; set; }

        [Required]
        public int GoodsReceiptNoteId { get; set; }

        [ForeignKey("GoodsReceiptNoteId")]
        public virtual GoodsReceiptNote GoodsReceiptNote { get; set; }

        [Required]
        public int PurchaseOrderItemId { get; set; }

        [ForeignKey("PurchaseOrderItemId")]
        public virtual PurchaseOrderItem PurchaseOrderItem { get; set; }

        // Added from old GRNDetail
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "PO Quantity")]
        public decimal POQuantity { get; set; } = 0;

        [Required]
        [Display(Name = "Product")]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Quantity")]
  
        public decimal ReceivedQuantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Price")]
        public decimal UnitPrice { get; set; }

        // Added from old GRNDetail
        public double RetailPrice { get; set; } = 0;
        public bool GRNVaryFromPO { get; set; }
        public bool GSTonRP { get; set; } = false;
        public double GSTRate { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "GST Amount")]
        public decimal? GSTAmount { get; set; }
        public double DiscRate { get; set; } = 0;
        public string? Narration { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Net Amount")]
        public decimal TotalPrice { get; set; }

        [StringLength(500)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public DateTime CreatedOn { get; set; } = DateTimeHelper.GetPakistanStandardTime();

        [StringLength(50)]
        public string? Status { get; set; }

        // The newly added properties
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; }

        [StringLength(50)]
        public string UOM { get; set; }
    }
}