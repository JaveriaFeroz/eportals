
using ProcureToPay.Areas.UserManagement.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProcureToPay.Areas.UserManagement.ViewModels
{
    // Interface for common view model properties
    public interface IUserFormViewModel
    {
        User User { get; set; }
        int? RoleId { get; set; }
        IEnumerable<SelectListItem> RolesList { get; set; }
        IEnumerable<SelectListItem> BranchesList { get; set; }
        IEnumerable<SelectListItem> DepartmentsList { get; set; }
    }

    // View model for the Users Index page
    public class UsersIndexViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public Dictionary<int, IList<string>> UserRoles { get; set; }
    }

    // View model for user details
    public class UserDetailsViewModel
    {
        public User User { get; set; }
        public IList<string> Roles { get; set; }
    }

    // Debug view model for system diagnostics
    public class DebugViewModel
    {
        public string DbConnectionStatus { get; set; }
        public bool IdentityEnabled { get; set; }

        public int UserCount { get; set; }
        public bool UserTableAccessible { get; set; }
        public string UserTableError { get; set; }

        public int RoleCount { get; set; }
        public bool RoleTableAccessible { get; set; }
        public string RoleTableError { get; set; }

        public int BranchCount { get; set; }
        public bool BranchTableAccessible { get; set; }
        public string BranchTableError { get; set; }

        public int DeptCount { get; set; }
        public bool DeptTableAccessible { get; set; }
        public string DeptTableError { get; set; }
    }
}