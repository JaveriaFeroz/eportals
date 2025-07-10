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
    public class ContractorController : Controller
    {
        private readonly IContractorService _contractorService;
        private readonly ILogger<ContractorController> _logger;

        public ContractorController(
            IContractorService contractorService,
            ILogger<ContractorController> logger)
        {
            _contractorService = contractorService;
            _logger = logger;
        }
        
        // GET: Master/Contractor
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var contractors = await _contractorService.GetContractorsAsync(showInactive, searchTerm);

                var viewModel = new ContractorIndexViewModel
                {
                    Contractors = contractors.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contractors");
                TempData["ErrorMessage"] = "An error occurred while loading the contractors.";
                return View(new ContractorIndexViewModel());
            }
        }

        // GET: Master/Contractor/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var contractor = await _contractorService.GetContractorByIdAsync(id);
                if (contractor == null)
                {
                    return NotFound();
                }

                return View(contractor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contractor details for ID {ContractorId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the contractor details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/Contractor/Create
        public IActionResult Create()
        {
            var model = new ContractorViewModel();
            return View(model);
        }

        // POST: Master/Contractor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractorViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if contractor name already exists
                    if (await _contractorService.ContractorExistsAsync(model.ContractorName))
                    {
                        ModelState.AddModelError("ContractorName", "A contractor with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _contractorService.SaveContractorAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Contractor created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create contractor.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contractor");
                ModelState.AddModelError("", "An error occurred while creating the contractor.");
                return View(model);
            }
        }

        // GET: Master/Contractor/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var contractor = await _contractorService.GetContractorByIdAsync(id);
                if (contractor == null)
                {
                    return NotFound();
                }

                return View(contractor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contractor for edit with ID {ContractorId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the contractor.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Contractor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, ContractorViewModel model)
        {
            if (id != model.ContractorId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _contractorService.ContractorExistsAsync(model.ContractorName, id))
                    {
                        ModelState.AddModelError("ContractorName", "A contractor with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _contractorService.SaveContractorAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Contractor updated successfully.";
                        return Json(new { success = true, message = "Contractor updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update contractor.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contractor with ID {ContractorId}", id);
                ModelState.AddModelError("", "An error occurred while updating the contractor.");
                return PartialView(model);
            }
        }

        // GET: Master/Contractor/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var contractor = await _contractorService.GetContractorByIdAsync(id);
                if (contractor == null)
                {
                    return NotFound();
                }

                return View(contractor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contractor for delete with ID {ContractorId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the contractor.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Contractor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _contractorService.DeleteContractorAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Contractor deleted successfully.";
                    return Json(new { success = true, message = "Contractor deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete contractor." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete contractor with ID {ContractorId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this contractor." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting contractor with ID {ContractorId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the contractor." });
            }
        }

        // AJAX endpoint for checking if contractor name exists
        [HttpGet]
        public async Task<JsonResult> CheckContractorNameExists(string contractorName, short? excludeId = null)
        {
            try
            {
                var exists = await _contractorService.ContractorExistsAsync(contractorName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking contractor name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}