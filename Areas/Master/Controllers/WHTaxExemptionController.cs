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
    public class WHTaxExemptionController : Controller
    {
        private readonly IWHTaxExemptionService _WHtaxExemptionService;
        private readonly ILogger<WHTaxExemptionController> _logger;

        public WHTaxExemptionController(IWHTaxExemptionService WHtaxExemptionService, ILogger<WHTaxExemptionController> logger)
        {
            _WHtaxExemptionService = WHtaxExemptionService;
            _logger = logger;
        }

        private async Task LoadDropdowns(WHTaxExemptionCreateEditViewModel viewModel)
        {
            var lookups = await _WHtaxExemptionService.GetLookupsAsync();
            viewModel.Companies = lookups.Companies.Select(c => new SelectListItem
            {
                Text = c.CompanyName,
                Value = c.CompanyId.ToString()
            }).ToList();
        }

        // FIX: Removed companyId. Now shows all exemptions.
        public async Task<IActionResult> Index(bool showInactiveOnly = false, string? searchTerm = null)
        {
            try
            {
                var WHtaxExemptions = await _WHtaxExemptionService.GetWHTaxExemptionsAsync(!showInactiveOnly, searchTerm);
                var viewModel = new WHTaxExemptionIndexViewModel
                {
                    WHTaxExemptions = WHtaxExemptions.ToList(),
                    ShowInactiveOnly = showInactiveOnly,
                    SearchTerm = searchTerm
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading WHTaxExemptions index");
                TempData["ErrorMessage"] = "Error loading WHTaxExemptions. Please try again.";
                // FIX: Corrected the view model type in the catch block.
                return View(new WHTaxExemptionIndexViewModel());
            }
        }

        // FIX: Removed companyId.
        public async Task<IActionResult> Create()
        {
            var viewModel = new WHTaxExemptionCreateEditViewModel();
            await LoadDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WHTaxExemptionCreateEditViewModel model)
        {
            try
            {
                // FIX: Validation logic now matches SKU pattern. CompanyId comes from the model.
                if (await _WHtaxExemptionService.DateRangeOverlapsAsync(model.WHTaxExemptions.DateFrom, model.WHTaxExemptions.DateTo, model.WHTaxExemptions.CompanyId))
                {
                    ModelState.AddModelError("WHTaxExemption.DateTo", "The selected date range overlaps with an existing exemption for this company.");
                }

                if (ModelState.IsValid)
                {
                    // FIX: Service call simplified, just like in SKUController.
                    var success = await _WHtaxExemptionService.SaveWHTaxExemptionAsync(model.WHTaxExemptions);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "WHTaxExemption created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError("", "Failed to create WHTaxExemption. Please try again.");
                }

                await LoadDropdowns(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating WHTaxExemption");
                TempData["ErrorMessage"] = "Error creating WHTaxExemption. Please try again.";
                await LoadDropdowns(model);
                return View(model);
            }
        }

        // FIX: Removed companyId.
        public async Task<IActionResult> Edit(short id)
        {
            var WHtaxExemption = await _WHtaxExemptionService.GetWHTaxExemptionByIdAsync(id);
            if (WHtaxExemption == null)
            {
                return NotFound();
            }

            var viewModel = new WHTaxExemptionCreateEditViewModel ();
            await LoadDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // FIX: Removed companyId.
        public async Task<IActionResult> Edit(short id, WHTaxExemptionCreateEditViewModel viewModel)
        {
            if (id != viewModel.WHTaxExemptions.ExemptionId)
            {
                return BadRequest();
            }

            try
            {
                // FIX: Pass companyId from the view model for overlap check.
                if (await _WHtaxExemptionService.DateRangeOverlapsAsync(viewModel.WHTaxExemptions.DateFrom, viewModel.WHTaxExemptions.DateTo, viewModel.WHTaxExemptions.CompanyId, id))
                {
                    ModelState.AddModelError("WHTaxExemption.DateTo", "The selected date range overlaps with an existing exemption for this company.");
                }

                if (ModelState.IsValid)
                {
                    var success = await _WHtaxExemptionService.SaveWHTaxExemptionAsync(viewModel.WHTaxExemptions);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "WHTaxExemption updated successfully.";
                        // FIX: Simplified redirect.
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError("", "Failed to update the WHTaxExemption.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating WHTaxExemption with ID {Id}", id);
                ModelState.AddModelError("", "An unexpected error occurred while updating.");
            }

            await LoadDropdowns(viewModel);
            return View(viewModel);
        }

        public async Task<IActionResult> Details(short id)
        {
            var wHTaxExemption = await _WHtaxExemptionService.GetWHTaxExemptionByIdAsync(id);
            if (wHTaxExemption == null)
            {
                return NotFound();
            }
            return View(wHTaxExemption);
        }

        public async Task<IActionResult> Delete(short id)
        {
            var wHTaxExemption = await _WHtaxExemptionService.GetWHTaxExemptionByIdAsync(id);
            if (wHTaxExemption == null)
            {
                return NotFound();
            }
            return View(wHTaxExemption);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                await _WHtaxExemptionService.DeleteWHTaxExemptionAsync(id);
                TempData["SuccessMessage"] = "WHTaxExemption deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting WHTaxExemption with ID {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the WHTaxExemption.";
            }
            // FIX: Simplified redirect.
            return RedirectToAction(nameof(Index));
        }
    }
}