using ProcureToPay.Areas.Common.Models; // Assuming AuditableEntity is here
using ProcureToPay.Areas.Master.Models; // Assuming Product and UoM models are here
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System; // For DateTime in AuditableEntity if applicable

namespace ProcureToPay.Areas.Inventory.Models
{
    [Table("PurchaseRequisitionDetails")]
    public class PurchaseRequisitionDetail : AuditableEntity 
    {
        [Key]
        [Column("DetailId")]
        [Display(Name = "DetailId")]
        public int Id { get; set; } 

        [Column("PRNo")]
        [Display(Name = "PRNo")]
        public int PRNo { get; set; }

        [Column("ProductId")]
        [Display(Name = "ProductId")] 
        public short? ProductId { get; set; }

        // [Column("ServiceId")] // Uncomment if needed in DB
        // [Display(Name = "ServiceId")]
        // public short? ServiceId { get; set; }

        [StringLength(500)] 
        [Display(Name = "Remarks")] 
        public string? Remarks { get; set; } 

        [Display(Name = "Quantity")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; } 

        [Display(Name = "UoMId")]
        public short? UoMId { get; set; }

        [Display(Name = "Price")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Display(Name = "GSTRate")]
        [Column(TypeName = "decimal(18,2)")] 
        public decimal GSTRate { get; set; } 

        [NotMapped]
        [Display(Name = "GrossAmount")]
        public decimal GrossAmount { get { return Math.Round(Quantity * Price, 2); } }

        [NotMapped]
        [Display(Name = "GSTAmount")]
        public decimal GSTAmount { get { return Math.Round(GrossAmount * GSTRate / 100.00m, 2); } }

        [NotMapped]
        [Display(Name = "TotalAmount")]
        public decimal TotalAmount { get { return Math.Round(GrossAmount + GSTAmount, 2); } }

        // Navigation Properties for EF Core
        [ForeignKey("PRNo")]
        public virtual PurchaseRequisition PurchaseRequisition { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Products { get; set; }

        [ForeignKey("UoMId")]
        public virtual UoM UoMs { get; set; }

        // If ServiceId is uncommented:
        // [ForeignKey("ServiceId")]
        // public virtual Service Service { get; set; }
    }
}