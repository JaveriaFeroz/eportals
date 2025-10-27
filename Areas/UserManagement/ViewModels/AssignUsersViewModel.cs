using ProcureToPay.Areas.UserManagement.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic; // Make sure this is included for List and IEnumerable

namespace ProcureToPay.Areas.UserManagement.ViewModels
{
    public class AssignUsersViewModel
    {
        public Role Role { get; set; }
        // Change the types from IdentityUser<int> to your custom User class
        public IEnumerable<User> UsersInRole { get; set; }
        public IEnumerable<User> AvailableUsers { get; set; }

        // Crucial for binding selected checkboxes from the form
        public List<int> SelectedUsers { get; set; }
    }
}