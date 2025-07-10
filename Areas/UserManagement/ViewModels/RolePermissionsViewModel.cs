using ProcureToPay.Areas.UserManagement.Models;
using System.Collections.Generic;

namespace ProcureToPay.Areas.UserManagement.ViewModels
{
    public class RolePermissionSummaryViewModel
    {
        public Role Role { get; set; }
        public int PermissionCount { get; set; }
    }

    public class ManageRolePermissionsViewModel
    {
        public Role Role { get; set; }
        public List<string> Modules { get; set; }
        public List<Permission> Permissions { get; set; }
        public List<int> AssignedPermissionIds { get; set; }
    }

    public class CompareRolesViewModel
    {
        public List<Role> Roles { get; set; }
        public List<Permission> AllPermissions { get; set; }
        public Dictionary<int, HashSet<int>> RolePermissionMap { get; set; }
    }

    public class CopyPermissionsViewModel
    {
        public List<Role> SourceRoles { get; set; }
        public List<Role> TargetRoles { get; set; }
        public int SelectedSourceRoleId { get; set; }
        public List<int> SelectedTargetRoleIds { get; set; }
    }
}