using Microsoft.AspNetCore.Identity;

namespace ProcureToPay.Areas.UserManagement.Models
{
    public class Role : IdentityRole<int>
    {
        // Basic properties
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Default hierarchy level (for backward compatibility)
        public int HierarchyLevel { get; set; }

        // Collection of module-specific hierarchy levels
        public virtual ICollection<ModuleRoleHierarchy> ModuleHierarchies { get; set; } = new List<ModuleRoleHierarchy>();


        // Collection of role permissions
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        // Add this navigation property for user roles
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        // Constructors to ensure compatibility
        public Role() : base() { }
        public Role(string roleName) : base(roleName)
        {
            Name = roleName;
        }
    }

    // New entity to store module-specific hierarchy levels
    public class ModuleRoleHierarchy
    {
        public int Id { get; set; }

        // Foreign key to Role
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }

        // Module identifier 
        public string ModuleName { get; set; }

        // Hierarchy level within this specific module
        public int HierarchyLevel { get; set; }
    }

    //  Module class to track available modules
    public class Module
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
