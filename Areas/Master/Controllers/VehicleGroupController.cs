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
    public class VehicleGroupController : Controller
    {
        private readonly IVehicleGroupService _vehicleGroupService;
        private readonly ILogger<VehicleGroupController> _logger;

        public VehicleGroupController(
            IVehicleGroupService vehicleGroupService,
            ILogger<VehicleGroupController> logger)
        {
            _vehicleGroupService = vehicleGroupService;
            _logger = logger;
        }

        // GET: Master/VehicleGroup
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var vehicleGroups = await _vehicleGroupService.GetVehicleGroupsAsync(showInactive, searchTerm);

                var viewModel = new VehicleGroupIndexViewModel
                {
                    VehicleGroups = vehicleGroups.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicleGroups");
                TempData["ErrorMessage"] = "An error occurred while loading the vehicleGroups.";
                return View(new VehicleGroupIndexViewModel());
            }
        }

        // GET: Master/VehicleGroup/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var vehicleGroup = await _vehicleGroupService.GetVehicleGroupByIdAsync(id);
                if (vehicleGroup == null)
                {
                    return NotFound();
                }

                return View(vehicleGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicleGroup details for ID {VehicleGroupId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the vehicleGroup details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/VehicleGroup/Create
        public IActionResult Create()
        {
            var model = new VehicleGroupViewModel();
            return View(model);
        }

        // POST: Master/VehicleGroup/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleGroupViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if vehicleGroup name already exists
                    if (await _vehicleGroupService.VehicleGroupExistsAsync(model.GroupName))
                    {
                        ModelState.AddModelError("GroupName", "A vehicleGroup with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _vehicleGroupService.SaveVehicleGroupAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "VehicleGroup created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create vehicleGroup.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicleGroup");
                ModelState.AddModelError("", "An error occurred while creating the vehicleGroup.");
                return View(model);
            }
        }

        // GET: Master/VehicleGroup/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var vehicleGroup = await _vehicleGroupService.GetVehicleGroupByIdAsync(id);
                if (vehicleGroup == null)
                {
                    return NotFound();
                }

                return View(vehicleGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicleGroup for edit with ID {VehicleGroupId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the vehicleGroup.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/VehicleGroup/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, VehicleGroupViewModel model)
        {
            if (id != model.GroupId) 
                
                return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _vehicleGroupService.VehicleGroupExistsAsync(model.GroupName, id))
                    {
                        ModelState.AddModelError("GroupName", "A vehicleGroup with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _vehicleGroupService.SaveVehicleGroupAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "VehicleGroup updated successfully.";
                        return Json(new { success = true, message = "VehicleGroup updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update vehicleGroup.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicleGroup with ID {VehicleGroupId}", id);
                ModelState.AddModelError("", "An error occurred while updating the vehicleGroup.");
                return PartialView(model);
            }
        }

        // GET: Master/VehicleGroup/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var vehicleGroup = await _vehicleGroupService.GetVehicleGroupByIdAsync(id);
                if (vehicleGroup == null)
                {
                    return NotFound();
                }

                return View(vehicleGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicleGroup for delete with ID {VehicleGroupId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the vehicleGroup.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/VehicleGroup/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _vehicleGroupService.DeleteVehicleGroupAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "VehicleGroup deleted successfully.";
                    return Json(new { success = true, message = "VehicleGroup deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete vehicleGroup." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete vehicleGroup with ID {VehicleGroupId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this vehicleGroup." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting vehicleGroup with ID {VehicleGroupId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the vehicleGroup." });
            }
        }

        // AJAX endpoint for checking if vehicleGroup name exists
        [HttpGet]
        public async Task<JsonResult> CheckGroupNameExists(string vehicleGroupName, short? excludeId = null)
        {
            try
            {
                var exists = await _vehicleGroupService.VehicleGroupExistsAsync(vehicleGroupName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking vehicleGroup name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}