using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Companies")]
    public class Company : AuditableEntity
    {
        [Key]
        public short CompanyId { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        [Display(Name = "Company Name")]
        [StringLength(100)]
        public string CompanyName { get; set; }

        [Display(Name = "Company Address")]
        [StringLength(500)]
        public string CompanyAddress { get; set; }

        [Display(Name = "NTN")]
        [StringLength(50)]
        public string NTN { get; set; }

        [Display(Name = "Period Name")]
        [StringLength(100)]
        public string? PeriodName { get; set; }

        [Display(Name = "Distance Threshold")]
        public short? DistanceThreshold { get; set; }

        [Display(Name = "Report Grace Hours")]
        public short? ReportGraceHRs { get; set; }

        [Display(Name = "Bank Account")]
        public short? BankAccountId { get; set; }

        [Display(Name = "AR Period")]
        public short? ARPeriodId { get; set; }

        [Display(Name = "AR Account")]
        public short? ARAccountId { get; set; }

        [Display(Name = "AP Period")]
        public short? APPeriodId { get; set; }

        [Display(Name = "AP Account")]
        public short? APAccountId { get; set; }

        [Display(Name = "GL Period")]
        public short? GLPeriodId { get; set; }

        [Display(Name = "Ops Period")]
        public short? OpsPeriodId { get; set; }

        [Display(Name = "Trip Revenue Account")]
        public short? TripRevenueAccountId { get; set; }

        [Display(Name = "Fuel Expense Account")]
        public short? FuelExpenseAccountId { get; set; }

        [Display(Name = "Advance Account")]
        public short? AdvanceAccountId { get; set; }

        [Display(Name = "Enable GL")]
        public bool EnableGL { get; set; }

        [Display(Name = "Enable Partial Delivery")]
        public bool EnablePartialDelivery { get; set; }

        [Display(Name = "Route By Consignee")]
        public bool RouteByConsignee { get; set; }

        [Display(Name = "Separate Fixed Invoice")]
        public bool SeparateFixedInvoice { get; set; }

        [Display(Name = "Mandatory Driver 2")]
        public bool IsMandatoryDriver2 { get; set; }

        [Display(Name = "Allow Trailer")]
        public bool AllowTrailer { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }


}