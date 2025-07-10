using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.Master.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Insurance.Models
{
    // Entity Model
    [Table("InsuranceTypes")]
    public class InsuranceType : AuditableEntity
    {
        [Key]
        public short TypeId { get; set; }

        [Required(ErrorMessage = "Type Name is required")]
        [Display(Name = "Type Name")]
        [StringLength(100)]
        public string TypeName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Required]
        public short CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        // Navigation properties
       // public virtual ICollection<InsuranceTypeClient> TypeClients { get; set; } = new List<InsuranceTypeClient>();
    }
}