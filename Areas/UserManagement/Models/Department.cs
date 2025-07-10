using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.UserManagement.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Department Name is required")]
        [StringLength(100, ErrorMessage = "Department Name cannot exceed 100 characters")]
        public string DepartmentName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Department Code is required.")]
        [StringLength(3, MinimumLength = 2, ErrorMessage = "Department Code must be between 2 and 3 characters.")]
        [RegularExpression("^[a-zA-Z]{2,3}$", ErrorMessage = "Department Code must contain only letters and be 2 or 3 characters long.")]
        public string DepartmentCode { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Foreign key for the admin who created the department
        public int? CreatedByUserId { get; set; }

        //// Navigation property for the creating admin
        //[ForeignKey(nameof(CreatedByUserId))]
        //public virtual User? CreatedByUser { get; set; }

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
