using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Common.Services;
using ProcureToPay.Areas.Procurement.Enums;
using ProcureToPay.Areas.Procurement.Models;
using ProcureToPay.Areas.Procurement.Models.DTOs; // Added for CreatePOFromPRRequest
using ProcureToPay.Data;
using static ProcureToPay.Areas.Procurement.Models.PurchaseOrder;
using static ProcureToPay.Areas.Procurement.Models.PurchaseOrderItem;

namespace ProcureToPay.Areas.Procurement.Services
{
    public interface IPurchaseOrderService
    {
        Task<List<PurchaseRequest>> GetEligiblePurchaseRequestsAsync(int userId);
        Task<PurchaseOrder> CreatePOFromPRAsync(CreatePOFromPRRequest request);
        Task<string> GenerateOrderNumberAsync();
        Task<List<PurchaseOrder>> GetPurchaseOrdersForPRAsync(int purchaseRequestId);
        Task<bool> IsPRFullyCoveredByPOsAsync(int purchaseRequestId);
        Task<decimal> GetRemainingPRAmountAsync(int purchaseRequestId);
    }

    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PurchaseOrderService> _logger;

        public PurchaseOrderService(ApplicationDbContext context, ILogger<PurchaseOrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PurchaseRequest>> GetEligiblePurchaseRequestsAsync(int userId)
        {
            // Define the specific role ID
            const int REQUIRED_ROLE_ID = 26;

            // First, check if the current user has the required role.
            bool hasRequiredRole = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == REQUIRED_ROLE_ID);

            // If the user does not have the required role, return an empty list immediately.
            if (!hasRequiredRole)
            {
                return new List<PurchaseRequest>();
            }

            // If the user has the required role, proceed with the main query.
            var approvedPRs = await _context.PurchaseRequests
             .Include(pr => pr.RequestedByUser)
             .Include(pr => pr.Department)
             .Include(pr => pr.Branch)
             .Where(pr => pr.StateId == WorkflowService.STATE_APPROVED)
             .OrderByDescending(pr => pr.RequestDate)
             .ToListAsync();

            var eligiblePRs = new List<PurchaseRequest>();
            foreach (var pr in approvedPRs)
            {
                // This is a simplified check. Your original code had a more complex loop,
                // but the core is that you want to filter based on remaining amount.
                var remainingAmount = await GetRemainingPRAmountAsync(pr.Id);
                if (remainingAmount >= 0)
                {
                    eligiblePRs.Add(pr);
                }
            }

            return eligiblePRs;
        }
        public async Task<PurchaseOrder> CreatePOFromPRAsync(CreatePOFromPRRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate PR exists and is approved
                var purchaseRequest = await _context.PurchaseRequests
                    .Include(pr => pr.Items)
                        .ThenInclude(item => item.UoM)
                    .Include(pr => pr.RequestedByUser)
                        .ThenInclude(u => u.Department)
                    .Include(pr => pr.RequestedByUser)
                        .ThenInclude(u => u.Branch)
                    .FirstOrDefaultAsync(pr => pr.Id == request.PurchaseRequestId);

                if (purchaseRequest == null)
                    throw new InvalidOperationException("Purchase Request not found.");

                if (purchaseRequest.StateId != WorkflowService.STATE_APPROVED)
                    throw new InvalidOperationException("Only approved Purchase Requests can be converted to Purchase Orders.");

                // Check if remaining amount is sufficient
                var remainingAmount = await GetRemainingPRAmountAsync(request.PurchaseRequestId);
                //if (request.MappedAmount > remainingAmount)
                //    throw new InvalidOperationException($"Mapped amount cannot exceed remaining PR amount of {remainingAmount:C}.");

                // Create Purchase Order
                var purchaseOrder = new PurchaseOrder
                {
                    PONumber = await GenerateOrderNumberAsync(), // Corrected from OrderNumber
                    Title = request.Title ?? $"PO from PR: {purchaseRequest.Title}",
                    Description = request.Description ?? $"Purchase Order created from PR #{purchaseRequest.RequestNumber}",
                    PODate = DateTime.UtcNow, // Use UtcNow for consistency
                    ExpectedDeliveryDate = request.ExpectedDeliveryDate,
                    VendorId = (short)request.VendorId,
                    SupplierAddress = request.VendorAddress, // Corrected to SupplierAddress
                    SupplierContact = request.VendorContact, // Corrected to SupplierContact
                    PurchaseNature = request.PurchaseNature,
                    PurchaseType = request.PurchaseType,
                    DepartmentId = purchaseRequest.DepartmentId,
                    BranchId = purchaseRequest.BranchId,
                    Terms = request.Terms,
                    PaymentDays = request.PaymentDays,
                    TaxAmount = request.TaxAmount, // Use the tax amount from the request

                    // Workflow properties - initial state set to SAVED/Draft
                    StateId = WorkflowService.STATE_SAVED,
                    Owner = _context.Users.First(u => u.Id == request.CreatedByUserId).UserName,
                    Status = PurchaseOrderStatus.Draft,
                    WorkFlowTypeId = (short)ProcureToPay.Areas.Common.Enums.WorkFlowType.PurchaseOrder,
                    DepartmentCode = purchaseRequest.DepartmentCode,
                    BranchCode = purchaseRequest.BranchCode,
                    CompanyCode = purchaseRequest.CompanyCode,
                    RequestNatureId = purchaseRequest.RequestNatureId, // Populate from PR
                    RequestTypeId = purchaseRequest.RequestTypeId,      // Populate from PR

                    // Audit fields
                    CreatedByUserId = request.CreatedByUserId,
                    CreatedOn = DateTime.UtcNow
                };

                // Add selected items from PR and update PR's OrderedQuantity
                decimal poTotalAmount = 0;
                foreach (var selectedItem in request.SelectedItems)
                {
                    var prItem = purchaseRequest.Items.FirstOrDefault(i => i.DetailId == selectedItem.PurchaseRequestItemId);
                    if (prItem == null) continue;

                    var poItem = new PurchaseOrderItem
                    {
                        ItemName = prItem.ItemName,
                        Description = prItem.Narration,
                        Quantity = selectedItem.Quantity,
                        UpdatedQuantity = selectedItem.Quantity,
                        UnitPrice = selectedItem.UnitPrice,
                        TotalPrice = selectedItem.Quantity * selectedItem.UnitPrice,
                        TaxRate = selectedItem.TaxRate,
                        TaxAmount = selectedItem.TaxAmount,
                        DiscRate = selectedItem.DiscRate,
                        DiscAmount = selectedItem.DiscAmount,
                        GSTRate = selectedItem.GSTRate,
                        GSTAmount = selectedItem.GSTAmount,
                        UoMId = prItem.UoMId,
                        Unit = prItem.UoM?.UoMName,
                        ExpectedDeliveryDate = selectedItem.ExpectedDeliveryDate,
                        Remarks = selectedItem.Remarks,
                        SourcePurchaseRequestItemId = prItem.DetailId, // Corrected property name for PurchaseOrderItem
                        Status = POItemStatus.Open // Set initial status for PO item
                    };

                    purchaseOrder.Items.Add(poItem);
                    poTotalAmount += poItem.TotalPrice;
                }

                purchaseOrder.TotalAmount = poTotalAmount;
                purchaseOrder.GrandTotal = poTotalAmount + purchaseOrder.TaxAmount;


                // Save Purchase Order (and implicitly the updated PR items due to change tracking)
                _context.PurchaseOrders.Add(purchaseOrder);
                await _context.SaveChangesAsync();

                // Create PR-PO mapping
                var mapping = new PurchaseRequestOrderMapping
                {
                    PurchaseRequestId = request.PurchaseRequestId,
                    PurchaseOrderId = purchaseOrder.Id,
                    MappedAmount = request.MappedAmount, // This should accurately reflect the value of the items from PR on this PO
                    Notes = request.MappingNotes,
                    CreatedByUserId = request.CreatedByUserId,
                    CreatedDate = DateTime.UtcNow
                };

                _context.PurchaseRequestOrderMappings.Add(mapping);
                await _context.SaveChangesAsync(); // Save mapping (and any remaining changes)

                // After saving the PO and updating PR items, check if the PR is now fully covered
                bool isPRFullyCovered = await IsPRFullyCoveredByPOsAsync(request.PurchaseRequestId);

                await transaction.CommitAsync();
                return purchaseOrder;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating purchase order from PR for request: {RequestId}. Error: {Message}",
                                 request.PurchaseRequestId, ex.Message);
                throw; // Re-throw the exception after logging and rolling back
            }
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var date = DateTime.Now;
            var prefix = $"PO{date:yyyyMMdd}"; // Corrected unterminated string

            var lastPO = await _context.PurchaseOrders
                .Where(po => po.PONumber != null && po.PONumber.StartsWith(prefix)) // Check for null PONumber
                .OrderByDescending(po => po.PONumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastPO != null)
            {
                var lastSequence = lastPO.PONumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }
            return $"{prefix}{sequence:D3}"; // e.g., PO20250719001
        }

        public async Task<List<PurchaseOrder>> GetPurchaseOrdersForPRAsync(int purchaseRequestId)
        {
            return await _context.PurchaseRequestOrderMappings
                .Where(m => m.PurchaseRequestId == purchaseRequestId)
                .Select(m => m.PurchaseOrder)
                .Include(po => po.Items)
                .OrderByDescending(po => po.PODate)
                .ToListAsync();
        }

        public async Task<bool> IsPRFullyCoveredByPOsAsync(int purchaseRequestId)
        {
            var pr = await _context.PurchaseRequests
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == purchaseRequestId);

            if (pr == null) return false;

            var totalRequestedQuantity = pr.Items.Sum(item => item.Quantity);
            var totalOrderedQuantity = await _context.PurchaseOrderItems
                .Where(poi => poi.SourcePurchaseRequestItemId.HasValue &&
                              pr.Items.Select(pri => pri.DetailId).Contains(poi.SourcePurchaseRequestItemId.Value))
                .SumAsync(poi => poi.Quantity);

            return totalOrderedQuantity >= totalRequestedQuantity;
        }

        public async Task<decimal> GetRemainingPRAmountAsync(int purchaseRequestId)
        {
            var pr = await _context.PurchaseRequests
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == purchaseRequestId);

            if (pr == null) return 0;

            decimal totalRequestedAmount = pr.Items.Sum(item => item.Quantity * item.UnitPrice);

            

            return totalRequestedAmount;
        }
    }
}
