using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Master.Services
{
    public interface IAssetService
    {
        Task<IEnumerable<AssetListViewModel>> GetAssetsAsync(bool activeOnly = true, string searchTerm = null, short? assetTypeFilter = null, short? statusFilter = null);
        Task<AssetViewModel> GetAssetByIdAsync(short id);
        Task<bool> SaveAssetAsync(AssetViewModel model);
        Task<bool> DeleteAssetAsync(short id);
        Task<bool> AssetExistsAsync(string assetNo, short? excludeId = null);
        Task<bool> UpdateAssetKilometersAsync(short assetId, decimal kilometers);
        Task<IEnumerable<AssetTyreViewModel>> GetAssetTyresAsync(short assetId);
        Task<AssetLookupsViewModel> GetLookupsAsync();
        Task<bool> SaveAssetTyresAsync(short assetId, List<AssetTyreViewModel> tyres);
    }

    public class AssetService : IAssetService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AssetService> _logger;

        public AssetService(ApplicationDbContext context, ILogger<AssetService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AssetListViewModel>> GetAssetsAsync(bool activeOnly = true, string searchTerm = null, short? assetTypeFilter = null, short? statusFilter = null)
        {
            try
            {
                var query = _context.Assets
                    .Include(x => x.AssetType)
                    .Include(x => x.AssetStatus)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x => x.AssetNo.Contains(searchTerm) ||
                                           x.Model.Contains(searchTerm) ||
                                           x.FACode.Contains(searchTerm));
                }

                if (assetTypeFilter.HasValue)
                {
                    query = query.Where(x => x.AssetTypeId == assetTypeFilter.Value);
                }

                if (statusFilter.HasValue)
                {
                    query = query.Where(x => x.StatusId == statusFilter.Value);
                }

                var result = await query
                    .OrderBy(x => x.AssetNo)
                    .Select(x => new AssetListViewModel
                    {
                        AssetId = x.AssetId,
                        AssetNo = x.AssetNo,
                        AssetTypeName = x.AssetType != null ? x.AssetType.TypeName : "N/A",
                        StatusName = x.AssetStatus != null ? x.AssetStatus.StatusName : "N/A",
                        KMs = x.KMs,
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
                _logger.LogError(ex, "Error retrieving assets");
                throw;
            }
        }

        public async Task<AssetViewModel> GetAssetByIdAsync(short id)
        {
            try
            {
                var entity = await _context.Assets
                    .Include(x => x.AssetType)
                    .Include(x => x.AssetStatus)
                    .Include(x => x.CapacityName)
                    .Include(x => x.MakeName)
                    .Include(x => x.LeaseTypeName)
                    .Include(x => x.TrailerName)
                    .Include(x => x.SupplierName)
                    .Include(x => x.DriverName1)
                    .Include(x => x.DriverName2)
                    .Include(x => x.CityName)
                    .Include(x => x.ClientName)
                    .Include(x => x.BaseName)
                    .Include(x => x.CompanyName)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.AssetId == id);

                if (entity == null) return null;

                return new AssetViewModel
                {
                    AssetId = entity.AssetId,
                    AssetNo = entity.AssetNo,
                    AssetTypeId = entity.AssetTypeId,
                    AssetTypeName = entity.AssetType?.TypeName,
                    CapacityId = entity.CapacityId,
                    CapacityName = entity.CapacityName?.CapacityName,
                    MakeId = entity.MakeId,
                    MakeName = entity.MakeName?.MakeName,
                    Model = entity.Model,
                    PurchaseDate = entity.PurchaseDate,
                    LeaseTypeId = entity.LeaseTypeId,
                    LeaseTypeName = entity.LeaseTypeName?.TypeName,
                    SupplierId = entity.SupplierId,
                    SupplierName = entity.SupplierName?.SupplierName,
                    StartKMs = entity.StartKMs,
                    KMs = entity.KMs,
                    StatusId = entity.StatusId,
                    StatusName = entity.AssetStatus?.StatusName,
                    DriverId1 = entity.DriverId1,
                    Driver1Name = entity.DriverName1?.DriverName,
                    DriverId2 = entity.DriverId2,
                    Driver2Name = entity.DriverName2?.DriverName,
                    TrailerId = entity.TrailerId,
                    TrailerName = entity.TrailerName?.TrailerName,
                    FACode = entity.FACode,
                    CityId = entity.CityId,
                    CityName = entity.CityName?.CityName,
                    ClientId = entity.ClientId,
                    ClientName = entity.ClientName?.ClientName,
                    BaseId = entity.BaseId,
                    BaseName = entity.BaseName?.BaseName,
                    CompanyId = entity.CompanyId,
                    CompanyName = entity.CompanyName?.CompanyName,
                    IsActive = entity.IsActive,
                    AssetTyres = entity.AssetTyres.Select(t => new AssetTyreViewModel
                    {
                        DetailId = t.DetailId,
                        AssetId = t.AssetId,
                        SerialNo = t.SerialNo,
                        Make = t.Make,
                        StartKMs = t.StartKMs,
                        IsActive = t.IsActive
                    }).ToList(),
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving asset with ID {AssetId}", id);
                throw;
            }
        }

        public async Task<bool> SaveAssetAsync(AssetViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Asset entity;

                if (model.AssetId.HasValue && model.AssetId > 0)
                {
                    // Update existing
                    entity = await _context.Assets
                        .Include(x => x.AssetTyres)
                        .FirstOrDefaultAsync(x => x.AssetId == model.AssetId.Value);

                    if (entity == null) return false;

                    entity.AssetNo = model.AssetNo;
                    entity.AssetTypeId = model.AssetTypeId;
                    entity.CapacityId = model.CapacityId;
                    entity.MakeId = model.MakeId;
                    entity.Model = model.Model;
                    entity.PurchaseDate = model.PurchaseDate;
                    entity.LeaseTypeId = model.LeaseTypeId;
                    entity.SupplierId = model.SupplierId;
                    entity.StartKMs = model.StartKMs;
                    entity.KMs = model.KMs;
                    entity.StatusId = model.StatusId;
                    entity.DriverId1 = model.DriverId1;
                    entity.DriverId2 = model.DriverId2;
                    entity.TrailerId = model.TrailerId;
                    entity.FACode = model.FACode;
                    entity.CityId = model.CityId;
                    entity.ClientId = model.ClientId;
                    entity.BaseId = model.BaseId;
                    entity.CompanyId = model.CompanyId;
                    entity.IsActive = model.IsActive;
                }
                else
                {
                    // Create new
                    entity = new Asset
                    {
                        AssetNo = model.AssetNo,
                        AssetTypeId = model.AssetTypeId,
                        CapacityId = model.CapacityId,
                        MakeId = model.MakeId,
                        Model = model.Model,
                        PurchaseDate = model.PurchaseDate,
                        LeaseTypeId = model.LeaseTypeId,
                        SupplierId = model.SupplierId,
                        StartKMs = model.StartKMs,
                        KMs = model.KMs,
                        StatusId = model.StatusId,
                        DriverId1 = model.DriverId1,
                        DriverId2 = model.DriverId2,
                        TrailerId = model.TrailerId,
                        FACode = model.FACode,
                        CityId = model.CityId,
                        ClientId = model.ClientId,
                        BaseId = model.BaseId,
                        CompanyId = model.CompanyId,
                        IsActive = model.IsActive
                    };
                    _context.Assets.Add(entity);
                }

                await _context.SaveChangesAsync();

                // Handle Asset Tyres if provided
                if (model.AssetTyres != null && model.AssetTyres.Any())
                {
                    await SaveAssetTyresAsync(entity.AssetId, model.AssetTyres);
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving asset");
                throw;
            }
        }

        public async Task<bool> DeleteAssetAsync(short id)
        {
            try
            {
                var entity = await _context.Assets
                    .Include(x => x.AssetTyres)
                    .FirstOrDefaultAsync(x => x.AssetId == id);

                if (entity == null) return false;

                // Remove related tyres first
                _context.AssetTyres.RemoveRange(entity.AssetTyres);

                // Remove the asset
                _context.Assets.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting asset with ID {AssetId}", id);
                throw;
            }
        }

        public async Task<bool> AssetExistsAsync(string assetNo, short? excludeId = null)
        {
            try
            {
                var query = _context.Assets.Where(x => x.AssetNo == assetNo);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.AssetId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if asset exists");
                throw;
            }
        }

        public async Task<bool> UpdateAssetKilometersAsync(short assetId, decimal kilometers)
        {
            try
            {
                var asset = await _context.Assets.FindAsync(assetId);
                if (asset == null) return false;

                asset.KMs = kilometers;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating kilometers for asset {AssetId}", assetId);
                throw;
            }
        }

        public async Task<IEnumerable<AssetTyreViewModel>> GetAssetTyresAsync(short assetId)
        {
            try
            {
                var tyres = await _context.AssetTyres
                    .Where(x => x.AssetId == assetId)
                    .OrderBy(x => x.SerialNo)
                    .Select(x => new AssetTyreViewModel
                    {
                        DetailId = x.DetailId,
                        AssetId = x.AssetId,
                        SerialNo = x.SerialNo,
                        Make = x.Make,
                        StartKMs = x.StartKMs,
                        IsActive = x.IsActive
                    })
                    .ToListAsync();

                return tyres;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tyres for asset {AssetId}", assetId);
                throw;
            }
        }

        public async Task<AssetLookupsViewModel> GetLookupsAsync()
        {
            try
            {
                var lookups = new AssetLookupsViewModel();

                lookups.AssetTypes = await _context.AssetTypes
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.TypeName)
                   .Select(x => new AssetTypeLookupViewModel
                   {
                       TypeId = x.TypeId,
                       TypeName = x.TypeName
                   })
                   .ToListAsync();

                lookups.AssetStatuses = await _context.AssetStatuses
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.StatusName)
                   .Select(x => new AssetStatusLookupViewModel
                   {
                       StatusId = x.StatusId,
                       StatusName = x.StatusName
                   })
                   .ToListAsync();

                lookups.Capacities = await _context.Capacities
                  .Where(x => x.IsActive)
                  .OrderBy(x => x.CapacityName)
                  .Select(x => new CapacityLookupViewModel
                  {
                      CapacityId = x.CapacityId,
                      CapacityName = x.CapacityName
                  })
                  .ToListAsync();

                lookups.Makes = await _context.Makes
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.MakeName)
                    .Select(x => new MakeLookupViewModel
                    {
                        MakeId = x.MakeId,
                        MakeName = x.MakeName
                    })
                    .ToListAsync();

                lookups.LeaseTypes = await _context.LeaseTypes
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.TypeName)
                    .Select(x => new LeaseTypeLookupViewModel
                    {
                        TypeId = x.TypeId,
                        TypeName = x.TypeName
                    })
                    .ToListAsync();
                lookups.Trailers = await _context.Trailers
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.TrailerName)
                    .Select(x => new TrailerLookupViewModel
                    {
                        TrailerId = x.TrailerId,
                        TrailerName = x.TrailerName
                    })
                    .ToListAsync();
                lookups.Suppliers = await _context.Suppliers
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SupplierName)
                    .Select(x => new SupplierLookupViewModel
                    {
                        SupplierId = x.SupplierId,
                        SupplierName = x.SupplierName
                    })
                    .ToListAsync();
                lookups.Drivers = await _context.Drivers
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.DriverName)
                    .Select(x => new DriverLookupViewModel
                    {
                        DriverId = x.DriverId,
                        DriverName = x.DriverName
                    })
                    .ToListAsync();

                lookups.Cities = await _context.Cities
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.CityName)
                    .Select(x => new ViewModels.CityLookupViewModel
                    {
                        CityId = x.CityId,
                        CityName = x.CityName
                    })
                    .ToListAsync();

                lookups.Clients = await _context.Clients
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.ClientName)
                    .Select(x => new ClientLookupViewModel
                    {
                        ClientId = x.ClientId,
                        ClientName = x.ClientName
                    })
                    .ToListAsync();
                lookups.Bases = await _context.Bases
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.BaseName)
                    .Select(x => new BaseLookupViewModel
                    {
                        BaseId = x.BaseId,
                        BaseName = x.BaseName
                    })
                    .ToListAsync();

                lookups.Companies = await _context.Companies
                   .Where(x => x.IsActive)
                   .OrderBy(x => x.CompanyName)
                   .Select(x => new ViewModels.CompanyLookupViewModel
                   {
                       CompanyId = x.CompanyId,
                       CompanyName = x.CompanyName
                   })
                   .ToListAsync();


                return lookups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lookups");
                throw;
            }
        }

        public async Task<bool> SaveAssetTyresAsync(short assetId, List<AssetTyreViewModel> tyres)
        {
            try
            {
                // Get existing tyres
                var existingTyres = await _context.AssetTyres
                    .Where(x => x.AssetId == assetId)
                    .ToListAsync();

                // Handle deletions
                var tyresToDelete = tyres.Where(t => t.IsDeleted && t.DetailId.HasValue).ToList();
                foreach (var tyreToDelete in tyresToDelete)
                {
                    var existingTyre = existingTyres.FirstOrDefault(t => t.DetailId == tyreToDelete.DetailId);
                    if (existingTyre != null)
                    {
                        _context.AssetTyres.Remove(existingTyre);
                    }
                }

                // Handle updates and new additions
                var activeTyres = tyres.Where(t => !t.IsDeleted).ToList();
                foreach (var tyre in activeTyres)
                {
                    if (tyre.DetailId.HasValue && tyre.DetailId > 0)
                    {
                        // Update existing
                        var existingTyre = existingTyres.FirstOrDefault(t => t.DetailId == tyre.DetailId);
                        if (existingTyre != null)
                        {
                            existingTyre.SerialNo = tyre.SerialNo;
                            existingTyre.Make = tyre.Make;
                            existingTyre.StartKMs = tyre.StartKMs;
                            existingTyre.IsActive = tyre.IsActive;
                        }
                    }
                    else
                    {
                        // Add new
                        var newTyre = new AssetTyre
                        {
                            AssetId = assetId,
                            SerialNo = tyre.SerialNo,
                            Make = tyre.Make,
                            StartKMs = tyre.StartKMs,
                            IsActive = tyre.IsActive
                        };
                        _context.AssetTyres.Add(newTyre);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving asset tyres for asset {AssetId}", assetId);
                throw;
            }
        }
    }
}