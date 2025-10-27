using ProcureToPay.Areas.Master.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Procurement.Models
{
    [Table("BidItems", Schema = "Procurement")]
    public class BidItem
    {
        [Key]
        public int BidItemId { get; set; }

        [Required]
        public int BidId { get; set; }

        [ForeignKey("BidId")]
        public virtual Bid? Bid { get; set; }

        // Link to PR Item for comparison
        public int? PurchaseRequestItemId { get; set; }

        [ForeignKey("PurchaseRequestItemId")]
        public virtual PurchaseRequestItem? PurchaseRequestItem { get; set; }

        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Specifications { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        [Required]
        [StringLength(50)]
        public string UOM { get; set; } = string.Empty;

        [Display(Name = "Unit of Measure")]
        public short? UoMId { get; set; }

        [ForeignKey("UoMId")]
        public virtual UoM? UoM { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public bool MeetsSpecifications { get; set; } = true;
    }
}
