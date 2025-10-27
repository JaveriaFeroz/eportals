using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Services")]
    public class Service : AuditableEntity
    {
        [Key]
        public short ServiceId { get; set; }

        [Required(ErrorMessage = "Service Name is required")]
        [Display(Name = "Service Name")]
        [StringLength(200)]
        public string ServiceName { get; set; }

        [Required(ErrorMessage = "Purchase Price is required")]
        [Display(Name = "Purchase Price")]
        public double UnitPrice { get; set; }

        [Display(Name = "UOM")]
        public short? UoMId { get; set; }

        [Display(Name = "Service Nature")]
        public short? ServiceNatureId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [ForeignKey("UoMId")]
        public virtual UoM UoM { get; set; }

        [ForeignKey("ServiceNatureId")]
        public virtual ServiceNature ServiceNature { get; set; }
    }
}