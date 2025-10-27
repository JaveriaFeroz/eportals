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
    public class ServiceController : Controller
    {
        private readonly IServiceService _serviceService;
        private readonly ILogger<ServiceController> _logger;

        public ServiceController(
            IServiceService serviceService,
            ILogger<ServiceController> logger)
        {
            _serviceService = serviceService;
            _logger = logger;
        }

        // GET: Master/Service
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var services = await _serviceService.GetServicesAsync(showInactive, searchTerm);

                var viewModel = new ServiceIndexViewModel
                {
                    Services = services.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading services");
                TempData["ErrorMessage"] = "An error occurred while loading the services.";
                return View(new ServiceIndexViewModel());
            }
        }

        // GET: Master/Service/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var service = await _serviceService.GetServiceByIdAsync(id);
                if (service == null)
                {
                    return NotFound();
                }

                return View(service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service details for ID {ServiceId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the service details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/Service/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new ServiceViewModel();
                var lookups = await _serviceService.GetLookupsAsync();

                // Load dropdowns
                model.UoMs = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(lookups.UoMs, "UoMId", "UoMName");
                model.ServiceNatures = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(lookups.ServiceNatures, "NatureId", "NatureName");

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create service form");
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Service/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if service name already exists
                    if (await _serviceService.ServiceExistsAsync(model.ServiceName))
                    {
                        ModelState.AddModelError("ServiceName", "A service with this name already exists.");
                        await LoadDropdownsForModel(model);
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _serviceService.SaveServiceAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Service created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create service.");
                    }
                }

                await LoadDropdownsForModel(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service");
                ModelState.AddModelError("", "An error occurred while creating the service.");
                await LoadDropdownsForModel(model);
                return View(model);
            }
        }

        // GET: Master/Service/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var service = await _serviceService.GetServiceByIdAsync(id);
                if (service == null)
                {
                    return NotFound();
                }

                return View(service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service for edit with ID {ServiceId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the service.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Service/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, ServiceViewModel model)
        {
            if (id != model.ServiceId)
            {
                return NotFound();
            }

            var originalService = await _serviceService.GetServiceByIdAsync(id);
            // Assuming GetProductByIdAsync returns the existing product details 
            // or ProductViewModel containing the original price.

            if (originalService == null)
            {
                // Should not happen if the product ID is valid, but good practice to check.
                ModelState.AddModelError("", "Serv9ce not found.");
                await LoadDropdownsForModel(model);
                return PartialView(model);
            }

            try
            {
                if (model.UnitPrice > originalService.UnitPrice)
                {
                    ModelState.AddModelError("UnitPrice", "Please raise bid to increase the price. Unit Price cannot be increased manually.");
                }

                if (ModelState.IsValid)
                {
                    // Check if service name already exists (excluding current record)
                    if (await _serviceService.ServiceExistsAsync(model.ServiceName, id))
                    {
                        ModelState.AddModelError("ServiceName", "A service with this name already exists.");
                        await LoadDropdownsForModel(model);
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _serviceService.SaveServiceAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Service updated successfully.";
                        return Json(new { success = true, message = "Service updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update service.");
                    }
                }

                await LoadDropdownsForModel(model);
                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service with ID {ServiceId}", id);
                ModelState.AddModelError("", "An error occurred while updating the service.");
                await LoadDropdownsForModel(model);
                return PartialView(model);
            }
        }


        // GET: Master/Service/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var service = await _serviceService.GetServiceByIdAsync(id);
                if (service == null)
                {
                    return NotFound();
                }

                return View(service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service for delete with ID {ServiceId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the service.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _serviceService.DeleteServiceAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Service deleted successfully.";
                    return Json(new { success = true, message = "Service deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete service." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete service with ID {ServiceId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this service." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting service with ID {ServiceId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the service." });
            }
        }

        // AJAX endpoint for checking if service name exists
        [HttpGet]
        public async Task<JsonResult> CheckServiceNameExists(string serviceName, short? excludeId = null)
        {
            try
            {
                var exists = await _serviceService.ServiceExistsAsync(serviceName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking service name existence");
                return Json(new { exists = false, error = true });
            }
        }

        // API endpoint for getting lookups (similar to your original GetLookups)
        [HttpGet]
        public async Task<JsonResult> GetLookups()
        {
            try
            {
                var lookups = await _serviceService.GetLookupsAsync();
                return Json(new
                {
                    lstUoM = lookups.UoMs,
                    lstServiceNature = lookups.ServiceNatures
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lookups");
                return Json(new { error = ex.Message });
            }
        }

        private async Task LoadDropdownsForModel(ServiceViewModel model)
        {
            try
            {
                var lookups = await _serviceService.GetLookupsAsync();
                model.UoMs = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(lookups.UoMs, "UoMId", "UoMName", model.UoMId);
                model.ServiceNatures = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(lookups.ServiceNatures, "NatureId", "NatureName", model.ServiceNatureId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dropdowns for model");
            }
        }
    }
}