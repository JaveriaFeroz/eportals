using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Cities")]
    public class City : AuditableEntity
    {
        [Key]
        public short CityId { get; set; }


        [Required(ErrorMessage = "City Code is required.")]
        [Display(Name = "City Code")]
        [StringLength(3, MinimumLength = 2, ErrorMessage = "City Code must be between 2 and 3 characters.")]
        [RegularExpression("^[a-zA-Z]{2,3}$", ErrorMessage = "City Code must contain only letters and be 2 or 3 characters long.")]
        public string CityCode { get; set; }

        [Required(ErrorMessage = "City Name is required")]
        [Display(Name = "City Name")]
        [StringLength(100)]
        public string CityName { get; set; }

        [Required(ErrorMessage = "Region is required")]
        [Display(Name = "Region")]
        public short RegionId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey("RegionId")]
        public virtual Region Region { get; set; }
    }
}