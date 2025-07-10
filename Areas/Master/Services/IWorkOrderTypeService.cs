using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.Common.Services
{
    public interface IWorkOrderTypeService
    {
        Task<IEnumerable<WorkOrderTypeListViewModel>> GetWorkOrderTypesAsync(short? workFlowId = null, bool activeOnly = true, string searchTerm = null);
        Task<WorkOrderTypeViewModel> GetWorkOrderTypeByIdAsync(short workOrderTypeId);
        Task<bool> SaveWorkOrderTypeAsync(WorkOrderTypeViewModel model);
        Task<bool> DeleteWorkOrderTypeAsync(short workOrderTypeId);
        Task<bool> WorkOrderTypeExistsAsync(string workOrderTypeName, short workFlowId, short? excludeId = null);
        Task<bool> WorkOrderTypeCodeExistsAsync(string workOrderTypeCode, short? excludeId = null);
        Task<IEnumerable<WorkOrderTypeSelectListItem>> GetWorkOrderTypesForDropdownAsync(short? workFlowId = null, bool activeOnly = true);
        Task<bool> UpdateDisplayOrderAsync(short workOrderTypeId, int newDisplayOrder);
    }

    public class WorkOrderTypeService : IWorkOrderTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WorkOrderTypeService> _logger;

        public WorkOrderTypeService(ApplicationDbContext context, ILogger<WorkOrderTypeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<WorkOrderTypeListViewModel>> GetWorkOrderTypesAsync(short? workFlowId = null, bool activeOnly = true, string searchTerm = null)
        {
            try
            {
                var query = _context.WorkOrderTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .AsQueryable();

                if (workFlowId.HasValue)
                {
                    query = query.Where(x => x.WorkFlowId == workFlowId.Value);
                }

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var searchTermLower = searchTerm.ToLower();
                    query = query.Where(x =>
                        x.WorkOrderTypeName.ToLower().Contains(searchTermLower) ||
                        x.WorkOrderTypeCode.ToLower().Contains(searchTermLower) ||
                        (x.Description != null && x.Description.ToLower().Contains(searchTermLower)));
                }

                var result = await query
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.WorkOrderTypeName)
                    .Select(x => new WorkOrderTypeListViewModel
                    {
                        WorkOrderTypeId = x.WorkOrderTypeId,
                        WorkOrderTypeName = x.WorkOrderTypeName,
                        WorkOrderTypeCode = x.WorkOrderTypeCode,
                        WorkFlowId = x.WorkFlowId,
                        IsActive = x.IsActive,
                        Description = x.Description,
                        DisplayOrder = x.DisplayOrder,
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
                _logger.LogError(ex, "Error retrieving work order types for WorkFlowId: {WorkFlowId}", workFlowId);
                throw;
            }
        }

        public async Task<WorkOrderTypeViewModel> GetWorkOrderTypeByIdAsync(short workOrderTypeId)
        {
            try
            {
                var entity = await _context.WorkOrderTypes
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .FirstOrDefaultAsync(x => x.WorkOrderTypeId == workOrderTypeId);

                if (entity == null) return null;

                return new WorkOrderTypeViewModel
                {
                    WorkOrderTypeId = entity.WorkOrderTypeId,
                    WorkOrderTypeName = entity.WorkOrderTypeName,
                    WorkOrderTypeCode = entity.WorkOrderTypeCode,
                    WorkFlowId = entity.WorkFlowId,
                    IsActive = entity.IsActive,
                    Description = entity.Description,
                    DisplayOrder = entity.DisplayOrder,
                    CreatedBy = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "System",
                    CreatedOn = entity.CreatedOn,
                    UpdatedBy = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                    UpdatedOn = entity.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving work order type with ID {WorkOrderTypeId}", workOrderTypeId);
                throw;
            }
        }

        public async Task<bool> SaveWorkOrderTypeAsync(WorkOrderTypeViewModel model)
        {
            try
            {
                WorkOrderType entity;

                if (model.WorkOrderTypeId.HasValue && model.WorkOrderTypeId > 0)
                {
                    // Update existing
                    entity = await _context.WorkOrderTypes.FindAsync(model.WorkOrderTypeId.Value);
                    if (entity == null) return false;

                    entity.WorkOrderTypeName = model.WorkOrderTypeName;
                    entity.WorkOrderTypeCode = model.WorkOrderTypeCode;
                    entity.WorkFlowId = model.WorkFlowId;
                    entity.IsActive = model.IsActive;
                    entity.Description = model.Description;
                    entity.DisplayOrder = model.DisplayOrder;
                    // No need to set UpdatedBy/UpdatedOn - handled automatically by DbContext
                }
                else
                {
                    // Create new - set display order to max + 1 if not specified
                    if (model.DisplayOrder == 0)
                    {
                        var maxDisplayOrder = await _context.WorkOrderTypes
                            .Where(x => x.WorkFlowId == model.WorkFlowId)
                            .MaxAsync(x => (int?)x.DisplayOrder) ?? 0;
                        model.DisplayOrder = maxDisplayOrder + 1;
                    }

                    entity = new WorkOrderType
                    {
                        WorkOrderTypeName = model.WorkOrderTypeName,
                        WorkOrderTypeCode = model.WorkOrderTypeCode,
                        WorkFlowId = model.WorkFlowId,
                        IsActive = model.IsActive,
                        Description = model.Description,
                        DisplayOrder = model.DisplayOrder
                        // No need to set CreatedBy/CreatedOn - handled automatically by DbContext
                    };
                    _context.WorkOrderTypes.Add(entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving work order type: {WorkOrderTypeName}", model.WorkOrderTypeName);
                throw;
            }
        }

        public async Task<bool> DeleteWorkOrderTypeAsync(short workOrderTypeId)
        {
            try
            {
                var entity = await _context.WorkOrderTypes.FindAsync(workOrderTypeId);
                if (entity == null) return false;

                _context.WorkOrderTypes.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting work order type with ID {WorkOrderTypeId}", workOrderTypeId);
                throw;
            }
        }

        public async Task<bool> WorkOrderTypeExistsAsync(string workOrderTypeName, short workFlowId, short? excludeId = null)
        {
            try
            {
                var query = _context.WorkOrderTypes
                    .Where(x => x.WorkOrderTypeName.ToLower() == workOrderTypeName.ToLower() && x.WorkFlowId == workFlowId);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.WorkOrderTypeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if work order type exists: {WorkOrderTypeName}", workOrderTypeName);
                throw;
            }
        }

        public async Task<bool> WorkOrderTypeCodeExistsAsync(string workOrderTypeCode, short? excludeId = null)
        {
            try
            {
                var query = _context.WorkOrderTypes
                    .Where(x => x.WorkOrderTypeCode.ToLower() == workOrderTypeCode.ToLower());

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.WorkOrderTypeId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if work order type code exists: {WorkOrderTypeCode}", workOrderTypeCode);
                throw;
            }
        }

        public async Task<IEnumerable<WorkOrderTypeSelectListItem>> GetWorkOrderTypesForDropdownAsync(short? workFlowId = null, bool activeOnly = true)
        {
            try
            {
                var query = _context.WorkOrderTypes.AsQueryable();

                if (workFlowId.HasValue)
                {
                    query = query.Where(x => x.WorkFlowId == workFlowId.Value);
                }

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                var result = await query
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.WorkOrderTypeName)
                    .Select(x => new WorkOrderTypeSelectListItem
                    {
                        WorkOrderTypeId = x.WorkOrderTypeId,
                        WorkOrderTypeName = x.WorkOrderTypeName,
                        WorkOrderTypeCode = x.WorkOrderTypeCode,
                        WorkFlowId = x.WorkFlowId
                    })
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving work order types for dropdown");
                throw;
            }
        }

        public async Task<bool> UpdateDisplayOrderAsync(short workOrderTypeId, int newDisplayOrder)
        {
            try
            {
                var entity = await _context.WorkOrderTypes.FindAsync(workOrderTypeId);
                if (entity == null) return false;

                entity.DisplayOrder = newDisplayOrder;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating display order for work order type ID {WorkOrderTypeId}", workOrderTypeId);
                throw;
            }
        }
    }
}