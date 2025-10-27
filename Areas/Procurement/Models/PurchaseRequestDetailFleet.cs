using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Procurement.Models
{
    [Table("PurchaseRequestDetailFleet")]
    public class PurchaseRequestDetailFleet : AuditableEntity
    {
        public int Id { get; set; }

        [Required]
        public int PRFleetId { get; set; }

        [ForeignKey("PRFleetId")]
        public virtual PurchaseRequestsFleet PRFleet { get; set; }

        // New fields as requested
        public int? DetailId { get; set; }

        public short? ProductId { get; set; }

        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        public short? ServiceId { get; set; }

        [StringLength(200)]
        public string ServiceName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public double Quantity { get; set; }

        public short? UoMId { get; set; }

        [StringLength(50)]
        public string UoMName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public double Price { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public double GSTRate { get; set; }

        // Calculated properties - these compute values automatically
        [Column(TypeName = "decimal(18,2)")]
        public double GrossAmount
        {
            get { return Math.Round(Quantity * Price, 2); }
            set { } // Empty setter for EF compatibility
        }

        [Column(TypeName = "decimal(18,2)")]
        public double GSTAmount
        {
            get { return Math.Round(GrossAmount * GSTRate / 100.00, 2); }
            set { } // Empty setter for EF compatibility
        }

        [Column(TypeName = "decimal(18,2)")]
        public double NetAmount
        {
            get { return Math.Round(GrossAmount + GSTAmount, 2); }
            set { } // Empty setter for EF compatibility
        }

      
        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}