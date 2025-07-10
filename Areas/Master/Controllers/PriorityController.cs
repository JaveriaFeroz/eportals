using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels;
using System.Net;
using System.Security.Claims;

namespace ProcureToPay.Areas.Master.Controllers
{
    // Priority Controller
    [Area("Master")]
    [Authorize]
    public class PriorityController : Controller
    {
        private readonly IPriorityService _priorityService;
        private readonly ILogger<PriorityController> _logger;

        public PriorityController(IPriorityService priorityService, ILogger<PriorityController> logger)
        {
            _priorityService = priorityService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var priorities = await _priorityService.GetPrioritiesAsync(showInactive, searchTerm);
                var viewModel = new PriorityIndexViewModel
                {
                    Priorities = priorities.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading priorities");
                TempData["ErrorMessage"] = "An error occurred while loading the priorities.";
                return View(new PriorityIndexViewModel());
            }
        }

        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var priority = await _priorityService.GetPriorityByIdAsync(id);
                if (priority == null) return NotFound();
                return View(priority);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading priority details for ID {PriorityId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the priority details.";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            return View(new PriorityViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PriorityViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await _priorityService.PriorityExistsAsync(model.PriorityName))
                    {
                        ModelState.AddModelError("PriorityName", "A priority with this name already exists.");
                        return View(model);
                    }

                    var success = await _priorityService.SavePriorityAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Priority created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create priority.");
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating priority");
                ModelState.AddModelError("", "An error occurred while creating the priority.");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var priority = await _priorityService.GetPriorityByIdAsync(id);
                if (priority == null) return NotFound();
                return View(priority);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading priority for edit with ID {PriorityId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the priority.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, PriorityViewModel model)
        {
            if (id != model.PriorityId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _priorityService.PriorityExistsAsync(model.PriorityName, id))
                    {
                        ModelState.AddModelError("PriorityName", "A priority with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _priorityService.SavePriorityAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Priority updated successfully.";
                        return Json(new { success = true, message = "Priority updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update priority.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating priority with ID {PriorityId}", id);
                ModelState.AddModelError("", "An error occurred while updating the priority.");
                return PartialView(model);
            }
        }

        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var priority = await _priorityService.GetPriorityByIdAsync(id);
                if (priority == null) return NotFound();
                return View(priority);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading priority for delete with ID {PriorityId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the priority.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _priorityService.DeletePriorityAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Priority deleted successfully.";
                    return Json(new { success = true, message = "Priority deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete priority." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete priority with ID {PriorityId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this priority." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting priority with ID {PriorityId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the priority." });
            }
        }

        [HttpGet]
        public async Task<JsonResult> CheckPriorityNameExists(string priorityName, short? excludeId = null)
        {
            try
            {
                var exists = await _priorityService.PriorityExistsAsync(priorityName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking priority name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}