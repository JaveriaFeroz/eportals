using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Receiving.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Procurement.Models
{
    [Table("PurchaseOrderItems")]
    public class PurchaseOrderItem
    {
        public int Id { get; set; }

        [Required]
        public int PurchaseOrderId { get; set; }

        [ForeignKey("PurchaseOrderId")]
        public virtual PurchaseOrder PurchaseOrder { get; set; }

        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(1000)] // Increased length to match DbContext config
        public string? Description { get; set; }

        [StringLength(50)]
        public string? ItemCode { get; set; }

        [Required]
        [StringLength(50)] // Increased length to match DbContext config
        public string Unit { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Consistent with (18,4) for quantities
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")] // Consistent with (18,4) for quantities
        public decimal UpdatedQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; } // Stored for snapshot accuracy

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Disc %")]
        public decimal? TaxRate { get; set; }
        [Display(Name = "Unit of Measure")]
        public short? UoMId { get; set; }

        [ForeignKey("UoMId")]
        public virtual UoM? UoM { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tax Amount")]
        public decimal? TaxAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "GST Rate %")]
        public decimal? GSTRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "GST Amount")]
        public decimal? GSTAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Disc Rate %")]
        public decimal? DiscRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Disc Amount")]
        public decimal? DiscAmount { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Display(Name = "IsCompleted")]
        public bool IsCompleted { get; set; } = false;
        // Optional: Link to PR Item if created from PR
        public int? SourcePurchaseRequestItemId { get; set; } // Corrected property name

        [ForeignKey("SourcePurchaseRequestItemId")]
        public virtual PurchaseRequestItem? SourcePurchaseRequestItem { get; set; } // Corrected navigation property name
        public virtual ICollection<GoodsReceiptNoteItem> GoodsReceiptNoteItems { get; set; } = new List<GoodsReceiptNoteItem>();

        // New properties for PO Item status and received quantity
        public POItemStatus Status { get; set; } = POItemStatus.Open;

    }

    public enum POItemStatus
    {
        [Display(Name = "Open")]
        Open = 0,
        [Display(Name = "Partially Received")]
        PartiallyReceived = 1,
        [Display(Name = "Fully Received")]
        FullyReceived = 2,
        [Display(Name = "Cancelled")]
        Cancelled = 3
    }
}