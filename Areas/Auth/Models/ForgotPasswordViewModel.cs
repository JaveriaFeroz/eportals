using ProcureToPay.Areas.UserManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Auth.Models
{
    public class ForgotPasswordViewModel : AuthViewModelBase
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}
