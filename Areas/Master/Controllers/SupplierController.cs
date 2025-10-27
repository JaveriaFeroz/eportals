using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Helpers;
using System.Security.Claims;


[Area("Master")]
[Authorize]
public class SupplierController : Controller
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<SupplierController> _logger;

    public SupplierController(
        ISupplierService supplierService,
        ILogger<SupplierController> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    // This helper method now correctly populates the ViewModel, not ViewBag
    private async Task   LoadDropdowns(SupplierCreateEditViewModel viewModel)
    {
        var lookups = await _supplierService.GetLookupsAsync();
        viewModel.SupplierTypes = lookups.SupplierTypes.Select(st => new SelectListItem
        {
            Text = st.SupplierTypeName,
            Value = st.TypeId.ToString()
        }).ToList();

        viewModel.Cities = lookups.Cities.Select(c => new SelectListItem
        {
            Text = c.CityName,
            Value = c.CityId.ToString()
        }).ToList();
    }

    // GET: Master/Supplier
    public async Task<IActionResult> Index(bool showInactive = false, string? searchTerm = null)
    {
        try
        {
            // This now behaves identically to your AccessorialChargeController.
            // By default, 'showInactive' is false, so the service will return ALL records.
            var suppliers = await _supplierService.GetSuppliersAsync(showInactive, searchTerm);

            var viewModel = new SupplierIndexViewModel
            {
                Suppliers = suppliers.ToList(),
                ShowInactiveOnly = showInactive, // Your ViewModel is unchanged
                SearchTerm = searchTerm,
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading suppliers");
            TempData["ErrorMessage"] = "An error occurred while loading the suppliers.";
            return View(new SupplierIndexViewModel());
        }
    }
    // --- CORRECTED GET CREATE ACTION ---
    // GET: Master/Supplier/Create
    public async Task<IActionResult> Create()
    {
        // 1. Create the container ViewModel that the View expects
        var viewModel = new SupplierCreateEditViewModel();

        // 2. Load dropdown data into the ViewModel
        await LoadDropdowns(viewModel);

        // 3. Return the complete container ViewModel
        return View(viewModel);
    }

    // POST: Master/Supplier/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierCreateEditViewModel viewModel)
    {
        try
        {
            if (await _supplierService.SupplierExistsAsync(viewModel.Supplier.SupplierName))
            {
                ModelState.AddModelError("Supplier.SupplierName", "A supplier with this name already exists.");
            }
            if (await _supplierService.SupplierEmailExistsAsync(viewModel.Supplier.Email))
            {
                ModelState.AddModelError("Supplier.Email", "This email address is used by another supplier.");
            }
            if (await _supplierService.SupplierEmailExistsAsync(viewModel.Supplier.PhoneNo))
            {
                ModelState.AddModelError("Supplier.PhoneNo", "This phone number is used by another supplier.");
            }
            if ( await _supplierService.SupplierEmailExistsAsync(viewModel.Supplier.ControlSupplierId))
            {
                ModelState.AddModelError("Supplier.ControlSupplierId", "This Control ID is used by another supplier.");
            }



            if (ModelState.IsValid)
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                viewModel.Supplier.CreatedBy = currentUserId;
                viewModel.Supplier.UpdatedBy = currentUserId;
                viewModel.Supplier.CreatedOn = DateTimeHelper.GetPakistanStandardTime();
                viewModel.Supplier.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                if (await _supplierService.SaveSupplierAsync(viewModel.Supplier))
                {
                    TempData["SuccessMessage"] = "Supplier created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "A database error occurred.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier.");
            ModelState.AddModelError("", "An unexpected error occurred.");
        }

        await LoadDropdowns(viewModel);
        return View(viewModel);
    }

    // GET: Master/Supplier/Edit/5
    // This action is called by the modal's AJAX request to get the edit form.
    public async Task<IActionResult> Edit(short id)
    {
        try
        {
            var supplierData = await _supplierService.GetSupplierByIdAsync(id);
            if (supplierData == null)
            {
                return NotFound();
            }

            // 1. Create the container ViewModel that the View expects
            var viewModel = new SupplierCreateEditViewModel { Supplier = supplierData };

            // 2. Load dropdown data into the ViewModel
            await LoadDropdowns(viewModel);

            // 3. Return a PARTIAL VIEW, which is perfect for modals
            return PartialView(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading supplier for edit with ID {id}", id);
            // For AJAX, return an error message that can be displayed in the modal
            return Content("<div class='alert alert-danger'>An error occurred while loading the data.</div>");
        }
    }


    // POST: Master/Supplier/Edit/5
    // This action handles the AJAX form submission from the Edit modal.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(short id, SupplierCreateEditViewModel viewModel)
    {
        if (id != viewModel.Supplier.SupplierId)
        {
            return NotFound();
        }

        // Perform any custom validation before checking ModelState
        if (await _supplierService.SupplierExistsAsync(viewModel.Supplier.SupplierName, id))
        {
            ModelState.AddModelError("Supplier.SupplierName", "This name is used by another supplier.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Set the audit properties for the update
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                viewModel.Supplier.UpdatedBy = currentUserId;
                viewModel.Supplier.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                var success = await _supplierService.SaveSupplierAsync(viewModel.Supplier);

                if (success)
                {
                    // On success, return a JSON response for the AJAX call
                    return Json(new { success = true, message = "Supplier updated successfully." });
                }

                ModelState.AddModelError("", "A database error occurred and the supplier could not be saved.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier with ID {id}", id);
                ModelState.AddModelError("", "An unexpected error occurred while updating the supplier.");
            }
        }

        // If we reach here, validation failed. Reload dropdowns and return the Partial View with errors.
        await LoadDropdowns(viewModel);
        return PartialView(viewModel);
    }

    // GET: Master/Supplier/Details/5
    public async Task<IActionResult> Details(short id)
    {
        try
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading supplier details for ID {SupplierId}", id);
            TempData["ErrorMessage"] = "An error occurred while loading the supplier details.";
            return RedirectToAction(nameof(Index));
        }
    }


// GET: Master/Supplier/Delete/5
public async Task<IActionResult> Delete(short id)
    {
        try
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading supplier for delete with ID {SupplierId}", id);
            TempData["ErrorMessage"] = "An error occurred while loading the supplier.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(short id)
    {
        try
        {
            var success = await _supplierService.DeleteSupplierAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = "Supplier deleted successfully.";
                return Json(new { success = true, message = "Supplier deleted successfully." });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete supplier.";
                return Json(new { success = false, message = "Failed to delete supplier. It may have already been removed." });
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier with ID {SupplierId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the supplier.";
            return RedirectToAction(nameof(Index));
        }
    }
    // AJAX endpoint for checking if supplier exists
    [HttpGet]
    public async Task<JsonResult> CheckSupplierExists(string supplierName, short? excludeId = null)
    {
        try
        {
            var exists = await _supplierService.SupplierExistsAsync(supplierName, excludeId);
            return Json(new { exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking supplier existence");
            return Json(new { exists = false, error = true });
        }
    }

    // API endpoints for compatibility with existing API consumers
    [HttpGet]
    [Route("api/suppliers")]
    public async Task<JsonResult> GetSuppliers()
    {
        try
        {
            var suppliers = await _supplierService.GetSuppliersAsync();
            return Json(suppliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suppliers via API");
            return Json(new { error = ex.Message });
        }
    }

    [HttpGet]
    [Route("api/suppliers/fuel")]
    public async Task<JsonResult> GetFuelSuppliers()
    {
        try
        {
            var suppliers = await _supplierService.GetFuelSuppliersAsync();
            return Json(suppliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fuel suppliers via API");
            return Json(new { error = ex.Message });
        }
    }

    [HttpGet]
    [Route("api/suppliers/vehicles")]
    public async Task<JsonResult> GetVehicleSuppliers()
    {
        try
        {
            var suppliers = await _supplierService.GetOutSourcedVehicleSuppliersAsync();
            return Json(suppliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicle suppliers via API");
            return Json(new { error = ex.Message });
        }
    }

    [HttpGet]
    [Route("api/suppliers/lookups")]
    public async Task<JsonResult> GetLookups()
    {
        try
        {
            var lookups = await _supplierService.GetLookupsAsync();
            return Json(lookups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lookups via API");
            return Json(new { error = ex.Message });
        }
    }
}