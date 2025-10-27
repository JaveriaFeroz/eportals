using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.Procurement.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Suppliers")]
    public class Supplier : AuditableEntity
    {
        [Key]
        public short SupplierId { get; set; }

        [Required(ErrorMessage = "Supplier Type is required")]
        [Display(Name = "Supplier Type")]
        public short SupplierTypeId { get; set; }

        [Required(ErrorMessage = "Supplier Name is required")]
        [Display(Name = "Supplier Name")]
        [StringLength(200)]
        public string SupplierName { get; set; }

        [Display(Name = "SC Rate")]
        [Range(0, double.MaxValue, ErrorMessage = "SC Rate must be a positive number")]
        public double SCRate { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Address")]
        [StringLength(500)]
        public string Address { get; set; }

        [Display(Name = "City")]
        public short? CityId { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Display(Name = "Phone Number")]
        [StringLength(20)]
        public string? PhoneNo { get; set; }

        [Display(Name = "Fax Number")]
        [StringLength(20)]
        public string? FaxNo { get; set; }

        [Display(Name = "Contact Name")]
        [StringLength(100)]
        public string? ContactName { get; set; }

        [Display(Name = "Mobile Number")]
        [StringLength(20)]
        public string? MobileNo { get; set; }

        [Display(Name = "NTN")]
        [StringLength(50)]
        public string? NTN { get; set; }

        [Display(Name = "URL")]
        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(200)]
        public string? URL { get; set; }

        [Display(Name = "Control Supplier ID")]
        [StringLength(50)]
        public string? ControlSupplierId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("SupplierTypeId")]
        public virtual SupplierType? SupplierType { get; set; }

        [ForeignKey("CityId")]
        public virtual City? City { get; set; }
      
        public virtual ICollection<SupplierRate> SupplierRates { get; set; } = new List<SupplierRate>();

        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}
