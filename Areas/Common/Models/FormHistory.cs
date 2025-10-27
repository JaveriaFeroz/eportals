using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Helpers;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Common.Models

{

    [Table("FormHistory")]

    public class FormHistory

    {

        [Key]

        public int FormHistoryId { get; set; }

        [Required]

        public short WorkFlowTypeId { get; set; }

        [Required]

        public int FormId { get; set; }

        [Required]

        public short FromStateId { get; set; }

        [Required]

        public short ToStateId { get; set; }

        public string FromStateName { get; set; }

        public string ToStateName { get; set; }

        [Required, StringLength(50)]

        public string Action { get; set; } 

        [StringLength(1000)]

        public string Comments { get; set; }

        [Required]

        public int ActionByUserId { get; set; }

        [StringLength(100)]

        public string ActionByUserName { get; set; }


        public int? ToUserId { get; set; } // The user who will receive/handle this in the next state


        [StringLength(100)]

        public string? ToUserName { get; set; }

        public DateTime ActionDate { get; set; } = DateTimeHelper.GetPakistanStandardTime();

        // Navigation properties

        [ForeignKey("WorkFlowTypeId")]

        public virtual WorkFlowType WorkFlowType { get; set; }

        [ForeignKey("ActionByUserId")]

        public virtual User ActionByUser { get; set; }

        // ADD THIS NAVIGATION PROPERTY

        [ForeignKey("ToUserId")]

        public virtual User ToUser { get; set; }

    }

}
