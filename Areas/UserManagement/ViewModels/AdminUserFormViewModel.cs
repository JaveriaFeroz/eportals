using ProcureToPay.Areas.UserManagement.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.UserManagement.ViewModels
{
    public class AdminUserFormViewModel : IUserFormViewModel
    {
        // The User entity
        public User User { get; set; }

        // Admin-assigned password (not part of User entity)
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        // Selected role ID for this user
        [Required(ErrorMessage = "User must be assigned a role")]
        [Display(Name = "Role")]
        public int? RoleId { get; set; }

        // Dropdown data collections
        public IEnumerable<SelectListItem> RolesList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> BranchesList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> DepartmentsList { get; set; } = new List<SelectListItem>();


        // Constructor to initialize collections and User
        public AdminUserFormViewModel()
        {
            User = new User { IsActive = true };
            RolesList = new List<SelectListItem>();
            BranchesList = new List<SelectListItem>();
            DepartmentsList = new List<SelectListItem>();
        }
    }
}