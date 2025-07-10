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
    public class SupplierTypeController : Controller
    {
        private readonly ISupplierTypeService _supplierTypeService;
        private readonly ILogger<SupplierTypeController> _logger;

        public SupplierTypeController(
            ISupplierTypeService supplierTypeService,
            ILogger<SupplierTypeController> logger)
        {
            _supplierTypeService = supplierTypeService;
            _logger = logger;
        }

        // GET: Master/SupplierType
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var supplierTypes = await _supplierTypeService.GetSupplierTypesAsync(showInactive, searchTerm);

                var viewModel = new SupplierTypeIndexViewModel
                {
                    SupplierTypes = supplierTypes.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading supplier types");
                TempData["ErrorMessage"] = "An error occurred while loading the supplier types.";
                return View(new SupplierTypeIndexViewModel());
            }
        }

        // GET: Master/SupplierType/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var supplierType = await _supplierTypeService.GetSupplierTypeByIdAsync(id);
                if (supplierType == null)
                {
                    return NotFound();
                }

                // Return a PARTIAL VIEW which is perfect for modals
                return PartialView(supplierType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading supplier type details for ID {TypeId}", id);
                // For an AJAX call, return an error message directly
                return Content("<div class='alert alert-danger'>An error occurred while loading the details.</div>");
            }
        }

        // GET: Master/SupplierType/Create
        public IActionResult Create()
        {
            var model = new SupplierTypeViewModel();
            return View(model);
        }

        // POST: Master/SupplierType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierTypeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if supplier type name already exists
                    if (await _supplierTypeService.SupplierTypeExistsAsync(model.SupplierTypeName))
                    {
                        ModelState.AddModelError("TypeName", "A supplier type with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _supplierTypeService.SaveSupplierTypeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Supplier type created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create supplier type.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier type");
                ModelState.AddModelError("", "An error occurred while creating the supplier type.");
                return View(model);
            }
        }

        // GET: Master/SupplierType/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var viewModel = await _supplierTypeService.GetSupplierTypeByIdAsync(id);
                if (viewModel == null)
                {
                    return NotFound();
                }

                return PartialView(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading supplier type for edit with ID {TypeId}", id);
                // For AJAX requests, it's better to return an error status code
                return StatusCode(500, "An error occurred while loading the data.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, SupplierTypeViewModel model)
        {
            // The property name in your ViewModel is likely ChargeId
            if (id != model.TypeId)
            {
                return NotFound();
            }

            if (await _supplierTypeService.SupplierTypeExistsAsync(model.SupplierTypeName, id))
            {
                ModelState.AddModelError("TypeName", "A supplier type with this name already exists.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _supplierTypeService.SaveSupplierTypeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Supplier type updated successfully.";
                        return Json(new { success = true, message = "Supplier type updated successfully." });
                    }
                    else
                    {
                        // This case is for unexpected save failures (database error, etc.)
                        ModelState.AddModelError("", "Failed to update supplier type. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating activity with ID {TypeId}", id);
                    ModelState.AddModelError("", "An unexpected error occurred while updating the supplier type.");
                }
            }

            return PartialView(model);
        }
        // GET: Master/SupplierType/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var supplierType = await _supplierTypeService.GetSupplierTypeByIdAsync(id);
                if (supplierType == null)
                {
                    return NotFound();
                }

                return View(supplierType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading supplier type for delete with ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the supplier type.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/SupplierType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                // Attempt to delete the activity using the service
                var success = await _supplierTypeService.DeleteSupplierTypeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Supplier type deleted successfully.";
                    return Json(new { success = true, message = "Supplier type deleted successfully." });
                }
                else
                {
                    // If the service returns false (e.g., item not found), return a JSON error
                    return Json(new { success = false, message = "Failed to delete Supplier type. It may have already been removed." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Supplier type with ID {ActivityId}", id);

                // On an unexpected server error, return a JSON error
                return Json(new { success = false, message = "An error occurred while deleting the Supplier type." });
            }
        }

        // AJAX endpoint for checking if supplier type name exists
        [HttpGet]
        public async Task<JsonResult> CheckTypeNameExists(string typeName, short? excludeId = null)
        {
            try
            {
                var exists = await _supplierTypeService.SupplierTypeExistsAsync(typeName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking supplier type name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}