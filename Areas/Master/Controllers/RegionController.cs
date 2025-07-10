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
    public class RegionController : Controller
    {
        private readonly IRegionService _regionService;
        private readonly ILogger<RegionController> _logger;

        public RegionController(
            IRegionService regionService,
            ILogger<RegionController> logger)
        {
            _regionService = regionService;
            _logger = logger;
        }

        // GET: Master/Region
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var regions = await _regionService.GetRegionsAsync(showInactive, searchTerm);

                var viewModel = new RegionIndexViewModel
                {
                    Regions = regions.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading regions");
                TempData["ErrorMessage"] = "An error occurred while loading the regions.";
                return View(new RegionIndexViewModel());
            }
        }

        // GET: Master/Region/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var region = await _regionService.GetRegionByIdAsync(id);
                if (region == null)
                {
                    return NotFound();
                }

                return View(region);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading region details for ID {RegionId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the region details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/Region/Create
        public IActionResult Create()
        {
            try
            {
                var viewModel = new RegionCreateEditViewModel
                {
                    Region = new RegionViewModel()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create region page");
                TempData["ErrorMessage"] = "An error occurred while loading the page.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Region/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegionCreateEditViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if region name already exists
                    if (await _regionService.RegionExistsAsync(viewModel.Region.RegionName))
                    {
                        ModelState.AddModelError("Region.RegionName", "A region with this name already exists.");
                    }
                    else
                    {
                        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                        var success = await _regionService.SaveRegionAsync(viewModel.Region);

                        if (success)
                        {
                            TempData["SuccessMessage"] = "Region created successfully.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to create region.");
                        }
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating region");
                ModelState.AddModelError("", "An error occurred while creating the region.");
                return View(viewModel);
            }
        }

        // GET: Master/Region/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var region = await _regionService.GetRegionByIdAsync(id);
                if (region == null)
                {
                    return NotFound();
                }

                var viewModel = new RegionCreateEditViewModel
                {
                    Region = region
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading region for edit with ID {RegionId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the region.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Region/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, RegionCreateEditViewModel model)
        {
            if (id != model.Region.RegionId)
            {
                return NotFound(new { success = false, message = "Region ID mismatch." });
            }

            try
            {
                if (await _regionService.RegionExistsAsync(model.Region.RegionName, id))
                {
                    ModelState.AddModelError("Region.RegionName", "A region with this name already exists.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var success = await _regionService.SaveRegionAsync(model.Region);

                        if (success)
                        {
                            return Json(new { success = true, message = "Region updated successfully." });
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to update Region. Please try again.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating region with ID {RegionId}", id);
                        ModelState.AddModelError("", "An error occurred while updating the region.");
                    }
                }


                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView(model); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred in Edit POST for ID {RegionId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected server error occurred." });
            }
        }

        // POST: Master/Region/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _regionService.DeleteRegionAsync(id); // <-- The core deletion call

                if (success)
                {
                    return Json(new { success = true, message = "Region deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete region." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete region with ID {RegionId} - has associated cities", id);
                return BadRequest(new { success = false, message = "Cannot delete this region because it has associated cities." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting region with ID {RegionId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the region." });
            }
        }


        // AJAX endpoint for checking if region name exists
        [HttpGet]
        public async Task<JsonResult> CheckRegionNameExists(string regionName, short? excludeId = null)
        {
            try
            {
                var exists = await _regionService.RegionExistsAsync(regionName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking region name existence");
                return Json(new { exists = false, error = true });
            }
        }

        // AJAX endpoint for getting active regions (for use in other forms like City dropdown)
        [HttpGet]
        public async Task<JsonResult> GetActiveRegions()
        {
            try
            {
                var regions = await _regionService.GetActiveRegionsAsync();
                var result = regions.Select(r => new { value = r.RegionId, text = r.RegionName });
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active regions");
                return Json(new { error = true, message = "Error loading regions" });
            }
        }
    }
}