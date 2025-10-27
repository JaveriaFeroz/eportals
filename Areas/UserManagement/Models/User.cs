using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ProcureToPay.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.UserManagement.Models
    {
        public class User : IdentityUser<int>
        {
            [Required]
            [StringLength(50)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [StringLength(50)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }


            [Display(Name = "Branch")]
            public int? BranchId { get; set; }

            [Display(Name = "Department")]
            public int? DepartmentId { get; set; }

            [Display(Name = "Active")]
            public bool IsActive { get; set; } = true;

            [Display(Name = "Force Password Change")]
            public bool ForcePasswordChange { get; set; } = true;

            [Display(Name = "Created Date")]
            public DateTime CreatedDate { get; set; } = DateTimeHelper.GetPakistanStandardTime();

            [Display(Name = "Last Login")]
            public DateTime? LastLoginDate { get; set; }

            // Navigation properties
            [ForeignKey("BranchId")]
            [ValidateNever]
            public virtual Branch Branch { get; set; }

            [ForeignKey("DepartmentId")]
            [ValidateNever]
            public virtual Department Department { get; set; }
        [ValidateNever]
        public virtual ICollection<UserRole> UserRoles { get; set; }
        }
    }

