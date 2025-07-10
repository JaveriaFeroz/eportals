using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    [Table("SubCategories")]
    public class SubCategory : AuditableEntity
    {
        [Key]
        public short SubCategoryId { get; set; }

        [Required(ErrorMessage = "Sub Category Name is required")]
        [Display(Name = "Sub Category Name")]
        [StringLength(100)]
        public string SubCategoryName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}