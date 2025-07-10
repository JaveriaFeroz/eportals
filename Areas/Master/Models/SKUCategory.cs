using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{
    // Entity Model
    [Table("SKUCategories")]
    public class SKUCategory : AuditableEntity
    {
        [Key]
        public short CategoryId { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        [Display(Name = "Category Name")]
        [StringLength(100)]
        public string CategoryName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Required]
        public short CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        // Navigation properties
        public virtual ICollection<SKUCategoryClient> CategoryClients { get; set; } = new List<SKUCategoryClient>();
    }
}