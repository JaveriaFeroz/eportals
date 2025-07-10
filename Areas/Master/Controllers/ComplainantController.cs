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
    public class ComplainantController : Controller
    {
        private readonly IComplainantService _complainantService;
        private readonly ILogger<ComplainantController> _logger;

        public ComplainantController(
            IComplainantService complainantService,
            ILogger<ComplainantController> logger)
        {
            _complainantService = complainantService;
            _logger = logger;
        }

        // GET: Master/Complainant
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var complainants = await _complainantService.GetComplainantsAsync(showInactive, searchTerm);

                var viewModel = new ComplainantIndexViewModel
                {
                    Complainants = complainants.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading complainants");
                TempData["ErrorMessage"] = "An error occurred while loading the complainants.";
                return View(new ComplainantIndexViewModel());
            }
        }

        // GET: Master/Complainant/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var complainant = await _complainantService.GetComplainantByIdAsync(id);
                if (complainant == null)
                {
                    return NotFound();
                }

                return View(complainant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading complainant details for ID {ComplainantId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the complainant details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/Complainant/Create
        public IActionResult Create()
        {
            var model = new ComplainantViewModel();
            return View(model);
        }

        // POST: Master/Complainant/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComplainantViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if complainant name already exists
                    if (await _complainantService.ComplainantExistsAsync(model.ComplainantName))
                    {
                        ModelState.AddModelError("ComplainantName", "A complainant with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _complainantService.SaveComplainantAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Complainant created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create complainant.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating complainant");
                ModelState.AddModelError("", "An error occurred while creating the complainant.");
                return View(model);
            }
        }

        // GET: Master/Complainant/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var complainant = await _complainantService.GetComplainantByIdAsync(id);
                if (complainant == null)
                {
                    return NotFound();
                }

                return View(complainant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading complainant for edit with ID {ComplainantId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the complainant.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Complainant/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, ComplainantViewModel model)
        {
            if (id != model.ComplainantId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _complainantService.ComplainantExistsAsync(model.ComplainantName, id))
                    {
                        ModelState.AddModelError("ComplainantName", "A complainant with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _complainantService.SaveComplainantAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Complainant updated successfully.";
                        return Json(new { success = true, message = "Complainant updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update complainant.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating complainant with ID {ComplainantId}", id);
                ModelState.AddModelError("", "An error occurred while updating the complainant.");
                return PartialView(model);
            }
        }

        // GET: Master/Complainant/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var complainant = await _complainantService.GetComplainantByIdAsync(id);
                if (complainant == null)
                {
                    return NotFound();
                }

                return View(complainant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading complainant for delete with ID {ComplainantId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the complainant.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Complainant/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _complainantService.DeleteComplainantAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Complainant deleted successfully.";
                    return Json(new { success = true, message = "Complainant deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete complainant." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete complainant with ID {ComplainantId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this complainant." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting complainant with ID {ComplainantId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the complainant." });
            }
        }

        // AJAX endpoint for checking if complainant name exists
        [HttpGet]
        public async Task<JsonResult> CheckComplainantNameExists(string complainantName, short? excludeId = null)
        {
            try
            {
                var exists = await _complainantService.ComplainantExistsAsync(complainantName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking complainant name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}