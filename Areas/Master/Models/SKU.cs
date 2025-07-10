using ProcureToPay.Areas.Common.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("SKU")]
    public class SKU : AuditableEntity
    {
        [Key]
        public short SKUId { get; set; }

        [Required(ErrorMessage = "SKU Name is required")]
        [Display(Name = "SKU Name")]
        [StringLength(100)]
        public string SKUName { get; set; }

        [Display(Name = "SKU Type")]
        public short? SKUTypeId { get; set; }

        [Display(Name = "Company")]
        public short CompanyId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("SKUTypeId")]
        public virtual SKUType SKUType { get; set; }

        // FIX: Correctly defined the navigation property for Company.
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        public virtual ICollection<SKUClient> SKUClients { get; set; } = new List<SKUClient>();
    }
}
