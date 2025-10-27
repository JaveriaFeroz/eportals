using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Procurement.Models
{
    [Table("PurchaseRequestOrderMappings")]
    public class PurchaseRequestOrderMapping
    {
        public int Id { get; set; }

        [Required]
        public int PurchaseRequestId { get; set; }

        [ForeignKey("PurchaseRequestId")]
        public virtual PurchaseRequest PurchaseRequest { get; set; }

        [Required]
        public int PurchaseOrderId { get; set; }

        [ForeignKey("PurchaseOrderId")]
        public virtual PurchaseOrder PurchaseOrder { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MappedAmount { get; set; } // Amount from PR that this PO covers

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTimeHelper.GetPakistanStandardTime();

        public int CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; }
    }
}
