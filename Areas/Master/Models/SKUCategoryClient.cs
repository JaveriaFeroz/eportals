using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    // Entity Model
    [Table("SKUCategoryClients")]
    public class SKUCategoryClient : AuditableEntity
    {
        [Key]
        public int DetailId { get; set; }

        [Required]
        public short CategoryId { get; set; }

        [Required]
        public short ClientId { get; set; }

        // Navigation properties
        public virtual SKUCategory Category { get; set; }
    }
}