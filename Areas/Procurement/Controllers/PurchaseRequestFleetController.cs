using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureToPay.Areas.Procurement.Enums;
using ProcureToPay.Areas.Procurement.Services;
using ProcureToPay.Areas.Procurement.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ProcureToPay.Areas.Procurement.Controllers
{
    [Area("Procurement")]
    [Authorize]
    public class PurchaseRequestFleetController : Controller
    {
        private readonly IPurchaseRequestFleetService _purchaseRequestFleetService;
        private readonly ILogger<PurchaseRequestFleetController> _logger;

        public PurchaseRequestFleetController(
            IPurchaseRequestFleetService purchaseRequestFleetService,
            ILogger<PurchaseRequestFleetController> logger)
        {
            _purchaseRequestFleetService = purchaseRequestFleetService;
            _logger = logger;
        }

        #region Index and List

        [HttpGet]
        public async Task<IActionResult> Index(PurchaseRequestFleetFilterViewModel filter)
        {
            try
            {
                // Set default values if not provided
                filter.Page = filter.Page <= 0 ? 1 : filter.Page;
                filter.PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
                filter.SortField = string.IsNullOrEmpty(filter.SortField) ? "CreatedOn" : filter.SortField;
                filter.SortDirection = string.IsNullOrEmpty(filter.SortDirection) ? "desc" : filter.SortDirection;

                var purchaseRequests = await _purchaseRequestFleetService.GetAllAsync(filter);

                ViewBag.Filter = filter;
                ViewBag.TotalCount = purchaseRequests.Count(); // You might want to modify service to return total count

                return View(purchaseRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading purchase request fleet index");
                TempData["Error"] = "An error occurred while loading purchase requests.";
                return View(new List<PurchaseRequestFleetListViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Summary()
        {
            try
            {
                var summary = await _purchaseRequestFleetService.GetSummaryAsync();
                return View(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading purchase request fleet summary");
                TempData["Error"] = "An error occurred while loading the summary.";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var purchaseRequest = await _purchaseRequestFleetService.GetByIdAsync(id);
                if (purchaseRequest == null)
                {
                    TempData["Error"] = "Purchase request not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get detail summary
                var detailSummary = await _purchaseRequestFleetService.GetDetailSummaryAsync(id);
                ViewBag.DetailSummary = detailSummary;

                return View(purchaseRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading purchase request fleet details for ID {Id}", id);
                TempData["Error"] = "An error occurred while loading the purchase request details.";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = await _purchaseRequestFleetService.GetCreateViewModelAsync();
                await PopulateDropdownsForCreate(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create purchase request fleet form");
                TempData["Error"] = "An error occurred while loading the create form.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePurchaseRequestFleetViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await PopulateDropdownsForCreate(model);
                    return View(model);
                }

                var userId = GetCurrentUserId();
                var id = await _purchaseRequestFleetService.CreateAsync(model, userId);

                TempData["Success"] = "Purchase request created successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase request fleet");
                ModelState.AddModelError("", "An error occurred while creating the purchase request. Please try again.");
                await PopulateDropdownsForCreate(model);
                return View(model);
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await _purchaseRequestFleetService.GetEditViewModelAsync(id);
                if (model == null)
                {
                    TempData["Error"] = "Purchase request not found.";
                    return RedirectToAction(nameof(Index));
                }

                await PopulateDropdownsForEdit(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit purchase request fleet form for ID {Id}", id);
                TempData["Error"] = "An error occurred while loading the edit form.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditPurchaseRequestFleetViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    TempData["Error"] = "Invalid request.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    await PopulateDropdownsForEdit(model);
                    return View(model);
                }

                var userId = GetCurrentUserId();
                var success = await _purchaseRequestFleetService.UpdateAsync(id, model, userId);

                if (success)
                {
                    TempData["Success"] = "Purchase request updated successfully.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["Error"] = "Purchase request not found or could not be updated.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while updating purchase request fleet {Id}", id);
                ModelState.AddModelError("", ex.Message);
                await PopulateDropdownsForEdit(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purchase request fleet {Id}", id);
                ModelState.AddModelError("", "An error occurred while updating the purchase request. Please try again.");
                await PopulateDropdownsForEdit(model);
                return View(model);
            }
        }

        #endregion

        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _purchaseRequestFleetService.DeleteAsync(id, userId);

                if (success)
                {
                    TempData["Success"] = "Purchase request deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Purchase request not found or could not be deleted.";
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while deleting purchase request fleet {Id}", id);
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase request fleet {Id}", id);
                TempData["Error"] = "An error occurred while deleting the purchase request.";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Workflow Actions

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _purchaseRequestFleetService.SubmitAsync(id, userId);

                if (success)
                {
                    TempData["Success"] = "Purchase request submitted successfully and is now pending approval.";
                }
                else
                {
                    TempData["Error"] = "Purchase request not found or could not be submitted.";
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while submitting purchase request fleet {Id}", id);
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting purchase request fleet {Id}", id);
                TempData["Error"] = "An error occurred while submitting the purchase request.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var purchaseRequest = await _purchaseRequestFleetService.GetByIdAsync(id);
                if (purchaseRequest == null)
                {
                    TempData["Error"] = "Purchase request not found.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new ApprovalViewModel
                {
                    Id = id,
                    PRNo = purchaseRequest.PRNo,
                    Owner = purchaseRequest.Owner,
                    BudgetAmount = purchaseRequest.BudgetAmount,
                    Justification = purchaseRequest.Justification
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading approve form for purchase request fleet {Id}", id);
                TempData["Error"] = "An error occurred while loading the approval form.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(ApprovalViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userId = GetCurrentUserId();
                var success = await _purchaseRequestFleetService.ApproveAsync(model.Id, userId, model.Remarks ?? "");

                if (success)
                {
                    TempData["Success"] = "Purchase request approved successfully.";
                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }
                else
                {
                    TempData["Error"] = "Purchase request not found or could not be approved.";
                    return View(model);
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while approving purchase request fleet {Id}", model.Id);
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving purchase request fleet {Id}", model.Id);
                ModelState.AddModelError("", "An error occurred while approving the purchase request.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                var purchaseRequest = await _purchaseRequestFleetService.GetByIdAsync(id);
                if (purchaseRequest == null)
                {
                    TempData["Error"] = "Purchase request not found.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new RejectionViewModel
                {
                    Id = id,
                    PRNo = purchaseRequest.PRNo,
                    Owner = purchaseRequest.Owner,
                    BudgetAmount = purchaseRequest.BudgetAmount,
                    Justification = purchaseRequest.Justification
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reject form for purchase request fleet {Id}", id);
                TempData["Error"] = "An error occurred while loading the rejection form.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(RejectionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.Remarks))
                {
                    ModelState.AddModelError(nameof(model.Remarks), "Rejection remarks are required.");
                    return View(model);
                }

                var userId = GetCurrentUserId();
                var success = await _purchaseRequestFleetService.RejectAsync(model.Id, userId, model.Remarks);

                if (success)
                {
                    TempData["Success"] = "Purchase request rejected successfully.";
                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }
                else
                {
                    TempData["Error"] = "Purchase request not found or could not be rejected.";
                    return View(model);
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while rejecting purchase request fleet {Id}", model.Id);
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting purchase request fleet {Id}", model.Id);
                ModelState.AddModelError("", "An error occurred while rejecting the purchase request.");
                return View(model);
            }
        }

        #endregion

        #region AJAX/Partial Actions

        [HttpGet]
        public async Task<IActionResult> GetDetailSummary(int id)
        {
            try
            {
                var summary = await _purchaseRequestFleetService.GetDetailSummaryAsync(id);
                return PartialView("_DetailSummary", summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading detail summary for purchase request fleet {Id}", id);
                return BadRequest("Error loading detail summary");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateBusinessRules(int id)
        {
            try
            {
                var purchaseRequest = await _purchaseRequestFleetService.GetByIdAsync(id);
                if (purchaseRequest == null)
                {
                    return NotFound();
                }

                // Convert to entity for validation - you might need to adjust this based on your mapping
                var entity = new ProcureToPay.Areas.Procurement.Models.PurchaseRequestsFleet
                {
                    Id = purchaseRequest.Id,
                    BudgetAmount = purchaseRequest.BudgetAmount
                    // Map other required properties
                };

                var isValid = await _purchaseRequestFleetService.ValidateBusinessRulesAsync(entity);
                return Json(new { isValid, message = isValid ? "Validation passed" : "Validation failed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating business rules for purchase request fleet {Id}", id);
                return Json(new { isValid = false, message = "Error during validation" });
            }
        }

        #endregion

        #region Helper Methods

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            // Alternative: try custom claim type if using different claim structure
            var customUserIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(customUserIdClaim, out int customUserId))
            {
                return customUserId;
            }

            throw new InvalidOperationException("Unable to retrieve current user ID");
        }

        private async Task PopulateDropdownsForCreate(CreatePurchaseRequestFleetViewModel model)
        {
            // TODO: Populate dropdown lists from your services
            // Example:
            // ViewBag.Companies = await _companyService.GetAllAsync();
            // ViewBag.Branches = await _branchService.GetAllAsync();
            // ViewBag.Departments = await _departmentService.GetAllAsync();
            // ViewBag.ProductNatures = await _productNatureService.GetAllAsync();
            // ViewBag.ServiceNatures = await _serviceNatureService.GetAllAsync();

            // For now, using empty lists to prevent null reference errors
            ViewBag.Companies = new List<object>();
            ViewBag.Branches = new List<object>();
            ViewBag.Departments = new List<object>();
            ViewBag.ProductNatures = new List<object>();
            ViewBag.ServiceNatures = new List<object>();
        }

        private async Task PopulateDropdownsForEdit(EditPurchaseRequestFleetViewModel model)
        {
            // TODO: Populate dropdown lists from your services
            // Similar to PopulateDropdownsForCreate but for edit view

            // For now, using empty lists to prevent null reference errors
            ViewBag.Companies = new List<object>();
            ViewBag.Branches = new List<object>();
            ViewBag.Departments = new List<object>();
            ViewBag.ProductNatures = new List<object>();
            ViewBag.ServiceNatures = new List<object>();
        }

        #endregion
    }

    #region View Models for Approval/Rejection

    public class ApprovalViewModel
    {
        public int Id { get; set; }
        public int? PRNo { get; set; }
        public string Owner { get; set; } = string.Empty;
        public decimal? BudgetAmount { get; set; }
        public string Justification { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class RejectionViewModel
    {
        public int Id { get; set; }
        public int? PRNo { get; set; }
        public string Owner { get; set; } = string.Empty;
        public decimal? BudgetAmount { get; set; }
        public string Justification { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rejection remarks are required")]
        [StringLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters")]
        public string Remarks { get; set; } = string.Empty;
    }

    #endregion
}