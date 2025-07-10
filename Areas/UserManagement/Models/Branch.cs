using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.UserManagement.Models
{
    public class Branch
    {
        public int BranchId { get; set; }

        [Required(ErrorMessage = "Branch Name is required")]
        [StringLength(100, ErrorMessage = "Branch Name cannot exceed 100 characters")]
        public string BranchName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Branch Code is required.")]
        [StringLength(3, MinimumLength = 2, ErrorMessage = "Branch Code must be between 2 and 3 characters.")]
        [RegularExpression("^[a-zA-Z]{2,3}$", ErrorMessage = "Branch Code must contain only letters and be 2 or 3 characters long.")]
        public string BranchCode { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Foreign key for the admin who created the branch
        public int? CreatedByUserId { get; set; }

        // Navigation property for the creating admin
        //[ForeignKey(nameof(CreatedByUserId))]
        //public virtual User? CreatedByUser { get; set; }

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
