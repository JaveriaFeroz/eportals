// Services/IAssetDocumentService.cs
using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IAssetDocumentService
    {
        Task<IEnumerable<AssetDocumentListViewModel>> GetDocumentsAsync(
            bool activeOnly = true,
            string searchTerm = null,
            short? assetFilter = null,
            short? documentTypeFilter = null,
            bool showExpiringOnly = false);

        Task<AssetDocumentDetailViewModel> GetAssetDocumentsAsync(short assetId);
        Task<AssetDocumentViewModel> GetDocumentByIdAsync(int documentId);
        Task<bool> SaveDocumentAsync(AssetDocumentViewModel model, string userId);
        Task<bool> DeleteDocumentAsync(int documentId);
        Task<AssetDocumentDetailViewModel.LookupData> GetLookupsAsync(short? companyId = null);
        Task<(byte[] fileData, string fileName, string contentType)> DownloadDocumentAsync(int documentId);
        Task<IEnumerable<AssetDocumentListViewModel>> GetExpiringDocumentsAsync(int days = 30);
    }
    public class AssetDocumentService : IAssetDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AssetDocumentService> _logger;
        private readonly IWebHostEnvironment _environment;

        public AssetDocumentService(
            ApplicationDbContext context,
            ILogger<AssetDocumentService> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        public async Task<IEnumerable<AssetDocumentListViewModel>> GetDocumentsAsync(
            bool activeOnly = true,
            string searchTerm = null,
            short? assetFilter = null,
            short? documentTypeFilter = null,
            bool showExpiringOnly = false)
        {
            try
            {
                var query = _context.AssetDocuments
                    .Include(x => x.Asset)
                    .Include(x => x.DocumentType)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.DocumentTitle.Contains(searchTerm) ||
                                           x.DocumentNumber.Contains(searchTerm) ||
                                           x.Asset.AssetNo.Contains(searchTerm));
                }

                if (assetFilter.HasValue)
                {
                    query = query.Where(x => x.AssetId == assetFilter.Value);
                }

                if (documentTypeFilter.HasValue)
                {
                    query = query.Where(x => x.DocumentTypeId == documentTypeFilter.Value);
                }

                if (showExpiringOnly)
                {
                    var futureDate = DateTime.Now.AddDays(30);
                    query = query.Where(x => x.ExpiryDate.HasValue &&
                                           x.ExpiryDate.Value <= futureDate &&
                                           x.ExpiryDate.Value >= DateTime.Now);
                }

                var result = await query
                    .OrderBy(x => x.Asset.AssetNo)
                    .ThenBy(x => x.DocumentType.TypeName)
                    .Select(x => new AssetDocumentListViewModel
                    {
                        DocumentId = x.DocumentId,
                        AssetId = x.AssetId,
                        AssetName = x.Asset.AssetNo,
                        DocumentTypeName = x.DocumentType.TypeName,
                        DocumentTitle = x.DocumentTitle,
                        DocumentNumber = x.DocumentNumber,
                        IssueDate = x.IssueDate,
                        ExpiryDate = x.ExpiryDate,
                        FileName = x.FileName,
                        IsActive = x.IsActive,
                        CreatedBy = x.CreatedByUser != null ? $"{x.CreatedByUser.FirstName} {x.CreatedByUser.LastName}" : "System",
                        CreatedOn = x.CreatedOn,
                        UpdatedBy = x.UpdatedByUser != null ? $"{x.UpdatedByUser.FirstName} {x.UpdatedByUser.LastName}" : null,
                        UpdatedOn = x.UpdatedOn
                    })
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving asset documents");
                throw;
            }
        }

        public async Task<AssetDocumentDetailViewModel> GetAssetDocumentsAsync(short assetId)
        {
            try
            {
                var asset = await _context.Assets
                    .FirstOrDefaultAsync(x => x.AssetId == assetId);

                if (asset == null) return null;

                var documents = await _context.AssetDocuments
                    .Include(x => x.DocumentType)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .Where(x => x.AssetId == assetId)
                    .OrderBy(x => x.DocumentType.TypeName)
                    .Select(x => new AssetDocumentViewModel
                    {
                        DocumentId = x.DocumentId,
                        AssetId = x.AssetId,
                        DocumentTypeId = x.DocumentTypeId,
                        DocumentTitle = x.DocumentTitle,
                        DocumentNumber = x.DocumentNumber,
                        IssueDate = x.IssueDate,
                        ExpiryDate = x.ExpiryDate,
                        IssuingAuthority = x.IssuingAuthority,
                        CurrentFileName = x.FileName,
                        Notes = x.Notes,
                        IsActive = x.IsActive,
                        DocumentTypeName = x.DocumentType.TypeName,
                        CreatedBy = x.CreatedByUser != null ? $"{x.CreatedByUser.FirstName} {x.CreatedByUser.LastName}" : "System",
                        CreatedOn = x.CreatedOn,
                        UpdatedBy = x.UpdatedByUser != null ? $"{x.UpdatedByUser.FirstName} {x.UpdatedByUser.LastName}" : null,
                        UpdatedOn = x.UpdatedOn
                    })
                    .ToListAsync();

                return new AssetDocumentDetailViewModel
                {
                    AssetId = assetId,
                    AssetName = asset.AssetNo,
                    AssetNumber = asset.AssetNo,
                    Documents = documents.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving asset documents for asset {AssetId}", assetId);
                throw;
            }
        }

        public async Task<AssetDocumentViewModel> GetDocumentByIdAsync(int documentId)
        {
            try
            {
                var entity = await _context.AssetDocuments
                    .Include(x => x.Asset)
                    .Include(x => x.DocumentType)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.DocumentId == documentId);

                if (entity == null) return null;

                return new AssetDocumentViewModel
                {
                    DocumentId = entity.DocumentId,
                    AssetId = entity.AssetId,
                    DocumentTypeId = entity.DocumentTypeId,
                    DocumentTitle = entity.DocumentTitle,
                    DocumentNumber = entity.DocumentNumber,
                    IssueDate = entity.IssueDate,
                    ExpiryDate = entity.ExpiryDate,
                    IssuingAuthority = entity.IssuingAuthority,
                    CurrentFileName = entity.FileName,
                    Notes = entity.Notes,
                    IsActive = entity.IsActive,
                    AssetName = entity.Asset?.AssetNo,
                    DocumentTypeName = entity.DocumentType?.TypeName,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document with ID {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<bool> SaveDocumentAsync(AssetDocumentViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                AssetDocument entity;

                if (model.DocumentId.HasValue && model.DocumentId > 0)
                {
                    // Update existing
                    entity = await _context.AssetDocuments
                        .FirstOrDefaultAsync(x => x.DocumentId == model.DocumentId.Value);

                    if (entity == null) return false;

                    entity.AssetId = model.AssetId;
                    entity.DocumentTypeId = model.DocumentTypeId;
                    entity.DocumentTitle = model.DocumentTitle;
                    entity.DocumentNumber = model.DocumentNumber;
                    entity.IssueDate = model.IssueDate;
                    entity.ExpiryDate = model.ExpiryDate;
                    entity.IssuingAuthority = model.IssuingAuthority;
                    entity.Notes = model.Notes;
                    entity.IsActive = model.IsActive;
                }
                else
                {
                    // Create new
                    entity = new AssetDocument
                    {
                        AssetId = model.AssetId,
                        DocumentTypeId = model.DocumentTypeId,
                        DocumentTitle = model.DocumentTitle,
                        DocumentNumber = model.DocumentNumber,
                        IssueDate = model.IssueDate,
                        ExpiryDate = model.ExpiryDate,
                        IssuingAuthority = model.IssuingAuthority,
                        Notes = model.Notes,
                        IsActive = model.IsActive
                    };
                    _context.AssetDocuments.Add(entity);
                }

                // Handle file upload
                if (model.UploadedFile != null && model.UploadedFile.Length > 0)
                {
                    var fileName = await SaveFileAsync(model.UploadedFile, entity.AssetId);
                    entity.FileName = model.UploadedFile.FileName;
                    entity.FilePath = fileName;
                    entity.FileSize = model.UploadedFile.Length;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving asset document");
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(int documentId)
        {
            try
            {
                var entity = await _context.AssetDocuments
                    .FirstOrDefaultAsync(x => x.DocumentId == documentId);

                if (entity == null) return false;

                // Delete file from disk if exists
                if (!string.IsNullOrEmpty(entity.FilePath))
                {
                    await DeleteFileAsync(entity.FilePath);
                }

                _context.AssetDocuments.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document with ID {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<AssetDocumentDetailViewModel.LookupData> GetLookupsAsync(short? companyId = null)
        {
            try
            {
                var assetsQuery = _context.Assets.Where(x => x.IsActive);
                var documentTypesQuery = _context.DocumentTypes.Where(x => x.IsActive);

                if (companyId.HasValue)
                {
                    assetsQuery = assetsQuery.Where(x => x.CompanyId == companyId.Value);
                    documentTypesQuery = documentTypesQuery.Where(x => x.CompanyId == companyId.Value);
                }

                var assets = await assetsQuery
                    .OrderBy(x => x.AssetNo)
                    .Select(x => new AssetDocumentDetailViewModel.AssetLookupItem
                    {
                        AssetId = x.AssetId,
                        AssetNo = x.AssetNo
                    })
                    .ToListAsync();

                var documentTypes = await documentTypesQuery
                    .OrderBy(x => x.TypeName)
                    .Select(x => new AssetDocumentDetailViewModel.DocumentTypeLookupItem
                    {
                        DocumentTypeId = x.DocumentTypeId,
                        TypeName = x.TypeName,
                        HasExpiry = x.HasExpiry
                    })
                    .ToListAsync();

                return new AssetDocumentDetailViewModel.LookupData
                {
                    Assets = assets,
                    DocumentTypes = documentTypes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lookups");
                throw;
            }
        }

        public async Task<(byte[] fileData, string fileName, string contentType)> DownloadDocumentAsync(int documentId)
        {
            try
            {
                var document = await _context.AssetDocuments
                    .FirstOrDefaultAsync(x => x.DocumentId == documentId);

                if (document == null || string.IsNullOrEmpty(document.FilePath))
                    return (null, null, null);

                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "documents", document.FilePath);

                if (!File.Exists(filePath))
                    return (null, null, null);

                var fileData = await File.ReadAllBytesAsync(filePath);
                var contentType = GetContentType(document.FileName);

                return (fileData, document.FileName, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document with ID {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<IEnumerable<AssetDocumentListViewModel>> GetExpiringDocumentsAsync(int days = 30)
        {
            try
            {
                var futureDate = DateTime.Now.AddDays(days);
                return await GetDocumentsAsync(
                    activeOnly: true,
                    showExpiringOnly: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expiring documents");
                throw;
            }
        }

        private async Task<string> SaveFileAsync(IFormFile file, short assetId)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{assetId}_{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }

        private async Task DeleteFileAsync(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "documents", fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deleting file {FileName}", fileName);
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}