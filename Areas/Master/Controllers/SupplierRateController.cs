using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels;
using System.Security.Claims;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class SupplierRateController : Controller
    {
        private readonly ISupplierRateService _supplierRateService;
        private readonly ILogger<SupplierRateController> _logger;

        public SupplierRateController(
            ISupplierRateService supplierRateService,
            ILogger<SupplierRateController> logger)
        {
            _supplierRateService = supplierRateService;
            _logger = logger;
        }

        // GET: Master/SupplierRate
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var supplierRates = await _supplierRateService.GetSupplierRatesAsync(!showInactive, searchTerm);

                var viewModel = new SupplierRateIndexViewModel
                {
                    SupplierRates = supplierRates.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading supplier rates");
                TempData["ErrorMessage"] = "An error occurred while loading the supplier rates.";
                return View(new SupplierRateIndexViewModel());
            }
        }

        // GET: Master/SupplierRate/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var supplierRate = await _supplierRateService.GetSupplierRateByIdAsync(id);
                if (supplierRate == null)
                {
                    return NotFound();
                }

                return View(supplierRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading supplier rate details for ID {SupplierRateId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the supplier rate details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/SupplierRate/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new SupplierRateViewModel();
                await PopulateLookupsAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing create supplier rate view");
                TempData["ErrorMessage"] = "An error occurred while preparing the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/SupplierRate/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierRateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if supplier rate already exists
                    if (await _supplierRateService.SupplierRateExistsAsync(model.SupplierId))
                    {
                        ModelState.AddModelError("SupplierId", "A rate configuration for this supplier already exists.");
                        await PopulateLookupsAsync();
                        return View(model);
                    }

                    var success = await _supplierRateService.SaveSupplierRateAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Supplier rate created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create supplier rate.");
                    }
                }

                await PopulateLookupsAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier rate");
                ModelState.AddModelError("", "An error occurred while creating the supplier rate.");
                await PopulateLookupsAsync();
                return View(model);
            }
        }

        // GET: Master/SupplierRate/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var supplierRate = await _supplierRateService.GetSupplierRateByIdAsync(id);
                if (supplierRate == null)
                {
                    return NotFound();
                }

                await PopulateLookupsAsync();
                return View(supplierRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading supplier rate for edit with ID {SupplierRateId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the supplier rate.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/SupplierRate/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierRateViewModel model)
        {
            if (id != model.SupplierRateId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Check if supplier rate already exists (excluding current record)
                    if (await _supplierRateService.SupplierRateExistsAsync(model.SupplierId, id))
                    {
                        ModelState.AddModelError("SupplierId", "A rate configuration for this supplier already exists.");
                        await PopulateLookupsAsync();
                        return View(model);
                    }

                    var success = await _supplierRateService.SaveSupplierRateAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Supplier rate updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update supplier rate.");
                    }
                }

                await PopulateLookupsAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier rate with ID {SupplierRateId}", id);
                ModelState.AddModelError("", "An error occurred while updating the supplier rate.");
                await PopulateLookupsAsync();
                return View(model);
            }
        }

        // GET: Master/SupplierRate/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var supplierRate = await _supplierRateService.GetSupplierRateByIdAsync(id);
                if (supplierRate == null)
                {
                    return NotFound();
                }

                return View(supplierRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading supplier rate for delete with ID {SupplierRateId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the supplier rate.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/SupplierRate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _supplierRateService.DeleteSupplierRateAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Supplier rate deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete supplier rate.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier rate with ID {SupplierRateId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the supplier rate.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/SupplierRate/EditBySupplier/5
        public async Task<IActionResult> EditBySupplier(short supplierId)
        {
            try
            {
                var supplierRate = await _supplierRateService.GetSupplierRateBySupplierIdAsync(supplierId);
                if (supplierRate == null)
                {
                    return NotFound();
                }

                await PopulateLookupsAsync();
                return View("Edit", supplierRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading supplier rate for supplier ID {SupplierId}", supplierId);
                TempData["ErrorMessage"] = "An error occurred while loading the supplier rate.";
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX endpoint for getting supplier rate by supplier ID
        [HttpGet]
        public async Task<JsonResult> GetSupplierRate(short supplierId)
        {
            try
            {
                var supplierRate = await _supplierRateService.GetSupplierRateBySupplierIdAsync(supplierId);
                return Json(supplierRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier rate for supplier ID {SupplierId}", supplierId);
                return Json(new { error = true, message = "An error occurred while loading the supplier rate." });
            }
        }

        // AJAX endpoint for getting lookups
        [HttpGet]
        public async Task<JsonResult> GetLookups()
        {
            try
            {
                var suppliers = await _supplierRateService.GetSuppliersForFuelAsync();
                return Json(new { lstSupplier = suppliers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lookups");
                return Json(new { error = true, message = "An error occurred while loading lookups." });
            }
        }

        // AJAX endpoint for checking if supplier rate exists
        [HttpGet]
        public async Task<JsonResult> CheckSupplierRateExists(short supplierId, int? excludeId = null)
        {
            try
            {
                var exists = await _supplierRateService.SupplierRateExistsAsync(supplierId, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking supplier rate existence");
                return Json(new { exists = false, error = true });
            }
        }

        private async Task PopulateLookupsAsync()
        {
            ViewBag.Suppliers = await _supplierRateService.GetSuppliersForFuelAsync();
        }
    }

    // API Controller for backward compatibility (if needed)
    [Authorize]
    [ApiController]
    [Route("api/Master/[controller]")]
    public class SupplierRateApiController : ControllerBase
    {
        private readonly ISupplierRateService _supplierRateService;
        private readonly ILogger<SupplierRateApiController> _logger;

        public SupplierRateApiController(
            ISupplierRateService supplierRateService,
            ILogger<SupplierRateApiController> logger)
        {
            _supplierRateService = supplierRateService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var suppliers = await _supplierRateService.GetSuppliersForFuelAsync();
                return Ok(suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suppliers for fuel");
                return Conflict(new { Message = ex.Message, FieldName = "GetList" });
            }
        }

        [HttpGet("{supplierId}")]
        public async Task<IActionResult> Get(short supplierId)
        {
            try
            {
                var supplierRate = await _supplierRateService.GetSupplierRateBySupplierIdAsync(supplierId);
                return Ok(supplierRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier rate for supplier ID {SupplierId}", supplierId);
                return Conflict(new { Message = ex.Message, FieldName = "Get" });
            }
        }

        [HttpGet]
        [Route("GetLookups")]
        public async Task<IActionResult> GetLookups()
        {
            try
            {
                var suppliers = await _supplierRateService.GetSuppliersForFuelAsync();
                return Ok(new { lstSupplier = suppliers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lookups");
                return Conflict(new { Message = ex.Message, FieldName = "GetLookups" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SupplierRateViewModel supplierRate)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _supplierRateService.SaveSupplierRateAsync(supplierRate);
                if (success)
                {
                    return Ok("Success");
                }
                else
                {
                    return Conflict(new { Message = "Failed to save supplier rate", FieldName = "Save" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving supplier rate");
                return Conflict(new { Message = ex.Message, FieldName = "Save" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] SupplierRateViewModel supplierRate)
        {
            try
            {
                if (id != supplierRate.SupplierRateId)
                {
                    return BadRequest("ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _supplierRateService.SaveSupplierRateAsync(supplierRate);
                if (success)
                {
                    return Ok("Success");
                }
                else
                {
                    return Conflict(new { Message = "Failed to update supplier rate", FieldName = "Update" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier rate with ID {SupplierRateId}", id);
                return Conflict(new { Message = ex.Message, FieldName = "Update" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _supplierRateService.DeleteSupplierRateAsync(id);
                if (success)
                {
                    return Ok("Success");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier rate with ID {SupplierRateId}", id);
                return Conflict(new { Message = ex.Message, FieldName = "Delete" });
            }
        }
    }
}