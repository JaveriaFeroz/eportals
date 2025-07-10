using ProcureToPay.Areas.Auth.Models;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Areas.UserManagement.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;

namespace ProcureToPay.Areas.Auth.Controllers
{
    [Area("Auth")]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IUpdateService _updateService;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AccountController> logger,
            IUpdateService updateService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _updateService = updateService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["ReturnUrl"] = returnUrl;

            var model = new LoginViewModel();
            // Get updates for login page
            model.Updates = await _updateService.GetActiveUpdatesAsync();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Log the raw form data
            foreach (var key in Request.Form.Keys)
            {
                _logger.LogInformation($"Form key: {key}, value: {Request.Form[key]}");
            }

            // Handle RememberMe explicitly if needed (model binding should handle this)
            if (Request.Form.ContainsKey("RememberMe"))
            {
                bool rememberMe = Request.Form["RememberMe"] == "true";
                model.RememberMe = rememberMe;
                _logger.LogInformation($"RememberMe set to: {rememberMe}");
            }

            _logger.LogInformation($"Model: Username={model?.Username ?? "null"}, " +
                                    $"Password={(model?.Password != null ? "provided" : "null")}, " +
                                    $"RememberMe={model?.RememberMe}");

            // Remove validation error for RememberMe if present (not usually needed)
            ModelState.Remove("RememberMe");

            if (ModelState.IsValid)
            {
                // Your authentication logic...
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null)
                {
                    // Check if they used email instead of username
                    user = await _userManager.FindByEmailAsync(model.Username);
                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        // Re-fetch updates for the login view on failure
                        model.Updates = await _updateService.GetActiveUpdatesAsync();
                        return View(model);
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in: {UserName}", user.UserName);
                    return RedirectToLocal(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    // Re-fetch updates for the login view on failure
                    model.Updates = await _updateService.GetActiveUpdatesAsync();
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form with updates
            model.Updates = await _updateService.GetActiveUpdatesAsync();
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
       
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Login");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult TestFormSubmit()
        {
            var response = new StringBuilder();
            response.AppendLine("<h3>Form Data Received:</h3>");

            foreach (var key in Request.Form.Keys)
            {
                response.AppendLine($"{key}: {Request.Form[key]}<br>");
            }

            return Content(response.ToString(), "text/html");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            _logger.LogInformation($"Redirecting to: {(string.IsNullOrEmpty(returnUrl) ? "Home/Index" : returnUrl)}");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new ChangePasswordViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                ForceChange = user.ForcePasswordChange
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // If this is a forced password change, verify the old password
            if (user.ForcePasswordChange)
            {
                var checkPasswordResult = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
                if (!checkPasswordResult)
                {
                    ModelState.AddModelError(string.Empty, "Current password is incorrect.");
                    return View(model);
                }
            }
            else
            {
                // Regular password change flow with old password verification
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            // Set the new password without confirming the old one (for users with temporary passwords)
            if (user.ForcePasswordChange)
            {
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    _logger.LogError("Failed to remove existing password for user {UserId}", user.Id);
                    ModelState.AddModelError(string.Empty, "Failed to update password. Please try again.");
                    return View(model);
                }

                var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
                if (!addPasswordResult.Succeeded)
                {
                    foreach (var error in addPasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                // Turn off force password change flag
                user.ForcePasswordChange = false;
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User {UserId} changed their password successfully.", user.Id);

            if (user.ForcePasswordChange)
            {
                TempData["StatusMessage"] = "Your password has been changed successfully. You can now access the system.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            else
            {
                TempData["StatusMessage"] = "Your password has been changed successfully.";
                return RedirectToAction("ChangePassword");
            }
        }

     

    }
}