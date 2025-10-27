using Microsoft.AspNetCore.Mvc.ModelBinding;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Common.Models
{
    public abstract class AuditableEntity
    {
        // Audit fields with User references
        [BindNever]
        [Display(Name = "Created By")]
        public int CreatedByUserId { get; set; }
        [BindNever]

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; }
        [BindNever]

        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; } = DateTimeHelper.GetPakistanStandardTime();
        [BindNever]

        [Display(Name = "Updated By")]
        public int? UpdatedByUserId { get; set; }
        [BindNever]

        [ForeignKey("UpdatedByUserId")]
        public virtual User UpdatedByUser { get; set; }
        [BindNever]

        [Display(Name = "Updated On")]
        public DateTime? UpdatedOn { get; set; }
       
    }
}
