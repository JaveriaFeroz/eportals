using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("SupplierRates")]
    public class SupplierRate : AuditableEntity
    {
        [Key]
        public int SupplierRateId { get; set; }

        [Required]
        public short SupplierId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; }

        public virtual ICollection<SupplierRateDetail> Details { get; set; } = new List<SupplierRateDetail>();
    }
}
