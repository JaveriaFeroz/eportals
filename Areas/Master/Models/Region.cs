using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Regions")]
    public class Region : AuditableEntity
    {
        [Key]
        public short RegionId { get; set; }

        [Required(ErrorMessage = "Region Name is required")]
        [Display(Name = "Region Name")]
        [StringLength(100)]
        public string RegionName { get; set; }

        [Display(Name = "Tax Rate")]
        [Range(0, 100, ErrorMessage = "Tax Rate must be between 0 and 100")]
        public double TaxRate { get; set; } = 0;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<City> Cities { get; set; } = new List<City>();
    }
}
