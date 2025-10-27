using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Helpers;
using System.Security.Claims;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class CityController : Controller
    {
        private readonly ICityService _cityService;
        private readonly ILogger<CityController> _logger;

        public CityController(
            ICityService cityService,
            ILogger<CityController> logger)
        {
            _cityService = cityService;
            _logger = logger;
        }

        // GET: Master/City
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var cities = await _cityService.GetCitiesAsync(showInactive, searchTerm);
                var regions = await _cityService.GetRegionsAsync();
                var viewModel = new CityIndexViewModel
                {
                    Cities = cities.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm,
                    Regions = regions.Select(r => new SelectListItem
                    {
                        Value = r.RegionId.ToString(),
                        Text = r.RegionName
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cities");
                TempData["ErrorMessage"] = "An error occurred while loading the cities.";
                return View(new CityIndexViewModel());
            }
        }

        // GET: Master/City/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var city = await _cityService.GetCityByIdAsync(id);
                if (city == null)
                {
                    return NotFound();
                }

                return View(city);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading city details for ID {CityId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the city details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/City/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var regions = await _cityService.GetRegionsAsync();
                var viewModel = new CityCreateEditViewModel
                {
                    City = new CityViewModel(),
                    Regions = regions.Select(r => new SelectListItem
                    {
                        Value = r.RegionId.ToString(),
                        Text = r.RegionName
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create city page");
                TempData["ErrorMessage"] = "An error occurred while loading the page.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/City/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CityCreateEditViewModel viewModel)
        {
            try
            {
                if (await _cityService.CityExistsAsync(viewModel.City.CityCode))
                {
                    ModelState.AddModelError("City.CityCode", "A city with this code already exists.");
                }
                if (await _cityService.CityExistsAsync(viewModel.City.CityName))
                {
                    ModelState.AddModelError("City.CityName", "A city with this name already exists.");
                }

                // 2. Now, check if the model is valid (including your custom checks above).
                if (ModelState.IsValid)
                {
                    // 3. Set the audit properties.
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    viewModel.City.CreatedBy = currentUserId;
                    viewModel.City.UpdatedBy = currentUserId;
                    viewModel.City.CreatedOn = DateTimeHelper.GetPakistanStandardTime();
                    viewModel.City.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                    // 4. Attempt to save the data.
                    var success = await _cityService.SaveCityAsync(viewModel.City);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "City created successfully.";
                        return RedirectToAction(nameof(Index)); // Success!
                    }
                    else
                    {
                        // This handles an unexpected failure from the save method.
                        ModelState.AddModelError("", "A database error occurred and the asset could not be saved.");
                    }
                }

                // 5. If we reach here, it means validation failed. Reload dropdowns and return the view.
                await LoadDropdowns(viewModel);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating city");
                ModelState.AddModelError("", "An error occurred while creating the city.");
                await LoadDropdowns(viewModel);
                return View(viewModel);
            }
        }

        // This helper method ensures dropdowns are reloaded correctly on failure.
        private async Task LoadDropdowns(CityCreateEditViewModel viewModel)
        {
            var regions = await _cityService.GetRegionsAsync();
            viewModel.Regions = regions.Select(r => new SelectListItem
            {
                Value = r.RegionId.ToString(),
                Text = r.RegionName,
                // Ensure the previously selected region is re-selected after a validation error
                Selected = r.RegionId == viewModel.City.RegionId
            }).ToList();
        }

        // GET: Master/City/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var city = await _cityService.GetCityByIdAsync(id);
                if (city == null)
                {
                    return NotFound();
                }

                var regions = await _cityService.GetRegionsAsync();
                var viewModel = new CityCreateEditViewModel
                {
                    City = city,
                    Regions = regions.Select(r => new SelectListItem
                    {
                        Value = r.RegionId.ToString(),
                        Text = r.RegionName,
                        Selected = r.RegionId == city.RegionId
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading city for edit with ID {CityId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the city.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/City/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, CityCreateEditViewModel viewModel)
        {
            if (id != viewModel.City.CityId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Check if city code already exists (excluding current record)
                    if (await _cityService.CityExistsAsync(viewModel.City.CityCode, id))
                    {
                        ModelState.AddModelError("City.CityCode", "A city with this code already exists.");
                    }
                    else
                    {
                        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                        var success = await _cityService.SaveCityAsync(viewModel.City);

                        if (success)
                        {
                            TempData["SuccessMessage"] = "City updated successfully.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to update city.");
                        }
                    }
                }

                // Reload regions if model state is invalid
                var regions = await _cityService.GetRegionsAsync();
                viewModel.Regions = regions.Select(r => new SelectListItem
                {
                    Value = r.RegionId.ToString(),
                    Text = r.RegionName,
                    Selected = r.RegionId == viewModel.City.RegionId
                }).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating city with ID {CityId}", id);
                ModelState.AddModelError("", "An error occurred while updating the city.");

                // Reload regions
                var regions = await _cityService.GetRegionsAsync();
                viewModel.Regions = regions.Select(r => new SelectListItem
                {
                    Value = r.RegionId.ToString(),
                    Text = r.RegionName,
                    Selected = r.RegionId == viewModel.City.RegionId
                }).ToList();

                return View(viewModel);
            }
        }

        // GET: Master/City/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var city = await _cityService.GetCityByIdAsync(id);
                if (city == null)
                {
                    return NotFound();
                }

                return View(city);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading city for delete with ID {CityId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the city.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/City/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _cityService.DeleteCityAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "City deleted successfully.";
                    return Json(new { success = true, message = "City deleted successfully." });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete city.";
                    return Json(new { success = false, message = "Failed to delete city. It may have already been removed." });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting city with ID {CityId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the city.";
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX endpoint for checking if city code exists
        [HttpGet]
        public async Task<JsonResult> CheckCityCodeExists(string cityCode, short? excludeId = null)
        {
            try
            {
                var exists = await _cityService.CityExistsAsync(cityCode, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking city code existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}