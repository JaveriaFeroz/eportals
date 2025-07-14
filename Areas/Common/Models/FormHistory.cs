//using ProcureToPay.Areas.UserManagement.Models;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace ProcureToPay.Areas.Common.Models
//{
//    [Table("FormHistory")]
//    public class FormHistory
//    {
//        [Key]
//        public int FormHistoryId { get; set; }

//        [Required]
//        public short WorkFlowTypeId { get; set; }

//        [Required]
//        public int FormId { get; set; } // This will be PRNo, PaymentRequestId, etc.

//        [Required]
//        public short FromStateId { get; set; }

//        [Required]
//        public short ToStateId { get; set; }
//        public string FromStateName { get; set; }
//        public string ToStateName { get; set; }
//        [Required, StringLength(50)]
//        public string Action { get; set; } // e.g., "Created", "Submitted", "Approved", "Rejected"

//        [StringLength(1000)]
//        public string Comments { get; set; }

//        [Required]
//        public int ActionByUserId { get; set; }

//        [StringLength(100)]
//        public string ActionByUserName { get; set; }

//        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

//        // Navigation properties
//        [ForeignKey("WorkFlowTypeId")]
//        public virtual WorkFlowType WorkFlowType { get; set; }

//        [ForeignKey("ActionByUserId")]
//        public virtual User ActionByUser { get; set; }
//    }
//}
