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
    public class TrailerController : Controller
    {
        private readonly ITrailerService _trailerService;
        private readonly ILogger<TrailerController> _logger;

        public TrailerController(
            ITrailerService trailerService,
            ILogger<TrailerController> logger)
        {
            _trailerService = trailerService;
            _logger = logger;
        }

        // GET: Master/Trailer
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var trailers = await _trailerService.GetTrailersAsync(showInactive, searchTerm);

                var viewModel = new TrailerIndexViewModel
                {
                    Trailers = trailers.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading trailers");
                TempData["ErrorMessage"] = "An error occurred while loading the trailers.";
                return View(new TrailerIndexViewModel());
            }
        }

        // GET: Master/Trailer/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var trailer = await _trailerService.GetTrailerByIdAsync(id);
                if (trailer == null)
                {
                    return NotFound();
                }

                return View(trailer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading trailer details for ID {TrailerId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the trailer details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/Trailer/Create
        public IActionResult Create()
        {
            var model = new TrailerViewModel();
            return View(model);
        }

        // POST: Master/Trailer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrailerViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if trailer name already exists
                    if (await _trailerService.TrailerExistsAsync(model.TrailerName))
                    {
                        ModelState.AddModelError("TrailerName", "A trailer with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _trailerService.SaveTrailerAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Trailer created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create trailer.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trailer");
                ModelState.AddModelError("", "An error occurred while creating the trailer.");
                return View(model);
            }
        }

        // GET: Master/Trailer/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var trailer = await _trailerService.GetTrailerByIdAsync(id);
                if (trailer == null)
                {
                    return NotFound();
                }

                return View(trailer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading trailer for edit with ID {TrailerId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the trailer.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Trailer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, TrailerViewModel model)
        {
            if (id != model.TrailerId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _trailerService.TrailerExistsAsync(model.TrailerName, id))
                    {
                        ModelState.AddModelError("TrailerName", "A trailer with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _trailerService.SaveTrailerAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Trailer updated successfully.";
                        return Json(new { success = true, message = "Trailer updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update trailer.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trailer with ID {TrailerId}", id);
                ModelState.AddModelError("", "An error occurred while updating the trailer.");
                return PartialView(model);
            }
        }

        // GET: Master/Trailer/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var trailer = await _trailerService.GetTrailerByIdAsync(id);
                if (trailer == null)
                {
                    return NotFound();
                }

                return View(trailer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading trailer for delete with ID {TrailerId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the trailer.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Trailer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _trailerService.DeleteTrailerAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Trailer deleted successfully.";
                    return Json(new { success = true, message = "Trailer deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete trailer." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete trailer with ID {TrailerId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this trailer." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting trailer with ID {TrailerId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the trailer." });
            }
        }

        // AJAX endpoint for checking if trailer name exists
        [HttpGet]
        public async Task<JsonResult> CheckTrailerNameExists(string trailerName, short? excludeId = null)
        {
            try
            {
                var exists = await _trailerService.TrailerExistsAsync(trailerName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking trailer name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}