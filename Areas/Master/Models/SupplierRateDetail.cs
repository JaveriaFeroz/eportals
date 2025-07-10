using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("SupplierRateDetails")]
    public class SupplierRateDetail : AuditableEntity
    {
        [Key]
        public int DetailId { get; set; }

        [Required]
        public int SupplierRateId { get; set; }

        [Required(ErrorMessage = "From Date is required")]
        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "To Date is required")]
        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }

        [Required(ErrorMessage = "Fuel Rate is required")]
        [Display(Name = "Fuel Rate")]
        [Range(0, double.MaxValue, ErrorMessage = "Fuel Rate must be greater than 0")]
        public double FuelRate { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        [ForeignKey("SupplierRateId")]
        public virtual SupplierRate SupplierRate { get; set; }
    }
}
