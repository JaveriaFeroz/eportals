using ProcureToPay.Areas.UserManagement.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic; // Make sure this is included for List and IEnumerable

namespace ProcureToPay.Areas.UserManagement.ViewModels
{   
    public class AssignUsersViewModel
    {
        public Role Role { get; set; } // Assuming this is your custom role class (e.g., inherits from IdentityRole<int>)
        public IEnumerable<IdentityUser<int>> UsersInRole { get; set; }
        public IEnumerable<IdentityUser<int>> AvailableUsers { get; set; }

        // Crucial for binding selected checkboxes from the form
        public List<int> SelectedUsers { get; set; } // Change to int if your User IDs are int
    }
}