//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace ProcureToPay.Areas.Common.Models
//{
//    [Table("WorkFlowType")]
//    public class WorkFlowType
//    {
//        [Key]
//        public short WorkFlowTypeId { get; set; }

//        [Required, StringLength(100)]
//        public string WorkFlowName { get; set; }

//        [Required, StringLength(10)]
//        public string WorkFlowShortName { get; set; }

//        public short WorkFlowGroupId { get; set; }

//        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

//        // Navigation properties
//        public virtual ICollection<WorkFlowState> States { get; set; } = new List<WorkFlowState>();
//        public virtual ICollection<WorkFlowApprovalSequence> ApprovalSequences { get; set; } = new List<WorkFlowApprovalSequence>();
//    }
//}
