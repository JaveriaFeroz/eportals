using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Inventory.Models; // Models for PurchaseRequisition
using ProcureToPay.Areas.Inventory.Services; // Service for PurchaseRequisition
using System.Security.Claims; // For accessing User claims like NameIdentifier

namespace ProcureToPay.Areas.Inventory.Controllers // Changed area and namespace
{
    [Area("Inventory")] // Define the MVC Area
    [Authorize] // Require authorization for all actions in this controller
    public class PurchaseRequisitionController : Controller
    {
        private readonly IPurchaseRequisitionService _purchaseRequisitionService;
        private readonly ILogger<PurchaseRequisitionController> _logger;

        // Constructor for dependency injection
        public PurchaseRequisitionController(IPurchaseRequisitionService purchaseRequisitionService, ILogger<PurchaseRequisitionController> logger)
        {
            _purchaseRequisitionService = purchaseRequisitionService;
            _logger = logger;
        }

        // Helper method to load dropdown data for Create/Edit views
        private async Task LoadDropdowns(PurchaseRequisitionCreateEditViewModel viewModel)
        {
            var lookups = await _purchaseRequisitionService.GetLookupsAsync();

            viewModel.Branches = lookups.Branches.Select(b => new SelectListItem
            {
                Text = b.BranchName,
                Value = b.BranchId.ToString()
            }).ToList();

            viewModel.Departments = lookups.Departments.Select(d => new SelectListItem
            {
                Text = d.DepartmentName,
                Value = d.DepartmentId.ToString()
            }).ToList();

            viewModel.ProductNatures = new List<SelectListItem>();

            viewModel.PurchaseNatures = lookups.PurchaseNatures.Select(d => new SelectListItem
            {
                Text = d.PurchaseNatureName,
                Value = d.PurchaseNatureId.ToString()
            }).ToList();

            // viewModel.ServiceGroups = lookups.ServiceGroups.Select(sg => new SelectListItem
            // {
            //     Text = sg.ServiceGroupName,
            //     Value = sg.ServiceGroupId.ToString()
            // }).ToList();

            // viewModel.RequestNatures = lookups.RequestNatures.Select(rn => new SelectListItem
            // {
            //     Text = rn.RequestNatureName,
            //     Value = rn.RequestNatureId.ToString()
            // }).ToList();

            // viewModel.RequestTypes = lookups.RequestTypes.Select(rt => new SelectListItem
            // {
            //     Text = rt.RequestTypeName,
            //     Value = rt.RequestTypeId.ToString()
            // }).ToList();

            // viewModel.Workflows = lookups.Workflows.Select(wf => new SelectListItem
            // {
            //     Text = wf.WorkflowName,
            //     Value = wf.WorkflowId.ToString()
            // }).ToList();
        }

        // 1. INDEX
        // GET: Inventory/PurchaseRequisition
        public async Task<IActionResult> Index(bool showCompletedOnly = false, string? searchTerm = null)
        {
            try
            {
                var purchaseRequisitions = await _purchaseRequisitionService.GetPurchaseRequisitionsAsync(showCompletedOnly, searchTerm);
                var viewModel = new PurchaseRequisitionIndexViewModel
                {
                    PurchaseRequisitions = purchaseRequisitions.ToList(),
                    ShowCompletedOnly = showCompletedOnly,
                    SearchTerm = searchTerm
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading purchase requisitions index.");
                TempData["ErrorMessage"] = "Error loading purchase requisitions. Please try again.";
                return View(new PurchaseRequisitionIndexViewModel()); // Return empty model on error
            }
        }

        // 2. CREATE
        // GET: Inventory/PurchaseRequisition/Create
        public async Task<IActionResult> Create()
        {           
            var viewModel = new PurchaseRequisitionCreateEditViewModel();
            viewModel.PurchaseRequisition.Owner = User.Identity?.Name ?? "System"; // Set initial owner
            viewModel.PurchaseRequisition.StateId = 1; // Assuming 1 is the ID for "Saved"
            await LoadDropdowns(viewModel); // Load dropdown data
            return View(viewModel);
        }

        // POST: Inventory/PurchaseRequisition/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseRequisitionCreateEditViewModel model)
        {
            try
            {
                // Custom validation for PRNo uniqueness (if PRNo is auto-generated, this check might be different or not needed for new records)
                // If PRNo is manually entered and needs to be unique on create, uncomment/adjust this:
                // if (await _purchaseRequisitionService.PurchaseRequisitionExistsAsync(model.PurchaseRequisition.PRNo))
                // {

                //     ModelState.AddModelError("PurchaseRequisition.PRNo", "A Purchase Requisition with this PR No. already exists.");
                // }

                if (ModelState.IsValid)
                {
                    // Set audit properties
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    // Note: Your ViewModel has CreatedBy/UpdatedBy as string, assume User.Identity.Name for that
                    // If you store UserId (GUID/int) in database and need to map user's full name,
                    // you'll need a separate lookup for that or adjust your model.
                    model.PurchaseRequisition.CreatedBy = User.Identity?.Name ?? "System";
                    model.PurchaseRequisition.UpdatedBy = User.Identity?.Name ?? "System";
                    model.PurchaseRequisition.CreatedOn = DateTime.UtcNow;
                    model.PurchaseRequisition.UpdatedOn = DateTime.UtcNow;

                    var success = await _purchaseRequisitionService.SavePurchaseRequisitionAsync(model.PurchaseRequisition);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Purchase Requisition created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create Purchase Requisition. Please try again.");
                    }
                }

                // If ModelState is not valid, or service save failed, reload dropdowns and return view with errors
                await LoadDropdowns(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Purchase Requisition.");
                TempData["ErrorMessage"] = "Error creating Purchase Requisition. Please try again.";
                // In case of a critical error, redirect to Index with error message
                return RedirectToAction(nameof(Index));
            }
        }

        // 3. EDIT
        // GET: Inventory/PurchaseRequisition/Edit/5
        public async Task<IActionResult> Edit(int id) // PRNo is int
        {
            var purchaseRequisition = await _purchaseRequisitionService.GetPurchaseRequisitionByIdAsync(id);
            if (purchaseRequisition == null)
            {
                return NotFound();
            }

            var viewModel = new PurchaseRequisitionCreateEditViewModel { PurchaseRequisition = purchaseRequisition };
            await LoadDropdowns(viewModel);

            if (viewModel.PurchaseRequisition.PurchaseNatureId > 0)
            {
                viewModel.ProductNatures = (await _purchaseRequisitionService.GetProductNaturesByPurchaseNatureIdAsync(viewModel.PurchaseRequisition.PurchaseNatureId))
                    .Select(pn => new SelectListItem { Text = pn.NatureName, Value = pn.NatureId.ToString() }).ToList();
            }

            return View(viewModel);
        }

        // POST: Inventory/PurchaseRequisition/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PurchaseRequisitionCreateEditViewModel viewModel)
        {
            if (id != viewModel.PurchaseRequisition.PRNo)
            {
                return NotFound();
            }

            // Custom validation for PRNo uniqueness during edit (excluding the current record)
            if (await _purchaseRequisitionService.PurchaseRequisitionExistsAsync(viewModel.PurchaseRequisition.PRNo, id))
            {
                ModelState.AddModelError("PurchaseRequisition.PRNo", "A Purchase Requisition with this PR No. already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set audit properties for the update
                    viewModel.PurchaseRequisition.UpdatedBy = User.Identity?.Name ?? "System";
                    viewModel.PurchaseRequisition.UpdatedOn = DateTime.UtcNow;

                    var success = await _purchaseRequisitionService.SavePurchaseRequisitionAsync(viewModel.PurchaseRequisition);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Purchase Requisition updated successfully.";
                        // Assuming this might be an AJAX call, returning Json for success.
                        // If it's a full page post, you might RedirectToAction(nameof(Index));
                        return Json(new { success = true, message = "Purchase Requisition updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "A database error occurred and the Purchase Requisition could not be saved.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating Purchase Requisition with PR No. {id}", id);
                    ModelState.AddModelError("", "An unexpected error occurred while updating the Purchase Requisition.");
                }
            }

            // If we reach here, validation failed or an error occurred. Reload dropdowns and return the view with errors.
            await LoadDropdowns(viewModel);
            if (viewModel.PurchaseRequisition.PurchaseNatureId > 0)
            {
                viewModel.ProductNatures = (await _purchaseRequisitionService.GetProductNaturesByPurchaseNatureIdAsync(viewModel.PurchaseRequisition.PurchaseNatureId))
                    .Select(pn => new SelectListItem { Text = pn.NatureName, Value = pn.NatureId.ToString() }).ToList();
            }
            return View(viewModel); // Assuming it's a full page post for now
        }

        // 4. DETAILS
        // GET: Inventory/PurchaseRequisition/Details/5
        public async Task<IActionResult> Details(int id) // PRNo is int
        {
            try
            {
                var purchaseRequisition = await _purchaseRequisitionService.GetPurchaseRequisitionByIdAsync(id);
                if (purchaseRequisition == null) return NotFound();
                return View(purchaseRequisition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Purchase Requisition details for PR No. {PRNo}", id);
                TempData["ErrorMessage"] = "Error loading Purchase Requisition details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 5. DELETE
        // GET: Inventory/PurchaseRequisition/Delete/5
        public async Task<IActionResult> Delete(int id) // PRNo is int
        {
            try
            {
                var purchaseRequisition = await _purchaseRequisitionService.GetPurchaseRequisitionByIdAsync(id);
                if (purchaseRequisition == null) return NotFound();
                return View(purchaseRequisition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete Purchase Requisition page for PR No. {PRNo}", id);
                TempData["ErrorMessage"] = "Error loading delete page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Inventory/PurchaseRequisition/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) // PRNo is int
        {
            try
            {
                var success = await _purchaseRequisitionService.DeletePurchaseRequisitionAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Purchase Requisition deleted successfully.";
                    // Assuming this might be an AJAX call, returning Json for success.
                    return Json(new { success = true, message = "Purchase Requisition deleted successfully." });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete Purchase Requisition.";
                    return Json(new { success = false, message = "Failed to delete Purchase Requisition. It may have already been removed." });
                }

                // If not an AJAX call, you would typically redirect to Index here:
                // return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Purchase Requisition with PR No. {PRNo}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the Purchase Requisition.";
                return RedirectToAction(nameof(Index)); // Redirect on error
            }
        }

        // --- API & AJAX Endpoints ---


        [HttpPost]
        public async Task<IActionResult> CheckPurchaseRequisitionExists(int prNo, int? excludeId = null)
        {
            try
            {
                var exists = await _purchaseRequisitionService.PurchaseRequisitionExistsAsync(prNo, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Purchase Requisition existence.");
                return Json(new { error = "Error checking Purchase Requisition" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetProductNaturesByPurchaseNature(short purchaseNatureId)
        {
            try
            {
                var productNatures = await _purchaseRequisitionService.GetProductNaturesByPurchaseNatureIdAsync(purchaseNatureId);
                var selectListItems = productNatures.Select(pn => new SelectListItem
                {
                    Text = pn.NatureName,
                    Value = pn.NatureId.ToString()
                }).ToList();

                return Json(selectListItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product natures for purchase nature ID: {purchaseNatureId}", purchaseNatureId);
                return BadRequest("Error loading product natures.");
            }
        }
    }
}