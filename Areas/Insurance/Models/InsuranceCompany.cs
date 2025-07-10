using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Insurance.Models
{
    [Table("InsuranceCompanies")]
    public class InsuranceCompany : AuditableEntity
    {
        [Key]
        public short CompanyId { get; set; }

        [Required(ErrorMessage = "Insurance Company Name is required")]
        [Display(Name = "Insurance Company Name")]
        [StringLength(100)]
        public string CompanyName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}