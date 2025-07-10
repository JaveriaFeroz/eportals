using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.Master.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Consignees")]
    public class Consignee : AuditableEntity
    {
        [Key]
        public int ConsigneeId { get; set; }

        [Required(ErrorMessage = "Consignee Name is required")]
        [Display(Name = "Consignee Name")]
        [StringLength(200)]
        public string ConsigneeName { get; set; }

        [Display(Name = "City")]
        public short? CityId { get; set; }

        [Display(Name = "Client")]
        public short? ClientId { get; set; }

        [Display(Name = "Contact Number")]
        [StringLength(50)]
        public string ContactNo { get; set; }

        [Display(Name = "Address")]
        [StringLength(500)]
        public string Address { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Last Delivery KMs")]
        public double LastDeliveryKMs { get; set; }

        [Display(Name = "Last Departure Date")]
        public DateTime? LastDepartureDate { get; set; }

        [Display(Name = "Last Departure Time")]
        public DateTime? LastDepartureTime { get; set; }

        [Display(Name = "Last Departure DateTime")]
        public DateTime? LastDepartureDateTime { get; set; }

        [Display(Name = "Standard KMs")]
        public double? StandardKMs { get; set; }

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