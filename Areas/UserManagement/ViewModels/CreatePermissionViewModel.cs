using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProcureToPay.Areas.UserManagement.Models.ViewModels
{
    public class CreatePermissionViewModel
    {
        [Required(ErrorMessage = "Permission name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Module is required")]
        [Display(Name = "Module")]
        public int ModuleId { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
        public string Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // For the dropdown
        public List<SelectListItem> ModuleOptions { get; set; } = new List<SelectListItem>();
    }
}