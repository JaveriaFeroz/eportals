using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductListViewModel>> GetProductsAsync(bool activeOnly = true, string searchTerm = null);
        Task<ProductViewModel> GetProductByIdAsync(short id);
        Task<bool> SaveProductAsync(ProductViewModel model);
        Task<bool> DeleteProductAsync(short id);
        Task<bool> ProductExistsAsync(string productName, short? excludeId = null);
        Task<ProductPriceUpdateResult> UpdateProductPricesFromBidAsync(int selectedBidId, int userId);
        Task<ProductLookupsViewModel> GetLookupsAsync();
    }

    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductListViewModel>> GetProductsAsync(bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.Products
                    .Include(x => x.UoM)
                    .Include(x => x.ProductNature)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.ProductName.Contains(searchTerm));
                }

                var result = await query
                    .OrderBy(x => x.ProductName)
                    .Select(x => new ProductListViewModel
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                        UnitPrice = x.UnitPrice,
                        UoMName = x.UoM != null ? x.UoM.UoMName : "",
                        ProductNatureName = x.ProductNature != null ? x.ProductNature.NatureName : "",
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
                _logger.LogError(ex, "Error retrieving products");
                throw;
            }
        }

        public async Task<ProductViewModel> GetProductByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Products
                    .Include(x => x.UoM)
                    .Include(x => x.ProductNature)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.ProductId == id);

                if (entity == null) return null;

                var viewModel = new ProductViewModel
                {
                    ProductId = entity.ProductId,
                    ProductName = entity.ProductName,
                    UnitPrice = entity.UnitPrice,
                    UoMId = entity.UoMId,
                    ProductNatureId = entity.ProductNatureId,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };

                // Load dropdowns
                await LoadDropdownsAsync(viewModel);

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> SaveProductAsync(ProductViewModel model)
        {
            try
            {
                Product entity;

                if (model.ProductId.HasValue && model.ProductId > 0)
                {
                    // Update existing
                    entity = await _context.Products.FindAsync(model.ProductId.Value);
                    if (entity == null) return false;

                    entity.ProductName = model.ProductName;
                    entity.UnitPrice = model.UnitPrice;
                    entity.UoMId = model.UoMId;
                    entity.ProductNatureId = model.ProductNatureId;
                    entity.IsActive = model.IsActive;
                    // UpdatedBy/UpdatedOn handled automatically by DbContext
                }
                else
                {
                    // Create new
                    entity = new Product
                    {
                        ProductName = model.ProductName,
                        UnitPrice = model.UnitPrice,
                        UoMId = model.UoMId,
                        ProductNatureId = model.ProductNatureId,
                        IsActive = model.IsActive
                        // CreatedBy/CreatedOn handled automatically by DbContext
                    };
                    _context.Products.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving product");
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(short id)
        {
            try
            {
                var entity = await _context.Products.FindAsync(id);
                if (entity == null) return false;

                _context.Products.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> ProductExistsAsync(string productName, short? excludeId = null)
        {
            try
            {
                var query = _context.Products.Where(x => x.ProductName == productName);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.ProductId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product exists");
                throw;
            }
        }

        public async Task<ProductPriceUpdateResult> UpdateProductPricesFromBidAsync(int selectedBidId, int userId)
        {
            var result = new ProductPriceUpdateResult { Success = false };

            try
            {
                // Get the selected bid with all related data
                var selectedBid = await _context.Bids
                    .Include(b => b.BidItems)
                        .ThenInclude(bi => bi.PurchaseRequestItem)
                            .ThenInclude(pri => pri.Product)
                    .FirstOrDefaultAsync(b => b.BidId == selectedBidId);

                if (selectedBid == null)
                {
                    result.Message = "Selected bid not found.";
                    return result;
                }

                var changes = new List<ProductPriceChange>();

                foreach (var bidItem in selectedBid.BidItems)
                {

                    if (bidItem.PurchaseRequestItem?.ProductId != null &&
                        bidItem.PurchaseRequestItem.ProductId > 0)
                    {
                        var productId = bidItem.PurchaseRequestItem.ProductId.Value;
                        var product = await _context.Products
                            .FirstOrDefaultAsync(p => p.ProductId == productId);

                        if (product != null)
                        {

                            var oldPrice = (decimal)product.UnitPrice;
                            var newPrice = bidItem.UnitPrice;


                            if (Math.Abs(oldPrice - newPrice) > 0.01m)
                            {

                                product.UnitPrice = (double)newPrice;
                                product.UpdatedByUserId = userId;
                                product.UpdatedOn = DateTime.Now;

                                changes.Add(new ProductPriceChange
                                {
                                    ProductId = product.ProductId,
                                    ProductName = product.ProductName,
                                    OldPrice = oldPrice,
                                    NewPrice = newPrice
                                });

                                _logger.LogInformation(
                                    $"Updated product price: ProductId={productId}, " +
                                    $"Name={product.ProductName}, " +
                                    $"OldPrice={oldPrice}, " +
                                    $"NewPrice={newPrice}");
                            }
                        }
                    }
                }

                if (changes.Any())
                {
                    await _context.SaveChangesAsync();
                    result.Success = true;
                    result.Changes = changes;
                    result.Message = $"Successfully updated {changes.Count} product price(s).";
                }
                else
                {
                    result.Success = true;
                    result.Message = "No product prices needed updating (no products linked to PR items or prices unchanged).";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product prices from bid {BidId}", selectedBidId);
                result.Message = $"Error updating prices: {ex.Message}";
                return result;
            }
        }
        public async Task<ProductLookupsViewModel> GetLookupsAsync()
        {
            try
            {
                var lookups = new ProductLookupsViewModel
                {
                    UoMs = await _context.UoMs
                        .Where(x => x.IsActive)
                        .OrderBy(x => x.UoMName)
                        .Select(x => new UoMListViewModel
                        {
                            UoMId = x.UoMId,
                            UoMName = x.UoMName,
                            IsActive = x.IsActive
                        })
                        .ToListAsync(),

                    ProductNatures = await _context.ProductNatures
                        .Where(x => x.IsActive)
                        .OrderBy(x => x.NatureName)
                        .Select(x => new ProductNatureListViewModel
                        {
                            NatureId = x.NatureId,
                            NatureName = x.NatureName,
                            IsActive = x.IsActive
                        })
                        .ToListAsync()
                };

                return lookups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product lookups");
                throw;
            }
        }

        private async Task LoadDropdownsAsync(ProductViewModel model)
        {
            var lookups = await GetLookupsAsync();

            model.UoMs = new SelectList(lookups.UoMs, "UoMId", "UoMName", model.UoMId);
            model.ProductNatures = new SelectList(lookups.ProductNatures, "NatureId", "NatureName", model.ProductNatureId);
        }
    }
}