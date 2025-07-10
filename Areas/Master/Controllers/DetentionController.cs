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
    public class DetentionController : Controller
    {
        private readonly IDetentionService _detentionService;
        private readonly ILogger<DetentionController> _logger;

        public DetentionController(
            IDetentionService detentionService,
            ILogger<DetentionController> logger)
        {
            _detentionService = detentionService;
            _logger = logger;
        }

        // GET: Master/Detention
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var detentions = await _detentionService.GetDetentionsAsync(showInactive, searchTerm);

                var viewModel = new DetentionIndexViewModel
                {
                    Detentions = detentions.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading detentions");
                TempData["ErrorMessage"] = "An error occurred while loading the detentions.";
                return View(new DetentionIndexViewModel());
            }
        }

        // GET: Master/Detention/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var detention = await _detentionService.GetDetentionByIdAsync(id);
                if (detention == null)
                {
                    return NotFound();
                }

                return View(detention);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading detention details for ID {DetentionId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the detention details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/Detention/Create
        public IActionResult Create()
        {
            var model = new DetentionViewModel();
            return View(model);
        }

        // POST: Master/Detention/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DetentionViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if detention name already exists
                    if (await _detentionService.DetentionExistsAsync(model.DetentionName))
                    {
                        ModelState.AddModelError("DetentionName", "A detention with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _detentionService.SaveDetentionAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Detention created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create detention.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating detention");
                ModelState.AddModelError("", "An error occurred while creating the detention.");
                return View(model);
            }
        }

        // GET: Master/Detention/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var detention = await _detentionService.GetDetentionByIdAsync(id);
                if (detention == null)
                {
                    return NotFound();
                }

                return View(detention);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading detention for edit with ID {DetentionId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the detention.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Detention/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, DetentionViewModel model)
        {
            if (id != model.DetentionId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _detentionService.DetentionExistsAsync(model.DetentionName, id))
                    {
                        ModelState.AddModelError("DetentionName", "A detention with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _detentionService.SaveDetentionAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Detention updated successfully.";
                        return Json(new { success = true, message = "Detention updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update detention.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating detention with ID {DetentionId}", id);
                ModelState.AddModelError("", "An error occurred while updating the detention.");
                return PartialView(model);
            }
        }

        // GET: Master/Detention/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var detention = await _detentionService.GetDetentionByIdAsync(id);
                if (detention == null)
                {
                    return NotFound();
                }

                return View(detention);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading detention for delete with ID {DetentionId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the detention.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Detention/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _detentionService.DeleteDetentionAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Detention deleted successfully.";
                    return Json(new { success = true, message = "Detention deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete detention." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete detention with ID {DetentionId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this detention." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting detention with ID {DetentionId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the detention." });
            }
        }

        // AJAX endpoint for checking if detention name exists
        [HttpGet]
        public async Task<JsonResult> CheckDetentionNameExists(string detentionName, short? excludeId = null)
        {
            try
            {
                var exists = await _detentionService.DetentionExistsAsync(detentionName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking detention name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}