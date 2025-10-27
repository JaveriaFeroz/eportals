using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels;
using System.Net;
using System.Security.Claims;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class SubNatureController : Controller
    {
        private readonly ISubNatureService _subNatureService;
        private readonly ILogger<SubNatureController> _logger;

        public SubNatureController(
            ISubNatureService subNatureService,
            ILogger<SubNatureController> logger)
        {
            _subNatureService = subNatureService;
            _logger = logger;
        }

        // GET: Master/SubNature
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var subNatures = await _subNatureService.GetSubNaturesAsync(showInactive, searchTerm);

                var viewModel = new SubNatureIndexViewModel
                {
                    SubNatures = subNatures.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading subNatures");
                TempData["ErrorMessage"] = "An error occurred while loading the subNatures.";
                return View(new SubNatureIndexViewModel());
            }
        }

        // GET: Master/SubNature/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var subNature = await _subNatureService.GetSubNatureByIdAsync(id);
                if (subNature == null)
                {
                    return NotFound();
                }

                return View(subNature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading subNature details for ID {SubNatureId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the subNature details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/SubNature/Create
        public IActionResult Create()
        {
            var model = new SubNatureViewModel();
            return View(model);
        }

        // POST: Master/SubNature/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubNatureViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if subNature name already exists
                    if (await _subNatureService.SubNatureExistsAsync(model.SubNatureName))
                    {
                        ModelState.AddModelError("SubNatureName", "A subNature with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _subNatureService.SaveSubNatureAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "SubNature created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create subNature.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subNature");
                ModelState.AddModelError("", "An error occurred while creating the subNature.");
                return View(model);
            }
        }

        // GET: Master/SubNature/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var subNature = await _subNatureService.GetSubNatureByIdAsync(id);
                if (subNature == null)
                {
                    return NotFound();
                }

                return View(subNature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading subNature for edit with ID {SubNatureId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the subNature.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/SubNature/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, SubNatureViewModel model)
        {
            if (id != model.SubNatureId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _subNatureService.SubNatureExistsAsync(model.SubNatureName, id))
                    {
                        ModelState.AddModelError("SubNatureName", "A subNature with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _subNatureService.SaveSubNatureAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "SubNature updated successfully.";
                        return Json(new { success = true, message = "SubNature updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update subNature.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subNature with ID {SubNatureId}", id);
                ModelState.AddModelError("", "An error occurred while updating the subNature.");
                return PartialView(model);
            }
        }

        // GET: Master/SubNature/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var subNature = await _subNatureService.GetSubNatureByIdAsync(id);
                if (subNature == null)
                {
                    return NotFound();
                }

                return View(subNature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading subNature for delete with ID {SubNatureId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the subNature.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/SubNature/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _subNatureService.DeleteSubNatureAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "SubNature deleted successfully.";
                    return Json(new { success = true, message = "SubNature deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete subNature." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete subNature with ID {SubNatureId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this subNature." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting subNature with ID {SubNatureId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the subNature." });
            }
        }

        // AJAX endpoint for checking if subNature name exists
        [HttpGet]
        public async Task<JsonResult> CheckSubNatureNameExists(string subNatureName, short? excludeId = null)
        {
            try
            {
                var exists = await _subNatureService.SubNatureExistsAsync(subNatureName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking subNature name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}