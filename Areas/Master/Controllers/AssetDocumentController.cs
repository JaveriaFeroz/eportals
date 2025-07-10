using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels;
using System.Security.Claims;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Route("Master/AssetDocument")]
    public class AssetDocumentController : Controller
    {
        private readonly IAssetDocumentService _assetDocumentService;
        private readonly ILogger<AssetDocumentController> _logger;

        public AssetDocumentController(
            IAssetDocumentService assetDocumentService,
            ILogger<AssetDocumentController> logger)
        {
            _assetDocumentService = assetDocumentService;
            _logger = logger;
        }

        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(
            bool showInactiveOnly = false,
            string searchTerm = null,
            short? assetFilter = null,
            short? documentTypeFilter = null,
            bool showExpiringOnly = false)
        {
            try
            {
                var documents = await _assetDocumentService.GetDocumentsAsync(
                    activeOnly: !showInactiveOnly,
                    searchTerm: searchTerm,
                    assetFilter: assetFilter,
                    documentTypeFilter: documentTypeFilter,
                    showExpiringOnly: showExpiringOnly);

                var viewModel = new AssetDocumentIndexViewModel
                {
                    Documents = documents.ToList(),
                    ShowInactiveOnly = showInactiveOnly,
                    SearchTerm = searchTerm,
                    AssetFilter = assetFilter,
                    DocumentTypeFilter = documentTypeFilter,
                    ShowExpiringOnly = showExpiringOnly
                };

                // Load filter dropdowns
                await LoadFilterDropdowns(assetFilter, documentTypeFilter);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading asset documents index");
                TempData["ErrorMessage"] = "An error occurred while loading the documents.";
                return View(new AssetDocumentIndexViewModel());
            }
        }

        [HttpGet]
        [Route("Details/{assetId:int}")]
        public async Task<IActionResult> Details(short assetId)
        {
            try
            {
                var viewModel = await _assetDocumentService.GetAssetDocumentsAsync(assetId);

                if (viewModel == null)
                {
                    TempData["ErrorMessage"] = "Asset not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading asset documents for asset {AssetId}", assetId);
                TempData["ErrorMessage"] = "An error occurred while loading the asset documents.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create(short? assetId = null)
        {
            try
            {
                var viewModel = new AssetDocumentViewModel();

                if (assetId.HasValue)
                {
                    viewModel.AssetId = assetId.Value;
                }

                await LoadDropdowns();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create document form");
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssetDocumentViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validate file if uploaded
                    if (model.UploadedFile != null)
                    {
                        if (!IsValidFile(model.UploadedFile))
                        {
                            ModelState.AddModelError("UploadedFile", "Invalid file type or size.");
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        var userId = GetCurrentUserId();
                        var result = await _assetDocumentService.SaveDocumentAsync(model, userId);

                        if (result)
                        {
                            TempData["SuccessMessage"] = "Document created successfully.";
                            return RedirectToAction(nameof(Details), new { assetId = model.AssetId });
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to create document. Please try again.");
                        }
                    }
                }

                await LoadDropdowns();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating asset document");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                await LoadDropdowns();
                return View(model);
            }
        }

        [HttpGet]
        [Route("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var viewModel = await _assetDocumentService.GetDocumentByIdAsync(id);

                if (viewModel == null)
                {
                    TempData["ErrorMessage"] = "Document not found.";
                    return RedirectToAction(nameof(Index));
                }

                await LoadDropdowns();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading document for edit. DocumentId: {DocumentId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the document.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssetDocumentViewModel model)
        {
            try
            {
                if (id != model.DocumentId)
                {
                    TempData["ErrorMessage"] = "Invalid document ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    // Validate file if uploaded
                    if (model.UploadedFile != null)
                    {
                        if (!IsValidFile(model.UploadedFile))
                        {
                            ModelState.AddModelError("UploadedFile", "Invalid file type or size.");
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        var userId = GetCurrentUserId();
                        var result = await _assetDocumentService.SaveDocumentAsync(model, userId);

                        if (result)
                        {
                            TempData["SuccessMessage"] = "Document updated successfully.";
                            return RedirectToAction(nameof(Details), new { assetId = model.AssetId });
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to update document. Please try again.");
                        }
                    }
                }

                await LoadDropdowns();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating asset document. DocumentId: {DocumentId}", id);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                await LoadDropdowns();
                return View(model);
            }
        }

        [HttpGet]
        [Route("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var viewModel = await _assetDocumentService.GetDocumentByIdAsync(id);

                if (viewModel == null)
                {
                    TempData["ErrorMessage"] = "Document not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading document for deletion. DocumentId: {DocumentId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the document.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Route("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Get document details before deletion for redirect
                var document = await _assetDocumentService.GetDocumentByIdAsync(id);
                if (document == null)
                {
                    TempData["ErrorMessage"] = "Document not found.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _assetDocumentService.DeleteDocumentAsync(id);

                if (result)
                {
                    TempData["SuccessMessage"] = "Document deleted successfully.";
                    return RedirectToAction(nameof(Details), new { assetId = document.AssetId });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete document. Please try again.";
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting asset document. DocumentId: {DocumentId}", id);
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the document.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        [HttpGet]
        [Route("Download/{id:int}")]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var (fileData, fileName, contentType) = await _assetDocumentService.DownloadDocumentAsync(id);

                if (fileData == null)
                {
                    TempData["ErrorMessage"] = "File not found or has been removed.";
                    return RedirectToAction(nameof(Index));
                }

                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document. DocumentId: {DocumentId}", id);
                TempData["ErrorMessage"] = "An error occurred while downloading the file.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [Route("Expiring")]
        public async Task<IActionResult> ExpiringDocuments(int days = 30)
        {
            try
            {
                var documents = await _assetDocumentService.GetExpiringDocumentsAsync(days);

                ViewBag.Days = days;
                ViewBag.Title = $"Documents Expiring Within {days} Days";

                return View("ExpiringDocuments", documents.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading expiring documents");
                TempData["ErrorMessage"] = "An error occurred while loading expiring documents.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [Route("GetDocumentTypes")]
        public async Task<IActionResult> GetDocumentTypes(short? companyId = null)
        {
            try
            {
                var lookups = await _assetDocumentService.GetLookupsAsync(companyId);

                var documentTypes = lookups.DocumentTypes.Select(x => new
                {
                    value = x.DocumentTypeId,
                    text = x.TypeName,
                    hasExpiry = x.HasExpiry
                }).ToList();

                return Json(documentTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading document types");
                return Json(new { error = "Failed to load document types" });
            }
        }

        #region Private Methods

        private async Task LoadDropdowns()
        {
            try
            {
                var lookups = await _assetDocumentService.GetLookupsAsync();

                ViewBag.AssetList = new SelectList(lookups.Assets, "AssetId", "DisplayText");
                ViewBag.DocumentTypeList = new SelectList(lookups.DocumentTypes, "DocumentTypeId", "TypeName");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dropdown data");
                ViewBag.AssetList = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.DocumentTypeList = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }

        private async Task LoadFilterDropdowns(short? selectedAsset = null, short? selectedDocumentType = null)
        {
            try
            {
                var lookups = await _assetDocumentService.GetLookupsAsync();

                ViewBag.AssetFilterList = new SelectList(lookups.Assets, "AssetId", "DisplayText", selectedAsset);
                ViewBag.DocumentTypeFilterList = new SelectList(lookups.DocumentTypes, "DocumentTypeId", "TypeName", selectedDocumentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading filter dropdown data");
                ViewBag.AssetFilterList = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.DocumentTypeFilterList = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }

        private bool IsValidFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Check file size (max 10MB)
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (file.Length > maxFileSize)
                return false;

            // Check allowed file extensions
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif", ".txt" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return allowedExtensions.Contains(fileExtension);
        }

        private string GetCurrentUserId()
        {
            return User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
        }

        #endregion
    }
}