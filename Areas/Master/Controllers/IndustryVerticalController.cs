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
    public class IndustryVerticalController : Controller
    {
        private readonly IIndustryVerticalService _industryVerticalService;
        private readonly ILogger<IndustryVerticalController> _logger;

        public IndustryVerticalController(
            IIndustryVerticalService industryVerticalService,
            ILogger<IndustryVerticalController> logger)
        {
            _industryVerticalService = industryVerticalService;
            _logger = logger;
        }

        // GET: Master/IndustryVertical
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var industryVerticals = await _industryVerticalService.GetIndustryVerticalsAsync(showInactive, searchTerm);

                var viewModel = new IndustryVerticalIndexViewModel
                {
                    IndustryVerticals = industryVerticals.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading industryVerticals");
                TempData["ErrorMessage"] = "An error occurred while loading the industryVerticals.";
                return View(new IndustryVerticalIndexViewModel());
            }
        }

        // GET: Master/IndustryVertical/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var industryVertical = await _industryVerticalService.GetIndustryVerticalByIdAsync(id);
                if (industryVertical == null)
                {
                    return NotFound();
                }

                return View(industryVertical);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading industryVertical details for ID {IndustryVerticalId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the industryVertical details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/IndustryVertical/Create
        public IActionResult Create()
        {
            var model = new IndustryVerticalViewModel();
            return View(model);
        }

        // POST: Master/IndustryVertical/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IndustryVerticalViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if industryVertical name already exists
                    if (await _industryVerticalService.IndustryVerticalExistsAsync(model.IndustryVerticalName))
                    {
                        ModelState.AddModelError("IndustryVerticalName", "A industryVertical with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _industryVerticalService.SaveIndustryVerticalAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "IndustryVertical created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create industryVertical.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating industryVertical");
                ModelState.AddModelError("", "An error occurred while creating the industryVertical.");
                return View(model);
            }
        }

        // GET: Master/IndustryVertical/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var industryVertical = await _industryVerticalService.GetIndustryVerticalByIdAsync(id);
                if (industryVertical == null)
                {
                    return NotFound();
                }

                return View(industryVertical);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading industryVertical for edit with ID {IndustryVerticalId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the industryVertical.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/IndustryVertical/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, IndustryVerticalViewModel model)
        {
            if (id != model.IndustryVerticalId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _industryVerticalService.IndustryVerticalExistsAsync(model.IndustryVerticalName, id))
                    {
                        ModelState.AddModelError("IndustryVerticalName", "A industryVertical with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _industryVerticalService.SaveIndustryVerticalAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "IndustryVertical updated successfully.";
                        return Json(new { success = true, message = "IndustryVertical updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update industryVertical.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating industryVertical with ID {IndustryVerticalId}", id);
                ModelState.AddModelError("", "An error occurred while updating the industryVertical.");
                return PartialView(model);
            }
        }

        // GET: Master/IndustryVertical/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var industryVertical = await _industryVerticalService.GetIndustryVerticalByIdAsync(id);
                if (industryVertical == null)
                {
                    return NotFound();
                }

                return View(industryVertical);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading industryVertical for delete with ID {IndustryVerticalId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the industryVertical.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/IndustryVertical/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _industryVerticalService.DeleteIndustryVerticalAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "IndustryVertical deleted successfully.";
                    return Json(new { success = true, message = "IndustryVertical deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete industryVertical." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete industryVertical with ID {IndustryVerticalId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this industryVertical." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting industryVertical with ID {IndustryVerticalId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the industryVertical." });
            }
        }

        // AJAX endpoint for checking if industryVertical name exists
        [HttpGet]
        public async Task<JsonResult> CheckIndustryVerticalNameExists(string industryVerticalName, short? excludeId = null)
        {
            try
            {
                var exists = await _industryVerticalService.IndustryVerticalExistsAsync(industryVerticalName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking industryVertical name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}