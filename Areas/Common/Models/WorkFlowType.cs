using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Common.Models
{
    [Table("WorkFlowType")]
    public class WorkFlowType
    {
        [Key]
        public int WorkFlowTypeId { get; set; }

        [Required, StringLength(100)]
        public string WorkFlowName { get; set; }

        [Required, StringLength(10)]
        public string WorkFlowShortName { get; set; }

        public short WorkFlowGroupId { get; set; }

        public DateTime CreatedOn { get; set; } = DateTimeHelper.GetPakistanStandardTime();

        // Navigation properties
        public virtual Module Module { get; set; }
        public virtual ICollection<WorkFlowState> States { get; set; } = new List<WorkFlowState>();
        public virtual ICollection<WorkFlowApprovalSequence> ApprovalSequences { get; set; } = new List<WorkFlowApprovalSequence>();
    }
}
