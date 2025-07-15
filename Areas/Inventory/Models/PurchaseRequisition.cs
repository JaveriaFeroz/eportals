using ProcureToPay.Areas.Common.Models; // Assuming AuditableEntity is here
using ProcureToPay.Areas.Master.Models; // Assuming related entities like Branch, Department, etc. are here
using ProcureToPay.Areas.UserManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Inventory.Models
{
    [Table("PurchaseRequisitions")]
    public class PurchaseRequisition : AuditableEntity
    {
        [Key]
        [Display(Name = "PR No.")]
        public int PRNo { get; set; } // Not nullable (Primary Key)

        [Required(ErrorMessage = "Company Code is required")]
        [Display(Name = "Company Code")]
        [StringLength(50)]
        public string CompanyCode { get; set; } // Not nullable

        [Display(Name = "Branch")]
        public int BranchId { get; set; } // Nullable

        [Display(Name = "Department")]
        public int DepartmentId { get; set; } // Nullable

        [Display(Name = "Required By")]
        [DataType(DataType.Date)]
        public DateTime? RequiredBy { get; set; } // Nullable

        [Display(Name = "Current State")]
        public short? StateId { get; set; } // Nullable

        [Display(Name = "Product Nature")]
        public short? ProductNatureId { get; set; } // Nullable
         
        [Display(Name = "Service Nature")]
        public short? ServiceNatureId { get; set; } // Nullable

        [Display(Name = "Request Type")]
        public short? RequestTypeId { get; set; } // Nullable

        [Display(Name = "Workflow")]
        public short? WorkFlowId { get; set; } // Nullable

        [Display(Name = "Owner")]
        [StringLength(100)]
        public string? Owner { get; set; } // Nullable

        [Display(Name = "Completed")]
        public bool? IsCompleted { get; set; } // Nullable

        [Display(Name = "Approved")]
        public bool? Approved { get; set; } // Nullable

        [Display(Name = "Rejected")]
        public bool? Rejected { get; set; } // Nullable

        [Display(Name = "Budgeted")]
        public bool? Budgeted { get; set; } // Nullable

        [Display(Name = "Budget Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? BudgetAmount { get; set; } // Nullable

        [Display(Name = "Budget Remarks")]
        [StringLength(500)]
        public string? BudgetRemarks { get; set; } // Nullable

        [Required(ErrorMessage = "Justification is required")]
        [Display(Name = "Justification")]
        [StringLength(1000)]
        public string Justification { get; set; } // Not nullable

        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [ForeignKey("ProductNatureId")]
        public virtual ProductNature ProducNature { get; set; }

        [ForeignKey("ServiceNatureId")]
        public virtual ServiceNature ServiceNature { get; set; }

        //[ForeignKey("StateId")]
        //public virtual State State { get; set; }

         public virtual ICollection<PurchaseRequisitionDetail> Details { get; set; }
    }
}