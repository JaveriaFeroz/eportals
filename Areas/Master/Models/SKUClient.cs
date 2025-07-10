using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("SKUClient")]
    public class SKUClient : AuditableEntity
    {
        [Key]
        public int DetailId { get; set; }

        [Required]
        public short SKUId { get; set; }

        [Required]
        public short ClientId { get; set; }

        // Navigation properties
        [ForeignKey("SKUId")]
        public virtual SKU SKU { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }
    }



}