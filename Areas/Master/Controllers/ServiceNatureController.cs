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
    public class ServiceNatureController : Controller
    {
        private readonly IServiceNatureService _serviceNatureService;
        private readonly ILogger<ServiceNatureController> _logger;

        public ServiceNatureController(IServiceNatureService serviceNatureService, ILogger<ServiceNatureController> logger)
        {
            _serviceNatureService = serviceNatureService;
            _logger = logger;
        }




        // 1. INDEX
        // GET: Master/ServiceNature
        public async Task<IActionResult> Index(bool showInactiveOnly = false, string? searchTerm = null)
        {
            try
            {
                var serviceNatures = await _serviceNatureService.GetServiceNaturesAsync(showInactiveOnly, searchTerm);
                var viewModel = new ServiceNatureIndexViewModel
                {
                    ServiceNatures = serviceNatures.ToList(),
                    ShowInactiveOnly = showInactiveOnly,
                    SearchTerm = searchTerm
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading serviceNatures index");
                TempData["ErrorMessage"] = "Error loading serviceNatures. Please try again.";
                return View(new ServiceNatureIndexViewModel());
            }
        }

        // 2. CREATE
        // GET: Master/ServiceNature/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new ServiceNatureCreateEditViewModel();


            return View(viewModel);
        }

        // POST: Master/ServiceNature/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceNatureCreateEditViewModel model) // FIX: Parameter is now the container ViewModel
        {
            try
            {
                // Since the parameter is 'model', we access the serviceNature data via 'model.ServiceNature'
                if (ModelState.IsValid)
                {
                    if (await _serviceNatureService.ServiceNatureExistsAsync(model.ServiceNature.NatureName))
                    {
                        ModelState.AddModelError("ServiceNature.NatureName", "ServiceNature name already exists.");
                    }
                    else
                    {
                        // Set audit properties on the correct object
                        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        model.ServiceNature.CreatedBy = currentUserId;
                        model.ServiceNature.UpdatedBy = currentUserId;
                        model.ServiceNature.CreatedOn = DateTime.UtcNow;
                        model.ServiceNature.UpdatedOn = DateTime.UtcNow;

                        // Pass the correct object (model.ServiceNature) to the service
                        var success = await _serviceNatureService.SaveServiceNatureAsync(model.ServiceNature);

                        if (success)
                        {
                            TempData["SuccessMessage"] = "ServiceNature created successfully.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to create serviceNature. Please try again.");
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
                _logger.LogError(ex, "Error creating serviceNature");
                TempData["ErrorMessage"] = "Error creating serviceNature. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 3. EDIT
        // GET: Master/ServiceNature/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            var serviceNature = await _serviceNatureService.GetServiceNatureByIdAsync(id);
            if (serviceNature == null)
            {
                return NotFound();
            }

            var viewModel = new ServiceNatureCreateEditViewModel { ServiceNature = serviceNature };

            return View(viewModel); // Assuming Edit uses a full view like Create.
        }


        // POST: Master/ServiceNature/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, ServiceNatureCreateEditViewModel viewModel)
        {
            if (id != viewModel.ServiceNature.NatureId)
            {
                return NotFound();
            }

            // Perform any custom validation before checking ModelState
            if (await _serviceNatureService.ServiceNatureExistsAsync(viewModel.ServiceNature.NatureName, id))
            {
                ModelState.AddModelError("ServiceNature.NatureName", "This name is used by another serviceNature.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set the audit properties for the update
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    viewModel.ServiceNature.UpdatedBy = currentUserId;
                    viewModel.ServiceNature.UpdatedOn = DateTime.UtcNow;

                    var success = await _serviceNatureService.SaveServiceNatureAsync(viewModel.ServiceNature);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "ServiceNature updated successfully.";
                        return Json(new { success = true, message = "ServiceNature updated successfully." });
                    }

                    ModelState.AddModelError("", "A database error occurred and the serviceNature could not be saved.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating serviceNature with ID {id}", id);
                    ModelState.AddModelError("", "An unexpected error occurred while updating the serviceNature.");
                }
            }

            // If we reach here, validation failed. Reload dropdowns and return the Partial View with errors.
            return PartialView(viewModel);
        }

        // 4. DETAILS
        // GET: Master/ServiceType/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var serviceNature = await _serviceNatureService.GetServiceNatureByIdAsync(id);
                if (serviceNature == null)
                {
                    return NotFound();
                }

                return View(serviceNature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service type details for ID {NatureId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the service Nature details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 5. DELETE
        // GET: Master/ServiceNature/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var serviceNature = await _serviceNatureService.GetServiceNatureByIdAsync(id);
                if (serviceNature == null) return NotFound();
                return View(serviceNature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete serviceNature page for ID {NatureId}", id);
                TempData["ErrorMessage"] = "Error loading delete page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/ServiceNature/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _serviceNatureService.DeleteServiceNatureAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Service Nature deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete service nature.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service nature with ID {NatureId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the service nature.";
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        public async Task<IActionResult> CheckNatureNameExists(string serviceNatureName, short? excludeId = null)
        {
            try
            {
                var exists = await _serviceNatureService.ServiceNatureExistsAsync(serviceNatureName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking serviceNature name existence");
                return Json(new { error = "Error checking serviceNature name" });
            }
        }

    }
}