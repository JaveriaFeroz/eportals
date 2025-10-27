using ProcureToPay.Areas.UserManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Auth.Models
{
    public class LoginViewModel : AuthViewModelBase
    {
        [Required(ErrorMessage = "The Username field is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "The Password field is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
