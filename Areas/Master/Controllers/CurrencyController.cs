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
    public class CurrencyController : Controller
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(
            ICurrencyService currencyService,
            ILogger<CurrencyController> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        // GET: Master/Currency
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var currencies = await _currencyService.GetCurrenciesAsync(showInactive, searchTerm);

                var viewModel = new CurrencyIndexViewModel
                {
                    Currencies = currencies.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading currencies");
                TempData["ErrorMessage"] = "An error occurred while loading the currencies.";
                return View(new CurrencyIndexViewModel());
            }
        }

        // GET: Master/Currency/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var currency = await _currencyService.GetCurrencyByIdAsync(id);
                if (currency == null)
                {
                    return NotFound();
                }

                return View(currency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading currency details for ID {CurrencyId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the currency details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/Currency/Create
        public IActionResult Create()
        {
            var model = new CurrencyViewModel();
            return View(model);
        }

        // POST: Master/Currency/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CurrencyViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if currency name already exists
                    if (await _currencyService.CurrencyExistsAsync(model.CurrencyName))
                    {
                        ModelState.AddModelError("CurrencyName", "A currency with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _currencyService.SaveCurrencyAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Currency created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create currency.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating currency");
                ModelState.AddModelError("", "An error occurred while creating the currency.");
                return View(model);
            }
        }

        // GET: Master/Currency/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var currency = await _currencyService.GetCurrencyByIdAsync(id);
                if (currency == null)
                {
                    return NotFound();
                }

                return View(currency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading currency for edit with ID {CurrencyId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the currency.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Currency/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, CurrencyViewModel model)
        {
            if (id != model.CurrencyId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _currencyService.CurrencyExistsAsync(model.CurrencyName, id))
                    {
                        ModelState.AddModelError("CurrencyName", "A currency with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _currencyService.SaveCurrencyAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Currency updated successfully.";
                        return Json(new { success = true, message = "Currency updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update currency.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating currency with ID {CurrencyId}", id);
                ModelState.AddModelError("", "An error occurred while updating the currency.");
                return PartialView(model);
            }
        }

        // GET: Master/Currency/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var currency = await _currencyService.GetCurrencyByIdAsync(id);
                if (currency == null)
                {
                    return NotFound();
                }

                return View(currency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading currency for delete with ID {CurrencyId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the currency.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Currency/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _currencyService.DeleteCurrencyAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Currency deleted successfully.";
                    return Json(new { success = true, message = "Currency deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete currency." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete currency with ID {CurrencyId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this currency." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting currency with ID {CurrencyId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the currency." });
            }
        }

        // AJAX endpoint for checking if currency name exists
        [HttpGet]
        public async Task<JsonResult> CheckCurrencyNameExists(string currencyName, short? excludeId = null)
        {
            try
            {
                var exists = await _currencyService.CurrencyExistsAsync(currencyName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking currency name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}