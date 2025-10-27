using ProcureToPay.Areas.UserManagement.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.UserManagement.ViewModels
{
    public class AdminEditUserViewModel : IUserFormViewModel
    {
        // User entity
        public User User { get; set; }

        // Selected role ID
        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Role")]
        public int? RoleId { get; set; }

        // Dropdown data 
        [ValidateNever]
        public IEnumerable<SelectListItem> RolesList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> BranchesList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> DepartmentsList { get; set; }
        public bool IsLockedOut { get; set; }

        // Constructor
        public AdminEditUserViewModel()
        {
            User = new User();
            RolesList = new List<SelectListItem>();
            BranchesList = new List<SelectListItem>();
            DepartmentsList = new List<SelectListItem>();
        }
    }
}