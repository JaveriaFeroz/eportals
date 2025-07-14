//using ProcureToPay.Areas.UserManagement.Models;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace ProcureToPay.Areas.Common.Models
//{
//    [Table("WorkFlowApprovalSequence")]
//    public class WorkFlowApprovalSequence
//    {
//        [Key]
//        public int WorkFlowApprovalSeqID { get; set; }

//        [Required]
//        public short WorkFlowTypeId { get; set; }

//        public short? PaymentNatureID { get; set; }
//        public short? PaymentSubNatureID { get; set; }
//        public short? RequestNatureId { get; set; }
//        public short? RequestTypeId { get; set; }

//        [StringLength(50)]
//        public string DepartmentCode { get; set; }

//        [StringLength(50)]
//        public string BranchCode { get; set; }

//        [StringLength(50)]
//        public string CompanyCode { get; set; }

//        public decimal? MinAmount { get; set; }
//        public decimal? MaxAmount { get; set; }

//        [Required]
//        public int RoleID { get; set; }

//        [Required]
//        public int ApprovalSeq { get; set; }

//        public bool IsActive { get; set; } = true;

//        // Navigation properties
//        [ForeignKey("WorkFlowTypeId")]
//        public virtual WorkFlowType WorkFlowType { get; set; }

//        [ForeignKey("RoleID")]
//        public virtual Role Role { get; set; }
//    }
//}
