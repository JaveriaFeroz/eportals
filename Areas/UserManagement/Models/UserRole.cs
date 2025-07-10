using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProcureToPay.Areas.UserManagement.Models
{
    public class UserRole : IdentityUserRole<int>
    {
        // Navigation properties
        public virtual User User { get; set; }
        
        public virtual Role Role { get; set; }
    }
}
