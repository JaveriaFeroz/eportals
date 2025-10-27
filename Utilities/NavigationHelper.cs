using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Areas.UserManagement.Utilities;
using ProcureToPay.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcureToPay.Utilities
{
    public class NavigationHelper
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NavigationHelper(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<NavigationMenuItem>> GetUserMenuItemsAsync()
        {
            var user = _httpContextAccessor.HttpContext.User;
            var menuItems = new List<NavigationMenuItem>();

            // Get all active modules
            var modules = await _context.Modules
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayName)
                .ToListAsync();

            foreach (var module in modules)
            {
                // Check if user has View permission for this module
                var hasViewPermission = await user.HasModulePermissionAsync(
                    module.Name, "View", _context, _userManager);

                if (!hasViewPermission)
                    continue;

                // Get all permissions for this module
                var modulePermissions = await user.GetModulePermissionsAsync(
                    module.Name, _context, _userManager);

                // Determine controller and action based on module name
                var (area, controller, action) = GetModuleRoute(module.Name);

                menuItems.Add(new NavigationMenuItem
                {
                    Title = module.DisplayName,
                    Icon = GetModuleIcon(module.Name),
                    Area = area,
                    Controller = controller,
                    Action = action,
                    ModuleName = module.Name,
                    CanView = modulePermissions.CanView,
                    CanAdd = modulePermissions.CanAdd,
                    CanEdit = modulePermissions.CanEdit,
                    CanDelete = modulePermissions.CanDelete,
                    CanApprove = modulePermissions.CanApprove
                });
            }

            return menuItems;
        }

        private (string Area, string Controller, string Action) GetModuleRoute(string moduleName)
        {
            // Map your module names to controller routes
            return moduleName switch
            {
                "Purchase Request" => ("Procurement", "PurchaseRequest", "Index"),
                "Purchase Order" => ("Procurement", "PurchaseOrder", "Index"),
                "Goods Receipt Note" => ("Procurement", "GoodsReceiptNote", "Index"),
                "Invoice" => ("Finance", "Invoice", "Index"),
                "Payment Request" => ("Finance", "PaymentRequest", "Index"),
                "Vendor Management" => ("Procurement", "Vendor", "Index"),
                "User Management" => ("UserManagement", "Users", "Index"),
                _ => ("", "Home", "Index")
            };
        }

        private string GetModuleIcon(string moduleName)
        {
            // Map your module names to FontAwesome icons
            return moduleName switch
            {
                "Purchase Request" => "fas fa-file-invoice",
                "Purchase Order" => "fas fa-shopping-cart",
                "Goods Receipt Note" => "fas fa-truck-loading",
                "Invoice" => "fas fa-file-invoice-dollar",
                "Payment Request" => "fas fa-money-check-alt",
                "Vendor Management" => "fas fa-building",
                "User Management" => "fas fa-users-cog",
                _ => "fas fa-circle"
            };
        }
    }

    public class NavigationMenuItem
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Area { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string ModuleName { get; set; }

        // Permissions
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }

        public List<NavigationMenuItem> SubItems { get; set; } = new List<NavigationMenuItem>();
    }
}