using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Clients")]
    public class Client : AuditableEntity
    {
        [Key]
        public short ClientId { get; set; }

        [Display(Name = "Account ID")]
        public short? AccountId { get; set; }

        [Required(ErrorMessage = "Client Name is required")]
        [Display(Name = "Client Name")]
        [StringLength(200)]
        public string ClientName { get; set; }

        [Display(Name = "Short Name")]
        [StringLength(50)]
        public string ShortName { get; set; }

        [Display(Name = "Address")]
        [StringLength(500)]
        public string Address { get; set; }

        [Display(Name = "Company")]
        public short? CompanyId { get; set; }

        [Display(Name = "City")]
        public short? CityId { get; set; }

        [Display(Name = "Industry Vertical")]
        public short? IndustryVerticalId { get; set; }

        [Display(Name = "Contract Period")]
        public short? ContractPeriod { get; set; }

        [Display(Name = "Contact Number")]
        [StringLength(50)]
        public string ContactNo { get; set; }

        [Display(Name = "Email")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Display(Name = "Website URL")]
        [StringLength(200)]
        [Url(ErrorMessage = "Invalid URL format")]
        public string URL { get; set; }

        [Display(Name = "Contact Person")]
        [StringLength(100)]
        public string ContactPerson { get; set; }

        [Display(Name = "Payment Mode")]
        public short? PaymentModeId { get; set; }

        [Display(Name = "Credit Limit")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditLimit { get; set; }

        [Display(Name = "Credit Days")]
        public short? CreditDays { get; set; }

        [Display(Name = "Rate Type")]
        public short? RateTypeId { get; set; }

        [Display(Name = "CW Client ID")]
        [StringLength(50)]
        public string CWClientId { get; set; }

        [Display(Name = "NTN")]
        [StringLength(50)]
        public string NTN { get; set; }

        [Display(Name = "STRN")]
        [StringLength(50)]
        public string STRN { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Category Mandatory")]
        public bool CategoryMandatory { get; set; }

        [Display(Name = "Product Mandatory")]
        public bool ProductMandatory { get; set; }

        [Display(Name = "Detention Grace Hours")]
        public short DetGraceHRs { get; set; }

        [Display(Name = "Tax Rate")]
        [Column(TypeName = "decimal(5,2)")]
        public double TaxRate { get; set; }

        // Navigation properties
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        [ForeignKey("CityId")]
        public virtual City City { get; set; }

        [ForeignKey("IndustryVerticalId")]
        public virtual IndustryVertical IndustryVertical { get; set; }

        [ForeignKey("PaymentModeId")]
        public virtual PaymentMode PaymentMode { get; set; }

        [ForeignKey("RateTypeId")]
        public virtual RateType RateType { get; set; }

        // Collection navigation properties
        public virtual ICollection<ClientInvoiceFormat> ClientInvoiceFormats { get; set; } = new List<ClientInvoiceFormat>();
    }
}