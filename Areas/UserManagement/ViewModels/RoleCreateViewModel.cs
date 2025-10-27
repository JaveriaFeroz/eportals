using ProcureToPay.Areas.UserManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.UserManagement.ViewModels
{
    public class RoleCreateViewModel
    {
        [Required(ErrorMessage = "Role name is required")]
        [Display(Name = "Role Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }


        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    public class RoleEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Role name is required")]
        [Display(Name = "Role Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }

   

  

 
}