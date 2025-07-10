using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Areas.UserManagement.Utilities;
using ProcureToPay.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ProcureToPay.Areas.UserManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        private readonly string _permissionName;
        private readonly string _moduleName;

        public RequirePermissionAttribute(string permissionName)
        {
            _permissionName = permissionName;
            _moduleName = null;
        }

        public RequirePermissionAttribute(string moduleName, string permissionName)
        {
            _permissionName = permissionName;
            _moduleName = moduleName;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Skip authorization if action is decorated with [AllowAnonymous]
            if (context.ActionDescriptor.EndpointMetadata.Any(em => em is AllowAnonymousAttribute))
                return;

            // Get services
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();

            // Check permission
            bool hasPermission;
            if (string.IsNullOrEmpty(_moduleName))
            {
                hasPermission = await context.HttpContext.User.HasPermissionAsync(
                    _permissionName, dbContext, userManager);
            }
            else
            {
                hasPermission = await context.HttpContext.User.HasModulePermissionAsync(
                    _moduleName, _permissionName, dbContext, userManager);
            }

            // If user doesn't have permission, return forbidden
            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}