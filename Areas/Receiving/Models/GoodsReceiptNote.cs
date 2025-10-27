using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.Common.Services;
using ProcureToPay.Areas.Procurement.Models;
using ProcureToPay.Areas.UserManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using ProcureToPay.Helpers;

namespace ProcureToPay.Areas.Receiving.Models
{
    [Table("GoodsReceiptNotes")]
    public class GoodsReceiptNote : AuditableEntity, IWorkflowEntity
    {
        public int Id { get; set; }

        [StringLength(50)]
        [Display(Name = "GRN Number")]
        public string? GRNNumber { get; set; }

        [Required]
        [Display(Name = "PO Number")]
        public int PurchaseOrderId { get; set; }

        [ForeignKey("PurchaseOrderId")]
        public virtual PurchaseOrder PurchaseOrder { get; set; }

        [Required]
        [Display(Name = "GRN Date")]
        public DateTime ReceiptDate { get; set; } = DateTimeHelper.GetPakistanStandardTime();

        // Added from old GRN
        public short? PrincipalId { get; set; }

        // Added from old GRN
        public string? BillDCNo { get; set; }
        public DateTime? BillDCDate { get; set; }

        // Added from old GRN
        public bool? GRNVaryFromPO { get; set; } = false;
        public double? GRNVaryRate { get; set; }
        public bool IsImported { get; set; } = false;
        public short CurrencyId { get; set; } = 1;
        public double ExRate { get; set; } = 1;

        [StringLength(1000)]
        public string? Remarks { get; set; }

        [Required]
        [Display(Name = "Received By")]
        public int ReceivedByUserId { get; set; }

        [ForeignKey("ReceivedByUserId")]
        public virtual User ReceivedByUser { get; set; }

        public virtual ICollection<FormHistory> FormHistory { get; set; } = new List<FormHistory>();

        // Navigation property for GRN items
        public virtual ICollection<GoodsReceiptNoteItem> Items { get; set; } = new List<GoodsReceiptNoteItem>();

        public short WorkFlowTypeId { get; set; } = (short)ProcureToPay.Areas.Common.Enums.WorkFlowType.GoodsReceiptNote;
        public int FormId => Id;
        public short StateId { get; set; } = WorkflowService.STATE_SAVED;
        public string Owner { get; set; } = string.Empty;
        public short? RequestNatureId { get; set; }
        public short? RequestTypeId { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string? CompanyCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public int CurrentApprovalSequence { get; set; } = 0;
        public bool IsCompleted { get; set; } = false;
        public bool Approved { get; set; } = false;
        public bool Rejected { get; set; } = false;

        public void SyncStatusWithState()
        {
            IsCompleted = true;
            Approved = true;
        }

        public bool IsValid()
        {
            return PurchaseOrderId > 0 &&
                   ReceivedByUserId > 0 &&
                   ReceiptDate != default(DateTime) &&
                   Items.Any(item => item.ReceivedQuantity > 0);
        }
    }
}