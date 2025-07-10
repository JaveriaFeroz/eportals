using ProcureToPay.Areas.UserManagement.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace ProcureToPay.Areas.UserManagement.ViewModels
{
    public class AssignPermissionToRolesViewModel
    {
        public Permission Permission { get; set; }
        public List<Role> Roles { get; set; }
        public List<int> AssignedRoleIds { get; set; }
    }

    public class RolePermissionsViewModel
    {
        public Role Role { get; set; }
        public List<Permission> AvailablePermissions { get; set; }
        public List<Permission> AssignedPermissions { get; set; }
    }
}