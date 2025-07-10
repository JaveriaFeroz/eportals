using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Shippers")]
    public class Shipper : AuditableEntity
    {
        [Key]
        public int ShipperId { get; set; }

        [Required(ErrorMessage = "Shipper Name is required")]
        [Display(Name = "Shipper Name")]
        [StringLength(100)]
        public string ShipperName { get; set; }

        [Display(Name = "Address")]
        [StringLength(255)]
        public string Address { get; set; }

        [Display(Name = "City")]
        public short? CityId { get; set; }

        [Display(Name = "Client")]
        public short? ClientId { get; set; }

        [Display(Name = "Contact Number")]
        [StringLength(50)]
        public string ContactNo { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Add the missing CompanyId property
        [Display(Name = "Company")]
        public short CompanyId { get; set; }

        // Navigation properties
        [ForeignKey("CityId")]
        public virtual City City { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
    }
}