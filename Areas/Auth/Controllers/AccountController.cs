using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProcureToPay.Areas.Auth.Models;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Areas.UserManagement.Services;
using ProcureToPay.Helpers;
using ProcureToPay.Services;
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
        private readonly IEmailService _emailService;
        private readonly IPasswordGeneratorService _passwordGeneratorService;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AccountController> logger,
            IUpdateService updateService,
            IEmailService emailService,
            IPasswordGeneratorService passwordGeneratorService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _updateService = updateService;
            _emailService = emailService;
            _passwordGeneratorService = passwordGeneratorService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["ReturnUrl"] = returnUrl;

            // Transfer TempData message to ViewData for display in the view
            if (TempData["StatusMessage"] != null)
            {
                ViewData["StatusMessage"] = TempData["StatusMessage"];
            }

            var model = new LoginViewModel();
            model.Updates = await _updateService.GetActiveUpdatesAsync();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // 1. Find User by Name or Email
                var user = await _userManager.FindByNameAsync(model.Username) ?? await _userManager.FindByEmailAsync(model.Username);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    model.Updates = await _updateService.GetActiveUpdatesAsync();
                    return View(model);
                }


                // Ensure lockout is enabled for this user if we are managing it manually
                if (!await _userManager.GetLockoutEnabledAsync(user))
                {
                    await _userManager.SetLockoutEnabledAsync(user, true);
                }

                // 2. Check Password and trigger Identity's internal lockout counter
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    user.LastLoginDate = DateTimeHelper.GetPakistanStandardTime();
                    await _userManager.UpdateAsync(user);
                    // NEW LOGIC: Check if ForcePasswordChange is set
                    if (user.ForcePasswordChange)
                    {
                        // User ko sign in karein, lekin phir usay password change page par redirect kar dein
                        await _signInManager.SignInAsync(user, model.RememberMe);
                        TempData["StatusMessage"] = "You must change your password before proceeding.";
                        return RedirectToAction(nameof(ChangePasswordPage));
                    }

                    // Normal successful login flow
                    await _userManager.ResetAccessFailedCountAsync(user);
                    await _signInManager.SignInAsync(user, model.RememberMe);
                    _logger.LogInformation("User logged in: {UserName}", user.UserName);
                    return RedirectToLocal(returnUrl);
                }

                // 3. Handle Lockout and Progressive Escalation (If result is not success)
                if (result.IsLockedOut)
                {
                    var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);

                    TimeSpan lockoutDuration;
                    string lockoutMessage;

                    // Implement progressive time lockout rules
                    if (failedAttempts >= 15)
                    {
                        lockoutDuration = TimeSpan.FromDays(3);
                        lockoutMessage = $"Your account has been locked for 3 days due to {failedAttempts} failed attempts. Please contact support.";
                    }
                    else if (failedAttempts >= 10)
                    {
                        lockoutDuration = TimeSpan.FromDays(1);
                        lockoutMessage = $"Your account has been locked for 1 day due to {failedAttempts} failed attempts.";
                    }
                    else
                    {
                        lockoutDuration = TimeSpan.FromHours(1);
                        lockoutMessage = $"Your account has been locked for 1 hour due to multiple failed attempts.";
                    }

                    // Manually override the lockout end date with the escalated duration
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.Add(lockoutDuration));
                    _logger.LogWarning($"User account locked out with custom duration: {lockoutDuration.TotalHours} hours. Attempts: {failedAttempts}");

                    // *** CRUCIAL STEP: Pass the Lockout End Date via TempData ***
                    TempData["LockoutEndDate"] = user.LockoutEnd?.UtcDateTime.ToString("o");
                    TempData["LockoutMessage"] = lockoutMessage;

                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    // Standard failure response if password was wrong but no lockout was triggered
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    model.Updates = await _updateService.GetActiveUpdatesAsync();
                    return View(model);
                }
            }

            // If ModelState was invalid initially, refresh updates and return view
            model.Updates = await _updateService.GetActiveUpdatesAsync();
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Lockout()
        {
            // Transfer LockoutEndDate from TempData to ViewData
            // This is the essential step for the JS countdown to access the data.
            if (TempData.ContainsKey("LockoutEndDate"))
            {
                // We use Peek/Keep to ensure the data persists until the view reads it,
                // and then we explicitly set ViewData to display it.
                ViewData["LockoutEndDate"] = TempData["LockoutEndDate"];
            }

            // NOTE: We could fetch the user by username if we stored it in TempData, 
            // but relying on the Login POST to set LockoutEndDate is safer for the progressive lockout time.

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
        public async Task<IActionResult> ChangePasswordPage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ChangePasswordViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                ForceChange = user.ForcePasswordChange
            };

            // Use TempData to display the status message
            if (TempData["StatusMessage"] != null)
            {
                ViewData["StatusMessage"] = TempData["StatusMessage"];
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePasswordPage(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.ForceChange = user.ForcePasswordChange;
                return View(model);
            }

            IdentityResult result;
            if (user.ForcePasswordChange)
            {
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to update password. Please contact support.");
                    model.ForceChange = user.ForcePasswordChange;
                    return View(model);
                }
                result = await _userManager.AddPasswordAsync(user, model.NewPassword);
                user.ForcePasswordChange = false;
                await _userManager.UpdateAsync(user);
            }
            else
            {
                result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            }

            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                TempData["StatusMessage"] = "Your password has been changed successfully. Please log in with your new password.";
                return RedirectToAction("Login", "Account", new { area = "Auth" });
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                model.ForceChange = user.ForcePasswordChange;
                return View(model);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ChangePasswordViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                // Check the flag to control the view's behavior
                ForceChange = user.ForcePasswordChange
            };

            return PartialView(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            if (!ModelState.IsValid)
            {
                return PartialView(model);
            }

            IdentityResult result;
            if (user.ForcePasswordChange)
            {
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to update password. Please contact support.");
                    return PartialView(model);
                }
                result = await _userManager.AddPasswordAsync(user, model.NewPassword);
                user.ForcePasswordChange = false;
                await _userManager.UpdateAsync(user);
            }
            else
            {
                result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            }

            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                TempData["StatusMessage"] = "Your password has been changed successfully. Please log in with your new password.";
                return RedirectToAction("Login", "Account", new { area = "Auth" });
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return PartialView(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword()
        {
            var model = new ForgotPasswordViewModel();
            model.Updates = await _updateService.GetActiveUpdatesAsync();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                // NEW LOGIC: Only proceed with password reset if the user exists and the email is confirmed.
                if (user != null)
                {
                    // NEW LOGIC START: Generate temporary password and force change
                    var newTemporaryPassword = _passwordGeneratorService.GenerateTemporaryPassword();

                    // 1. Remove old password (required before setting a new one without token)
                    var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                    if (!removePasswordResult.Succeeded)
                    {
                        _logger.LogError("Failed to remove existing password for user {UserId} during ForgotPassword.", user.Id);
                        ModelState.AddModelError(string.Empty, "An error occurred while resetting the password. Please contact support.");
                        model.Updates = await _updateService.GetActiveUpdatesAsync();
                        return View(model);
                    }

                    // 2. Add the new temporary password
                    var addPasswordResult = await _userManager.AddPasswordAsync(user, newTemporaryPassword);
                    if (!addPasswordResult.Succeeded)
                    {
                        _logger.LogError("Failed to add new temporary password for user {UserId} during ForgotPassword.", user.Id);
                        foreach (var error in addPasswordResult.Errors)
                        {
                            _logger.LogError("Identity Error: {Code} - {Description}", error.Code, error.Description);
                        }
                        ModelState.AddModelError(string.Empty, "Failed to set a new temporary password.");
                        model.Updates = await _updateService.GetActiveUpdatesAsync();
                        return View(model);
                    }

                    // 3. Force password change on next login
                    user.ForcePasswordChange = true;
                    var updateResult = await _userManager.UpdateAsync(user);

                    if (!updateResult.Succeeded)
                    {
                        _logger.LogError("Failed to set ForcePasswordChange flag for user {UserId}.", user.Id);
                        ModelState.AddModelError(string.Empty, "Failed to update user flag. Please contact support.");
                        model.Updates = await _updateService.GetActiveUpdatesAsync();
                        return View(model);
                    }

                    // 4. Send the credentials email
                    await _emailService.SendCredentialsEmailAsync(
                        model.Email,
                        user.UserName,
                        newTemporaryPassword);

                    _logger.LogInformation("Password reset (via temporary password) email sent to {Email}", model.Email);
                }

                // Always redirect to the confirmation page to avoid revealing user existence.
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            model.Updates = await _updateService.GetActiveUpdatesAsync();
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordConfirmation()
        {
            var model = new ForgotPasswordViewModel();
            model.Updates = await _updateService.GetActiveUpdatesAsync();

            return View(model);
        }
    }
}
