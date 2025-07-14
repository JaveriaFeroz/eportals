//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace ProcureToPay.Areas.Common.Models
//{
//    [Table("WorkFlowState")]
//    public class WorkFlowState
//    {
//        [Key]
//        public int WorkFlowStateId { get; set; }

//        [Required]
//        public short WorkFlowTypeId { get; set; }

//        [Required]
//        public short StateId { get; set; }

//        [Required, StringLength(100)]
//        public string StateName { get; set; }

//        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

//        // Navigation properties
//        [ForeignKey("WorkFlowTypeId")]
//        public virtual WorkFlowType WorkFlowType { get; set; }
//    }
//}
