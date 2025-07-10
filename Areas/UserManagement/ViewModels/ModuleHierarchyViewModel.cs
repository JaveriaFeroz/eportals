using ProcureToPay.Areas.UserManagement.Models;

namespace ProcureToPay.Areas.UserManagement.ViewModels
{
    public class ModuleHierarchyViewModel
    {
        public string ModuleName { get; set; }
        public string ModuleDisplayName { get; set; }
        public IEnumerable<ModuleRoleHierarchy> RoleHierarchies { get; set; }
        public IEnumerable<Role> AvailableRoles { get; set; }
    }
}
