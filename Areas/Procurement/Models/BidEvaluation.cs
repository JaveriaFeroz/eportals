using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.UserManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Procurement.Models
{
    [Table("BidEvaluations", Schema = "Procurement")]
    public class BidEvaluation : AuditableEntity, IWorkflowEntity
    {
        [Key]
        public int BidNo { get; set; } 

        [Required]
        [StringLength(50)]
        [Display(Name = "Bid Evaluation Number")]
        public string BidEvaluationNumber { get; set; } = string.Empty; // BE-20250101-001

        [Required]
        [Display(Name = "Purchase Request")]
        public int PRNo { get; set; } // Foreign key to PurchaseRequest

        [ForeignKey("PRNo")]
        public virtual PurchaseRequest? PurchaseRequest { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Justification is required")]
        [StringLength(2000)]
        [Display(Name = "Justification")]
        public string Justification { get; set; } = string.Empty;

        [Display(Name = "Evaluation Date")]
        public DateTime EvaluationDate { get; set; } = DateTime.Now;

        [Display(Name = "Submission Deadline")]
        public DateTime? SubmissionDeadline { get; set; }

        [Display(Name = "Estimated Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedAmount { get; set; }

        // Winner Selection
        public int? SelectedBidId { get; set; }

        [ForeignKey("SelectedBidId")]
        public virtual Bid? SelectedBid { get; set; }

        [StringLength(1000)]
        public string? SelectionJustification { get; set; }

        // IWorkflowEntity Implementation
        public short WorkFlowTypeId { get; set; } = (short)ProcureToPay.Areas.Common.Enums.WorkFlowType.BidEvaluation;
        public int FormId => BidNo;
        public short StateId { get; set; } = 1; // 1=New/Draft, 2=Submitted, 3=Completed, 4=Approved

        [Required]
        [StringLength(100)]
        public string Owner { get; set; } = string.Empty;

        public short? RequestNatureId { get; set; }
        public short? RequestTypeId { get; set; }

        // DEPARTMENT CONTEXT (from user's department)
        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        // BRANCH CONTEXT (from user's branch)
        [Display(Name = "Branch")]
        public int? BranchId { get; set; }

        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string? CompanyCode { get; set; } = string.Empty;

        public int CurrentApprovalSequence { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
        public bool Approved { get; set; } = false;
        public bool Rejected { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

        public void SyncStatusWithState()
        {
            IsCompleted = StateId == 3 || StateId == 4; // Completed or Approved
        }
    }



   
}