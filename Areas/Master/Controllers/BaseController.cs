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
    public class BaseController : Controller
    {
        private readonly IBaseService _basesService;
        private readonly ILogger<BaseController> _logger;

        public BaseController(
            IBaseService basesService,
            ILogger<BaseController> logger)
        {
            _basesService = basesService;
            _logger = logger;
        }

        // GET: Master/Base
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var basess = await _basesService.GetBasesAsync(showInactive, searchTerm);

                var viewModel = new BaseIndexViewModel
                {
                    Bases = basess.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading basess");
                TempData["ErrorMessage"] = "An error occurred while loading the basess.";
                return View(new BaseIndexViewModel());
            }
        }

        // GET: Master/Base/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var bases = await _basesService.GetBaseByIdAsync(id);
                if (bases == null)
                {
                    return NotFound();
                }

                return View(bases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bases details for ID {BaseId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the bases details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/Base/Create
        public IActionResult Create()
        {
            var model = new BaseViewModel();
            return View(model);
        }

        // POST: Master/Base/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BaseViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if bases name already exists
                    if (await _basesService.BaseExistsAsync(model.BaseName))
                    {
                        ModelState.AddModelError("BaseName", "A bases with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _basesService.SaveBaseAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Base created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create bases.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bases");
                ModelState.AddModelError("", "An error occurred while creating the bases.");
                return View(model);
            }
        }

        // GET: Master/Base/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var bases = await _basesService.GetBaseByIdAsync(id);
                if (bases == null)
                {
                    return NotFound();
                }

                return View(bases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bases for edit with ID {BaseId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the bases.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Base/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, BaseViewModel model)
        {
            if (id != model.BaseId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _basesService.BaseExistsAsync(model.BaseName, id))
                    {
                        ModelState.AddModelError("BaseName", "A bases with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _basesService.SaveBaseAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Base updated successfully.";
                        return Json(new { success = true, message = "Base updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update bases.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bases with ID {BaseId}", id);
                ModelState.AddModelError("", "An error occurred while updating the bases.");
                return PartialView(model);
            }
        }

        // GET: Master/Base/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var bases = await _basesService.GetBaseByIdAsync(id);
                if (bases == null)
                {
                    return NotFound();
                }

                return View(bases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bases for delete with ID {BaseId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the bases.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Base/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _basesService.DeleteBaseAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Base deleted successfully.";
                    return Json(new { success = true, message = "Base deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete bases." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete base with ID {BaseId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this bases." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting bases with ID {BaseId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the bases." });
            }
        }

        // AJAX endpoint for checking if bases name exists
        [HttpGet]
        public async Task<JsonResult> CheckBaseNameExists(string basesName, short? excludeId = null)
        {
            try
            {
                var exists = await _basesService.BaseExistsAsync(basesName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking bases name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}