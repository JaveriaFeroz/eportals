using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.Common.Services;
using ProcureToPay.Areas.Master.ViewModels;
using System.Security.Claims;

namespace ProcureToPay.Areas.Common.Controllers
{
    [Area("Common")]
    [Authorize]
    public class WorkOrderTypeController : Controller
    {
        private readonly IWorkOrderTypeService _workOrderTypeService;
        private readonly ILogger<WorkOrderTypeController> _logger;

        public WorkOrderTypeController(
            IWorkOrderTypeService workOrderTypeService,
            ILogger<WorkOrderTypeController> logger)
        {
            _workOrderTypeService = workOrderTypeService;
            _logger = logger;
        }

        // GET: Common/WorkOrderType
        public async Task<IActionResult> Index(short? workFlowId = null, bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var workOrderTypes = await _workOrderTypeService.GetWorkOrderTypesAsync(workFlowId, !showInactive, searchTerm);

                var viewModel = new WorkOrderTypeIndexViewModel
                {
                    WorkOrderTypes = workOrderTypes.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm,
                    WorkFlowId = workFlowId
                    // You can populate WorkFlowName if you have a WorkFlow service
                    // WorkFlowName = workFlowId.HasValue ? await _workFlowService.GetWorkFlowNameAsync(workFlowId.Value) : null
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading work order types for WorkFlowId: {WorkFlowId}", workFlowId);
                TempData["ErrorMessage"] = "An error occurred while loading the work order types.";
                return View(new WorkOrderTypeIndexViewModel());
            }
        }

        // GET: Common/WorkOrderType/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var workOrderType = await _workOrderTypeService.GetWorkOrderTypeByIdAsync(id);
                if (workOrderType == null)
                {
                    return NotFound();
                }

                return View(workOrderType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading work order type details for ID {WorkOrderTypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the work order type details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Common/WorkOrderType/Create
        public IActionResult Create(short? workFlowId = null)
        {
            var model = new WorkOrderTypeViewModel();
            if (workFlowId.HasValue)
            {
                model.WorkFlowId = workFlowId.Value;
            }
            return View(model);
        }

        // POST: Common/WorkOrderType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkOrderTypeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if work order type name already exists for this workflow
                    if (await _workOrderTypeService.WorkOrderTypeExistsAsync(model.WorkOrderTypeName, model.WorkFlowId))
                    {
                        ModelState.AddModelError("WorkOrderTypeName", "A work order type with this name already exists for the selected workflow.");
                        return View(model);
                    }

                    // Check if work order type code already exists globally
                    if (await _workOrderTypeService.WorkOrderTypeCodeExistsAsync(model.WorkOrderTypeCode))
                    {
                        ModelState.AddModelError("WorkOrderTypeCode", "A work order type with this code already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _workOrderTypeService.SaveWorkOrderTypeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Work order type created successfully.";
                        return RedirectToAction(nameof(Index), new { workFlowId = model.WorkFlowId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create work order type.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work order type: {WorkOrderTypeName}", model.WorkOrderTypeName);
                ModelState.AddModelError("", "An error occurred while creating the work order type.");
                return View(model);
            }
        }

        // GET: Common/WorkOrderType/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var workOrderType = await _workOrderTypeService.GetWorkOrderTypeByIdAsync(id);
                if (workOrderType == null)
                {
                    return NotFound();
                }

                return View(workOrderType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading work order type for edit with ID {WorkOrderTypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the work order type.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Common/WorkOrderType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, WorkOrderTypeViewModel model)
        {
            if (id != model.WorkOrderTypeId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Check if work order type name already exists for this workflow (excluding current record)
                    if (await _workOrderTypeService.WorkOrderTypeExistsAsync(model.WorkOrderTypeName, model.WorkFlowId, id))
                    {
                        ModelState.AddModelError("WorkOrderTypeName", "A work order type with this name already exists for the selected workflow.");
                        return View(model);
                    }

                    // Check if work order type code already exists globally (excluding current record)
                    if (await _workOrderTypeService.WorkOrderTypeCodeExistsAsync(model.WorkOrderTypeCode, id))
                    {
                        ModelState.AddModelError("WorkOrderTypeCode", "A work order type with this code already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _workOrderTypeService.SaveWorkOrderTypeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Work order type updated successfully.";
                        return RedirectToAction(nameof(Index), new { workFlowId = model.WorkFlowId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update work order type.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating work order type with ID {WorkOrderTypeId}", id);
                ModelState.AddModelError("", "An error occurred while updating the work order type.");
                return View(model);
            }
        }

        // GET: Common/WorkOrderType/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var workOrderType = await _workOrderTypeService.GetWorkOrderTypeByIdAsync(id);
                if (workOrderType == null)
                {
                    return NotFound();
                }

                return View(workOrderType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading work order type for delete with ID {WorkOrderTypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the work order type.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Common/WorkOrderType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var workOrderType = await _workOrderTypeService.GetWorkOrderTypeByIdAsync(id);
                var success = await _workOrderTypeService.DeleteWorkOrderTypeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Work order type deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete work order type.";
                }

                return RedirectToAction(nameof(Index), new { workFlowId = workOrderType?.WorkFlowId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting work order type with ID {WorkOrderTypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the work order type.";
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX endpoint for checking if work order type name exists
        [HttpGet]
        public async Task<JsonResult> CheckWorkOrderTypeNameExists(string workOrderTypeName, short workFlowId, short? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workOrderTypeName))
                {
                    return Json(new { exists = false });
                }

                var exists = await _workOrderTypeService.WorkOrderTypeExistsAsync(workOrderTypeName.Trim(), workFlowId, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking work order type name existence: {WorkOrderTypeName}", workOrderTypeName);
                return Json(new { exists = false, error = true });
            }
        }

        // AJAX endpoint for checking if work order type code exists
        [HttpGet]
        public async Task<JsonResult> CheckWorkOrderTypeCodeExists(string workOrderTypeCode, short? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workOrderTypeCode))
                {
                    return Json(new { exists = false });
                }

                var exists = await _workOrderTypeService.WorkOrderTypeCodeExistsAsync(workOrderTypeCode.Trim(), excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking work order type code existence: {WorkOrderTypeCode}", workOrderTypeCode);
                return Json(new { exists = false, error = true });
            }
        }

        // AJAX endpoint for getting work order types by workflow (for dropdowns)
        [HttpGet]
        public async Task<JsonResult> GetWorkOrderTypesByWorkflow(short workFlowId, bool activeOnly = true)
        {
            try
            {
                var workOrderTypes = await _workOrderTypeService.GetWorkOrderTypesForDropdownAsync(workFlowId, activeOnly);
                var result = workOrderTypes.Select(x => new {
                    value = x.WorkOrderTypeId,
                    text = x.WorkOrderTypeName,
                    code = x.WorkOrderTypeCode
                });
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work order types for workflow {WorkFlowId}", workFlowId);
                return Json(new { error = true, message = "Error loading work order types" });
            }
        }

        // AJAX endpoint for updating display order
        [HttpPost]
        public async Task<JsonResult> UpdateDisplayOrder(short workOrderTypeId, int displayOrder)
        {
            try
            {
                var success = await _workOrderTypeService.UpdateDisplayOrderAsync(workOrderTypeId, displayOrder);
                return Json(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating display order for work order type {WorkOrderTypeId}", workOrderTypeId);
                return Json(new { success = false, error = true });
            }
        }

        // GET: Common/WorkOrderType/GetActiveWorkOrderTypes
        // For use in other controllers/views that need work order type dropdowns
        [HttpGet]
        public async Task<JsonResult> GetActiveWorkOrderTypes(short? workFlowId = null)
        {
            try
            {
                var workOrderTypes = await _workOrderTypeService.GetWorkOrderTypesForDropdownAsync(workFlowId, true);
                return Json(workOrderTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active work order types");
                return Json(new { error = true, message = "Error loading work order types" });
            }
        }
    }
}