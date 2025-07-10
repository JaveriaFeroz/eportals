using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using ProcureToPay.Areas.Master.Models;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class ShipperController : Controller
    {
        private readonly IShipperService _shipperService;
        private readonly ILogger<ShipperController> _logger;

        public ShipperController(IShipperService shipperService, ILogger<ShipperController> logger)
        {
            _shipperService = shipperService;
            _logger = logger;
        }

        private async Task LoadDropdowns(ShipperCreateEditViewModel viewModel)
        {
            var lookups = await _shipperService.GetLookupsAsync();

            viewModel.Cities = lookups.Cities.Select(c => new SelectListItem
            {
                Text = c.CityName,
                Value = c.CityId.ToString()
            }).ToList();

            viewModel.Companies = lookups.Companies.Select(c => new SelectListItem
            {
                Text = c.CompanyName,
                Value = c.CompanyId.ToString()
            }).ToList();

            // FIX: Populate the Types dropdown
            viewModel.Clients = lookups.Clients.Select(t => new SelectListItem
            {
                Text = t.ClientName,
                Value = t.ClientId.ToString()
            }).ToList();
        }

        public async Task<IActionResult> Index(bool showInactiveOnly = false, string? searchTerm = null)
        {
            try
            {
                var shippers = await _shipperService.GetShippersAsync(showInactiveOnly, searchTerm);
                var viewModel = new ShipperIndexViewModel
                {
                    Shippers = shippers.ToList(),
                    ShowInactiveOnly = showInactiveOnly,
                    SearchTerm = searchTerm
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shippers index");
                TempData["ErrorMessage"] = "Error loading shippers. Please try again.";
                return View(new ShipperIndexViewModel());
            }
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new ShipperCreateEditViewModel();
            await LoadDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShipperCreateEditViewModel model)
        {
            try
            {
                // FIX: Corrected validation logic to check ShipperName.
                if (await _shipperService.ShipperNameExistsAsync(model.Shipper.ShipperName))
                {
                    ModelState.AddModelError("Shipper.ShipperName", "Shipper name already exists.");
                }

                if (ModelState.IsValid)
                {
                    var success = await _shipperService.SaveShipperAsync(model.Shipper);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Shipper created successfully.";
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError("", "Failed to create Shipper. Please try again.");
                }

                await LoadDropdowns(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Shipper");
                TempData["ErrorMessage"] = "Error creating Shipper. Please try again.";
                await LoadDropdowns(model);
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(short id)
        {
            var shipper = await _shipperService.GetShipperByIdAsync(id);
            if (shipper == null)
            {
                return NotFound();
            }

            var viewModel = new ShipperCreateEditViewModel { Shipper = shipper };
            await LoadDropdowns(viewModel);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, ShipperCreateEditViewModel viewModel)
        {
            // FIX: Corrected ID check to use ShipperId.
            if (id != viewModel.Shipper.ShipperId)
            {
                return NotFound();
            }

            // FIX: Corrected validation logic.
            if (await _shipperService.ShipperNameExistsAsync(viewModel.Shipper.ShipperName, id))
            {
                ModelState.AddModelError("Shipper.ShipperName", "This name is used by another Shipper.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set the audit properties for the update
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    viewModel.Shipper.UpdatedBy = currentUserId;
                    viewModel.Shipper.UpdatedOn = DateTime.UtcNow;
                    var success = await _shipperService.SaveShipperAsync(viewModel.Shipper);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Shipper updated successfully.";
                        return Json(new { success = true, message = "Shipper updated successfully." });
                    }
                    ModelState.AddModelError("", "A database error occurred and the Shipper could not be saved.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating Shipper with ID {id}", id);
                    ModelState.AddModelError("", "An unexpected error occurred while updating the Shipper.");
                }
            }

            await LoadDropdowns(viewModel);
            return PartialView(viewModel);
        }

        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var shipper = await _shipperService.GetShipperByIdAsync(id);
                if (shipper == null) return NotFound();
                return View(shipper);
            }
            catch (Exception ex)
            {
                // FIX: Corrected log message.
                _logger.LogError(ex, "Error loading Shipper details for ID {ShipperId}", id);
                TempData["ErrorMessage"] = "Error loading Shipper details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var shipper = await _shipperService.GetShipperByIdAsync(id);
                if (shipper == null) return NotFound();
                return View(shipper);
            }
            catch (Exception ex)
            {
                // FIX: Corrected log message.
                _logger.LogError(ex, "Error loading delete Shipper page for ID {ShipperId}", id);
                TempData["ErrorMessage"] = "Error loading delete page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _shipperService.DeleteShipperAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Shipper deleted successfully.";
                    return Json(new { success = true, message = "Shipper deleted successfully." });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete Shipper.";
                    return Json(new { success = false, message = "Failed to delete Shipper. It may have already been removed." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Shipper with ID {ShipperId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the Shipper.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckShipperNameExists(string shipperName, short? excludeId = null)
        {
            try
            {
                // FIX: Called the correct service method.
                var exists = await _shipperService.ShipperNameExistsAsync(shipperName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Shipper name existence");
                return Json(new { error = "Error checking Shipper name" });
            }
        }
    }
}