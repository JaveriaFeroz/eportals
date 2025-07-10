using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels; // Ensure you have a using for ViewModels
using System.Security.Claims;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class DriverController : Controller
    {
        private readonly IDriverService _driverService;
        private readonly ILogger<DriverController> _logger;

        public DriverController(IDriverService driverService, ILogger<DriverController> logger)
        {
            _driverService = driverService;
            _logger = logger;
        }


        private async Task LoadDropdowns(DriverCreateEditViewModel viewModel)
        {
            var lookups = await _driverService.GetLookupsAsync();

            viewModel.Companies = lookups.Companies.Select(c => new SelectListItem
            {
                Text = c.CompanyName,
                Value = c.CompanyId.ToString()
            }).ToList();

            viewModel.Branches = lookups.Branches.Select(c => new SelectListItem
            {
                Text = c.BranchName,
                Value = c.BranchId.ToString()
            }).ToList();
           

            viewModel.Contractors = lookups.Contractors.Select(c => new SelectListItem
            {
                Text = c.ContractorName,
                Value = c.ContractorId.ToString()
            }).ToList();

            viewModel.Qualifications = lookups.Qualifications.Select(c => new SelectListItem
            {
                Text = c.QualificationName,
                Value = c.QualificationId.ToString()
            }).ToList();

            viewModel.NOKRelations = lookups.NOKRelations.Select(c => new SelectListItem
            {
                Text = c.RelationName,
                Value = c.RelationId.ToString()
            }).ToList();

            viewModel.SeparationTypes = lookups.SeparationTypes.Select(c => new SelectListItem
            {
                Text = c.SeparationTypeName,
                Value = c.SeparationTypeId.ToString()
            }).ToList();
           
        }

        // 1. INDEX
        // GET: Master/Driver
        public async Task<IActionResult> Index(bool showInactiveOnly = false, string? searchTerm = null)
        {
            try
            {
                var drivers = await _driverService.GetDriversAsync(showInactiveOnly, searchTerm);
                var viewModel = new DriverIndexViewModel
                {
                    Drivers = drivers.ToList(),
                    ShowInactiveOnly = showInactiveOnly,
                    SearchTerm = searchTerm
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading drivers index");
                TempData["ErrorMessage"] = "Error loading drivers. Please try again.";
                return View(new DriverIndexViewModel());
            }
        }

        // 2. CREATE
        // GET: Master/Driver/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new DriverCreateEditViewModel();

            await LoadDropdowns(viewModel);

            return View(viewModel);
        }

        // POST: Master/Driver/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DriverCreateEditViewModel model) // FIX: Parameter is now the container ViewModel
        {
            try
            {
                // Since the parameter is 'model', we access the driver data via 'model.Driver'
                if (ModelState.IsValid)
                {
                    if (await _driverService.DriverNameExistsAsync(model.Driver.DriverName))
                    {
                        ModelState.AddModelError("Driver.DriverName", "Driver name already exists.");
                    }
                    else
                    {
                        // Set audit properties on the correct object
                        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        model.Driver.CreatedBy = currentUserId;
                        model.Driver.UpdatedBy = currentUserId;
                        model.Driver.CreatedOn = DateTime.UtcNow;
                        model.Driver.UpdatedOn = DateTime.UtcNow;

                        // Pass the correct object (model.Driver) to the service
                        var success = await _driverService.SaveDriverAsync(model.Driver);

                        if (success)
                        {
                            TempData["SuccessMessage"] = "Driver created successfully.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to create driver. Please try again.");
                        }
                    }
                }

                // If we reach here, validation failed. We must reload the dropdowns
                // before returning the view, otherwise it will crash.
                // await LoadDropdowns(model); // You need a LoadDropdowns method here
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating driver");
                TempData["ErrorMessage"] = "Error creating driver. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 3. EDIT
        // GET: Master/Driver/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            var driver = await _driverService.GetDriverByIdAsync(id);
            if (driver == null)
            {
                return NotFound();
            }

            var viewModel = new DriverCreateEditViewModel { Driver = driver };
            await LoadDropdowns(viewModel);

            return View(viewModel); // Assuming Edit uses a full view like Create.
        }


        // POST: Master/Driver/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, DriverCreateEditViewModel viewModel)
        {
            if (id != viewModel.Driver.DriverId)
            {
                return NotFound();
            }

            // Perform any custom validation before checking ModelState
            if (await _driverService.DriverNameExistsAsync(viewModel.Driver.DriverName, id))
            {
                ModelState.AddModelError("Driver.DriverName", "This name is used by another driver.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set the audit properties for the update
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    viewModel.Driver.UpdatedBy = currentUserId;
                    viewModel.Driver.UpdatedOn = DateTime.UtcNow;

                    var success = await _driverService.SaveDriverAsync(viewModel.Driver);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Driver updated successfully.";
                        return Json(new { success = true, message = "Driver updated successfully." });
                    }

                    ModelState.AddModelError("", "A database error occurred and the driver could not be saved.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating driver with ID {id}", id);
                    ModelState.AddModelError("", "An unexpected error occurred while updating the driver.");
                }
            }

            // If we reach here, validation failed. Reload dropdowns and return the Partial View with errors.
            await LoadDropdowns(viewModel);
            return PartialView(viewModel);
        }

        // 4. DETAILS
        // GET: Master/Driver/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var driver = await _driverService.GetDriverByIdAsync(id);
                if (driver == null) return NotFound();
                return View(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading driver details for ID {DriverId}", id);
                TempData["ErrorMessage"] = "Error loading driver details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 5. DELETE
        // GET: Master/Driver/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var driver = await _driverService.GetDriverByIdAsync(id);
                if (driver == null) return NotFound();
                return View(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete driver page for ID {DriverId}", id);
                TempData["ErrorMessage"] = "Error loading delete page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Driver/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _driverService.DeleteDriverAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Driver deleted successfully.";
                    return Json(new { success = true, message = "Driver deleted successfully." });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete driver.";
                    return Json(new { success = false, message = "Failed to delete driver. It may have already been removed." });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting driver with ID {DriverId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the driver.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckDriverNameExists(string driverName, short? excludeId = null)
        {
            try
            {
                var exists = await _driverService.DriverNameExistsAsync(driverName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking driver name existence");
                return Json(new { error = "Error checking driver name" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLookups()
        {
            try
            {
                var lookups = await _driverService.GetLookupsAsync();
                return Json(lookups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lookups");
                return Json(new { error = "Error retrieving lookups" });
            }
        }
    }
}