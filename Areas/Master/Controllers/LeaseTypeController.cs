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
    public class LeaseTypeController : Controller
    {
        private readonly ILeaseTypeService _leaseTypeService;
        private readonly ILogger<LeaseTypeController> _logger;

        public LeaseTypeController(
            ILeaseTypeService leaseTypeService,
            ILogger<LeaseTypeController> logger)
        {
            _leaseTypeService = leaseTypeService;
            _logger = logger;
        }

        // GET: Master/LeaseType
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var leaseTypes = await _leaseTypeService.GetLeaseTypesAsync(showInactive, searchTerm);

                var viewModel = new LeaseTypeIndexViewModel
                {
                    LeaseTypes = leaseTypes.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading lease types");
                TempData["ErrorMessage"] = "An error occurred while loading the lease types.";
                return View(new LeaseTypeIndexViewModel());
            }
        }

        // GET: Master/LeaseType/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var leaseType = await _leaseTypeService.GetLeaseTypeByIdAsync(id);
                if (leaseType == null)
                {
                    return NotFound();
                }

                return View(leaseType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading lease type details for ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the lease type details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/LeaseType/Create
        public IActionResult Create()
        {
            var model = new LeaseTypeViewModel();
            return View(model);
        }

        // POST: Master/LeaseType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaseTypeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if lease type name already exists
                    if (await _leaseTypeService.LeaseTypeExistsAsync(model.TypeName))
                    {
                        ModelState.AddModelError("TypeName", "A lease type with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _leaseTypeService.SaveLeaseTypeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Lease type created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create lease type.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lease type");
                ModelState.AddModelError("", "An error occurred while creating the lease type.");
                return View(model);
            }
        }

        // GET: Master/LeaseType/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var leaseType = await _leaseTypeService.GetLeaseTypeByIdAsync(id);
                if (leaseType == null)
                {
                    return NotFound();
                }

                return View(leaseType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading lease type for edit with ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the lease type.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, LeaseTypeViewModel model)
        {
            if (id != model.TypeId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Check if product name already exists (excluding current record)
                    if (await _leaseTypeService.LeaseTypeExistsAsync(model.TypeName, id))
                    {
                        ModelState.AddModelError("TypeName", "A lease type with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _leaseTypeService.SaveLeaseTypeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Lease type updated successfully.";
                        return Json(new { success = true, message = "Lease type updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update lease type.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subCategory with ID {SubCategoryId}", id);
                ModelState.AddModelError("", "An error occurred while updating the subCategory.");
                return PartialView(model);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _leaseTypeService.DeleteLeaseTypeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Lease type deleted successfully.";
                    return Json(new { success = true, message = "Lease type deleted successfully." });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to delete lease type." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete lease type with ID {TypeId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this region because it has associated cities." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting lease type with ID {TypeId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the region." });
            }
        }


        // AJAX endpoint for checking if lease type name exists
        [HttpGet]
        public async Task<JsonResult> CheckTypeNameExists(string typeName, short? excludeId = null)
        {
            try
            {
                var exists = await _leaseTypeService.LeaseTypeExistsAsync(typeName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking lease type name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}