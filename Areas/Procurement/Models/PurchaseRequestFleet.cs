using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Procurement.Enums;
using ProcureToPay.Areas.Common.Enums;
using ProcureToPay.Areas.UserManagement.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using WorkFlowTypeEnum = ProcureToPay.Areas.Common.Enums.WorkFlowType;

namespace ProcureToPay.Areas.Procurement.Models
{
    [Table("PurchaseRequestsFleet")]
    public class PurchaseRequestsFleet : AuditableEntity, IWorkflowEntity, IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "PR No.")]
        public int? PRNo { get; set; }

        [Required(ErrorMessage = "Company Code is required")]
        [StringLength(50)]
        public string CompanyCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Branch Code is required")]
        [StringLength(50)]
        public string BranchCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department Code is required")]
        [StringLength(50)]
        public string DepartmentCode { get; set; } = string.Empty;

        [Display(Name = "Required By")]
        [Required(ErrorMessage = "Required By date is required")]
        public DateTime? RequiredBy { get; set; }

        [Required]
        public short StateId { get; set; } = 1; // Default to STATE_SAVED

        [Display(Name = "State")]
        public string StateName { get { return ((PurchaseRequestState)StateId).ToString(); } }

        [Display(Name = "Product Nature")]
        public short? ProductNatureId { get; set; }

        [ForeignKey("ProductNatureId")]
        public virtual ProductNature ProductNature { get; set; }

        [Display(Name = "Service Nature")]
        public short? ServiceNatureId { get; set; }

        [ForeignKey("ServiceNatureId")]
        public virtual ServiceNature ServiceNature { get; set; }

        public short? RequestNatureId { get; set; }
        public short? RequestTypeId { get; set; }

        [Required]
        public short WorkFlowId { get; set; }

        [Required(ErrorMessage = "Owner is required")]
        [StringLength(100)]
        public string Owner { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;
        public bool Approved { get; set; } = false;
        public bool Rejected { get; set; } = false;
        public bool Budgeted { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Budget Amount")]
        public decimal? BudgetAmount { get; set; }

        [StringLength(500)]
        [Display(Name = "Budget Remarks")]
        public string? BudgetRemarks { get; set; }

        [Required(ErrorMessage = "Justification is required")]
        [StringLength(1000, ErrorMessage = "Justification cannot exceed 1000 characters")]
        public string Justification { get; set; } = string.Empty;

         [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
    

        // Navigation properties
        public virtual ICollection<PurchaseRequestDetailFleet> Details { get; set; } = new List<PurchaseRequestDetailFleet>();
        

        #region Supplementary fields for display (not mapped to database)
        [NotMapped]
        [Display(Name = "Branch Name")]
        public string? BranchName { get; set; }

        [NotMapped]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [NotMapped]
        [Display(Name = "Department Name")]
        public string? DepartmentName { get; set; }

        [NotMapped]
        [Display(Name = "Product Nature Name")]
        public string? ProductNatureName { get; set; }

        [NotMapped]
        [Display(Name = "Service Nature Name")]
        public string? ServiceNatureName { get; set; }
        #endregion

        #region Section for uploading new attachments
        [NotMapped]
        public bool AddNew { get; set; }

        [NotMapped]
        public short? AttachmentTypeId { get; set; }
        #endregion

        // --- IWorkflowEntity Implementation ---
        public short WorkFlowTypeId { get; set; } = (short)WorkFlowTypeEnum.PurchaseRequestFleet;

        // Map FormId to Id
        public int FormId => Id;

        // Workflow-specific properties
        public int CurrentApprovalSequence { get; set; } = 0;

        // Custom validation method using IValidatableObject
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Add fleet-specific validations here
            if (RequiredBy.HasValue && RequiredBy.Value <= DateTime.Now)
            {
                yield return new ValidationResult(
                    "Required By date must be in the future.",
                    new[] { nameof(RequiredBy) }
                );
            }

            if (ProductNatureId.HasValue && ServiceNatureId.HasValue)
            {
                yield return new ValidationResult(
                    "Cannot select both Product Nature and Service Nature. Please select only one.",
                    new[] { nameof(ProductNatureId), nameof(ServiceNatureId) }
                );
            }

            if (!ProductNatureId.HasValue && !ServiceNatureId.HasValue)
            {
                yield return new ValidationResult(
                    "Either Product Nature or Service Nature must be selected.",
                    new[] { nameof(ProductNatureId), nameof(ServiceNatureId) }
                );
            }
        }
    }

    
    

    // Fleet-specific attachment model
   

}