using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("Qualifications")]
    [DataContract]
    public class Qualification : AuditableEntity
    {
        [Key]
        [DataMember(Order = 0)]
        public short QualificationId { get; set; }

        [Required(ErrorMessage = "Qualification Name is required")]
        [Display(Name = "Qualification Name")]
        [StringLength(100)]
        [DataMember(Order = 1)]
        public string QualificationName { get; set; }

        [Display(Name = "Active")]
        [DataMember(Order = 2)]
        public bool IsActive { get; set; } = true;
    }

}
