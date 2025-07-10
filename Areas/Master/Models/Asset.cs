using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Assets")]
    public class Asset : AuditableEntity
    {
        [Key]
        public short AssetId { get; set; }

        [Required(ErrorMessage = "Asset Number is required")]
        [Display(Name = "Asset Number")]
        [StringLength(50)]
        public string AssetNo { get; set; }

        [Required]
        [Display(Name = "Asset Type")]
        public short AssetTypeId { get; set; }

        [Display(Name = "Capacity")]
        public short? CapacityId { get; set; }

        [Display(Name = "Make")]
        public short? MakeId { get; set; }

        [Display(Name = "Model")]
        [StringLength(100)]
        public string? Model { get; set; }

        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseDate { get; set; }

        [Display(Name = "Lease Type")]
        public short LeaseTypeId { get; set; }

        [Display(Name = "Supplier")]
        public short? SupplierId { get; set; }

        [Display(Name = "Start KMs")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal StartKMs { get; set; }

        [Display(Name = "KMs")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal KMs { get; set; }

        [Display(Name = "Status")]
        public short StatusId { get; set; }

        [Display(Name = "Driver 1")]
        public short? DriverId1 { get; set; }

        [Display(Name = "Driver 2")]
        public short? DriverId2 { get; set; }

        [Display(Name = "Trailer")]
        public short? TrailerId { get; set; }

        [Display(Name = "FA Code")]
        [StringLength(50)]
        public string? FACode { get; set; }

        [Required]
        [Display(Name = "City")]
        public short CityId { get; set; }

        [Display(Name = "Client")]
        public short? ClientId { get; set; }

        [Display(Name = "Base")]
        public short? BaseId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Company")]
        public short CompanyId { get; set; }

        // Navigation properties
        [ForeignKey("AssetTypeId")]
        public virtual AssetType AssetType { get; set; }

        [ForeignKey("CapacityId")]
        public virtual Capacity CapacityName { get; set; }

        [ForeignKey("MakeId")]
        public virtual Make MakeName { get; set; }

        [ForeignKey("LeaseTypeId")]
        public virtual LeaseType LeaseTypeName { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier SupplierName { get; set; }

        [ForeignKey("StatusId")]
        public virtual AssetStatus AssetStatus { get; set; }

        [ForeignKey("DriverId1")]
        public virtual Driver DriverName1 { get; set; }

        [ForeignKey("DriverId2")]
        public virtual Driver DriverName2 { get; set; }
        [ForeignKey("CityId")]
        public virtual City CityName { get; set; }
        [ForeignKey("ClientId")]
        public virtual Client ClientName { get; set; }
        [ForeignKey("BaseId")]
        public virtual Base BaseName { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company CompanyName { get; set; }

        [ForeignKey("TrailerId")]
        public virtual Trailer TrailerName { get; set; }
        public virtual ICollection<AssetTyre> AssetTyres { get; set; } = new List<AssetTyre>();
    }
}
