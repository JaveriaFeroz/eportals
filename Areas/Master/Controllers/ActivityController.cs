using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels;
using System.Security.Claims;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class ActivityController : Controller
    {
        private readonly IActivityService _activityService;
        private readonly ILogger<ActivityController> _logger;

        public ActivityController(
            IActivityService activityService,
            ILogger<ActivityController> logger)
        {
            _activityService = activityService;
            _logger = logger;
        }

        // GET: Master/Activity
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var activities = await _activityService.GetActivitiesAsync(showInactive, searchTerm);

                var viewModel = new ActivityIndexViewModel
                {
                    Activities = activities.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading activities");
                TempData["ErrorMessage"] = "An error occurred while loading the activities.";
                return View(new ActivityIndexViewModel());
            }
        }

        // GET: Master/Activity/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var viewModel = await _activityService.GetActivityByIdAsync(id);
                if (viewModel == null)
                {
                    return NotFound();
                }

                // Return a PARTIAL VIEW which is perfect for modals
                return PartialView(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading activity details for ID {ActivityId}", id);
                // For an AJAX call, return an error message directly
                return Content("<div class='alert alert-danger'>An error occurred while loading the details.</div>");
            }
        }

        // GET: Master/Activity/Create
        public IActionResult Create()
        {
            var model = new ActivityViewModel();
            return View(model);
        }

        // POST: Master/Activity/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActivityViewModel model)
        {
            try
            {
                // First, check for custom business rule violations like existing names
                if (await _activityService.ActivityExistsAsync(model.ActivityName))
                {
                    ModelState.AddModelError("ActivityName", "An activity with this name already exists.");
                }

                if (ModelState.IsValid)
                {
                    // --- Start: Add Audit Information ---

                    // Get the current logged-in user's ID from the HttpContext.
                    // This is the standard way in ASP.NET Core Identity.
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;

                    // Set the audit properties on the model before saving.
                    model.CreatedBy = currentUserId;
                    model.UpdatedBy = currentUserId; // As requested, UpdatedBy is same as CreatedBy on create
                    model.CreatedOn = DateTime.UtcNow; // It's best practice to also set the timestamp
                    model.UpdatedOn = DateTime.UtcNow; // Set timestamp for update as well

                    // --- End: Add Audit Information ---

                    var success = await _activityService.SaveActivityAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Activity created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create activity.");
                    }
                }

                // If we get here, something failed. Return the view with the model to show errors.
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity");
                ModelState.AddModelError("", "An error occurred while creating the activity.");
                return View(model);
            }
        }

        // GET: Master/Activity/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                // Get the data and map it to a ViewModel
                var viewModel = await _activityService.GetActivityByIdAsync(id);

                if (viewModel == null)
                {
                    // Return a 404 Not Found error if the item doesn't exist
                    return NotFound();
                }

                // Return a PARTIAL VIEW containing the form. 
                // This is the correct way for AJAX modals.
                // We assume the partial view file is named "_EditActivity.cshtml"
                return PartialView(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading activity for edit with ID {ActivityId}", id);
                // For AJAX requests, it's better to return an error status code
                return StatusCode(500, "An error occurred while loading the data.");
            }
        }

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, ActivityViewModel model)
        {
            // The property name in your ViewModel is likely ChargeId
            if (id != model.ActivityId)
            {
                return NotFound();
            }

            // Check for duplicate Charge Code *before* checking the entire model state
            // This provides a better user experience
            if (await _activityService.ActivityExistsAsync(model.ActivityName, id))
            {
                ModelState.AddModelError("ActivityName", "An activity with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _activityService.SaveActivityAsync(model);

                    if (success)
                    {
                        // CHANGED: Return JSON for a successful AJAX call
                        return Json(new { success = true, message = "Activity updated successfully." });
                    }
                    else
                    {
                        // This case is for unexpected save failures (database error, etc.)
                        ModelState.AddModelError("", "Failed to update activity. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating activity with ID {ActivityId}", id);
                    ModelState.AddModelError("", "An unexpected error occurred while updating the activity.");
                }
            }

            return PartialView(model);
        }

        // POST: Master/Activity/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                // Attempt to delete the activity using the service
                var success = await _activityService.DeleteActivityAsync(id);

                if (success)
                {
                    // On success, return a JSON object with a success message
                    return Json(new { success = true, message = "Activity deleted successfully." });
                }
                else
                {
                    // If the service returns false (e.g., item not found), return a JSON error
                    return Json(new { success = false, message = "Failed to delete activity. It may have already been removed." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity with ID {ActivityId}", id);

                // On an unexpected server error, return a JSON error
                return Json(new { success = false, message = "An error occurred while deleting the activity." });
            }
        }

        // AJAX endpoint for checking if activity name exists
        [HttpGet]
        public async Task<JsonResult> CheckActivityNameExists(string activityName, short? excludeId = null)
        {
            try
            {
                var exists = await _activityService.ActivityExistsAsync(activityName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking activity name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}