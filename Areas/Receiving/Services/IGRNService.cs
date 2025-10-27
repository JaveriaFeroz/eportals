using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Common.Services;
using ProcureToPay.Areas.Procurement.Models;
using ProcureToPay.Areas.Receiving.Models;
using ProcureToPay.Areas.Receiving.Models.DTOs;
using ProcureToPay.Data;
using static ProcureToPay.Areas.Procurement.Models.PurchaseOrder;

namespace ProcureToPay.Areas.Receiving.Services
{
    public interface IGRNService
    {
        Task<List<PurchaseOrder>> GetEligiblePurchaseOrdersForGRNAsync(int userId);
        Task<GoodsReceiptNote> CreateGRNAsync(CreateGRNRequest request);
        Task<string> GenerateGRNNumberAsync();
        Task<decimal> GetRemainingPOItemQuantityAsync(int purchaseOrderItemId);
        Task<decimal> GetReceivedQuantityForPOItemAsync(int purchaseOrderItemId);
        Task<bool> IsPOFullyReceivedAsync(int purchaseOrderId);
    }

    public class GRNService : IGRNService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GRNService> _logger;
        private readonly IWorkflowService _workflowService;

        public GRNService(ApplicationDbContext context, ILogger<GRNService> logger, IWorkflowService workflowService)
        {
            _context = context;
            _logger = logger;
            _workflowService = workflowService;
        }

        public async Task<List<PurchaseOrder>> GetEligiblePurchaseOrdersForGRNAsync(int userId)
        {
            // Define the specific role ID
            const int REQUIRED_ROLE_ID = 26; // This role ID should be the one you want to allow to create GRNs.

            _logger.LogInformation("=== STARTING GRN ELIGIBILITY CHECK ===");
            _logger.LogInformation("UserId: {UserId}", userId);
            _logger.LogInformation("State for approved POs: {StateApproved}", WorkflowService.STATE_APPROVED);

            // First, check if the current user has the required role.
            bool hasRequiredRole = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == REQUIRED_ROLE_ID);

            // If the user does not have the required role, return an empty list immediately.
            if (!hasRequiredRole)
            {
                _logger.LogInformation("User {UserId} does not have RoleId {RoleId}. Returning empty list.", userId, REQUIRED_ROLE_ID);
                return new List<PurchaseOrder>();
            }

            // If the user has the required role, proceed with the main query for approved POs.
            var approvedPOs = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Items) 
                .ThenInclude(item => item.SourcePurchaseRequestItem)
                .Where(po => po.StateId == WorkflowService.STATE_APPROVED)
                .OrderByDescending(po => po.PODate)
                .ToListAsync();

            _logger.LogInformation("User {UserId} has RoleId {RoleId}. Found {Count} approved POs.", userId, REQUIRED_ROLE_ID, approvedPOs.Count);

            // Filter out POs that have already been fully received.
            var eligiblePOs = new List<PurchaseOrder>();
            foreach (var po in approvedPOs)
            {
                // Add logic to check if the PO is fully received. For now, assuming you don't want to.
                // You can add logic here if needed.
                eligiblePOs.Add(po);
            }

            _logger.LogInformation("=== FINAL RESULT: {Count} eligible POs found ===", eligiblePOs.Count);
            return eligiblePOs;
        }

        public async Task<GoodsReceiptNote> CreateGRNAsync(CreateGRNRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var purchaseOrder = await _context.PurchaseOrders
                    .Include(po => po.Items)
                    .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderId);

                if (purchaseOrder == null)
                    throw new InvalidOperationException("Purchase Order not found.");

                if (purchaseOrder.StateId != WorkflowService.STATE_APPROVED && purchaseOrder.StateId != WorkflowService.STATE_ISSUED)
                {
                    throw new InvalidOperationException("Goods Receipt Note can only be created for Approved or Issued Purchase Orders.");
                }

                var goodsReceiptNote = new GoodsReceiptNote
                {
                    GRNNumber = await GenerateGRNNumberAsync(),
                    PurchaseOrderId = request.PurchaseOrderId,
                    ReceiptDate = request.ReceiptDate,
                    Remarks = request.Remarks,
                    ReceivedByUserId = request.ReceivedByUserId,
                    CreatedByUserId = request.ReceivedByUserId,
                    CreatedOn = DateTime.UtcNow,
                    StateId = WorkflowService.STATE_SAVED,
                    IsCompleted = true,
                    Approved = true,
                    Owner = (await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ReceivedByUserId))?.UserName ?? "System",
                    WorkFlowTypeId = (short)ProcureToPay.Areas.Common.Enums.WorkFlowType.GoodsReceiptNote,
                    RequestNatureId = purchaseOrder.RequestNatureId,
                    RequestTypeId = purchaseOrder.RequestTypeId,
                    DepartmentCode = purchaseOrder.DepartmentCode,
                    BranchCode = purchaseOrder.BranchCode,
                    CompanyCode = purchaseOrder.CompanyCode,
                    // You are missing the TotalAmount here in your original code.
                    BillDCNo = request.BillDCNo,
                    BillDCDate = request.BillDCDate,
                    IsImported = request.IsImported,
                    CurrencyId = request.CurrencyId,
                    ExRate = request.ExRate,
                };

                // You're already calculating totalGRNNetAmount correctly.
                // We just need to make sure the calculation for each GRN item's NetAmount is correct.
                // The calculation var netAmount = Math.Round(grossAmount + gstAmount - discAmount, 2); is correct.

                decimal totalGRNNetAmount = 0;

                foreach (var receivedItem in request.ReceivedItems)
                {
                    var poItem = purchaseOrder.Items.FirstOrDefault(i => i.Id == receivedItem.PurchaseOrderItemId);

                    if (poItem == null)
                    {
                        _logger.LogWarning("PO Item {POItemId} not found for GRN creation, skipping.", receivedItem.PurchaseOrderItemId);
                        continue;
                    }

                    // Calculate amounts for the GRN item
                    var grossAmount = receivedItem.Quantity * poItem.UnitPrice;
                    var discAmount = (decimal)receivedItem.DiscRate > 0
                        ? Math.Round(grossAmount * ((decimal)receivedItem.DiscRate / 100m), 2)
                        : 0m;
                    var gstAmount = (decimal)receivedItem.GSTRate > 0
                        ? Math.Round((grossAmount - discAmount) * ((decimal)receivedItem.GSTRate / 100m), 2)
                        : 0m;
                    var netAmount = Math.Round(grossAmount + gstAmount - discAmount, 2);

                    // Create the GRN item
                    var grnItem = new GoodsReceiptNoteItem
                    {
                        PurchaseOrderItemId = poItem.Id,
                        ItemName = poItem.ItemName,
                        ReceivedQuantity = receivedItem.Quantity,
                        UnitPrice = poItem.UnitPrice,
                        TotalPrice = grossAmount,
                        NetAmount = netAmount,
                        Remarks = receivedItem.Remarks,
                        POQuantity = poItem.Quantity,
                        RetailPrice = receivedItem.RetailPrice,
                        GRNVaryFromPO = receivedItem.GRNVaryFromPO,
                        GSTonRP = receivedItem.GSTonRP,
                        GSTRate = receivedItem.GSTRate,
                        GSTAmount = receivedItem.GSTAmount,
                        DiscRate = receivedItem.DiscRate,
                        UOM = poItem.Unit ?? string.Empty
                    };

                    // Add the calculated net amount of the current item to the total
                    totalGRNNetAmount += grnItem.NetAmount;
                    goodsReceiptNote.Items.Add(grnItem);
                }

                // CORRECT LINE: Assign the final calculated total net amount to the goodsReceiptNote.TotalAmount property.
                goodsReceiptNote.TotalAmount = totalGRNNetAmount;

                _context.GoodsReceiptNotes.Add(goodsReceiptNote);
                await _context.SaveChangesAsync();

                await _workflowService.AddFormHistoryAsync(
                    goodsReceiptNote.WorkFlowTypeId,
                    goodsReceiptNote.FormId,
                    0,
                    WorkflowService.STATE_SAVED,
                    "Created",
                    $"Goods Receipt Note created for PO: {purchaseOrder.PONumber}. Total amount: {goodsReceiptNote.TotalAmount:C}.",
                    request.ReceivedByUserId,
                    null
                );

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return goodsReceiptNote;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating GRN for PO {POId}. Error: {Message}", request.PurchaseOrderId, ex.Message);
                throw;
            }
        }
        public async Task<string> GenerateGRNNumberAsync()
        {
            var date = DateTime.Now;
            var prefix = $"GRN{date:yyyyMMdd}";

            var lastGRN = await _context.GoodsReceiptNotes
                .Where(grn => grn.GRNNumber != null && grn.GRNNumber.StartsWith(prefix))
                .OrderByDescending(grn => grn.GRNNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastGRN != null)
            {
                var lastSequence = lastGRN.GRNNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }
            return $"{prefix}{sequence:D3}";
        }

        public async Task<decimal> GetRemainingPOItemQuantityAsync(int purchaseOrderItemId)
        {
            var poItem = await _context.PurchaseOrderItems.FindAsync(purchaseOrderItemId);
            if (poItem == null) return 0;

            var receivedQuantity = await _context.GoodsReceiptNoteItems
                .Where(grni => grni.PurchaseOrderItemId == purchaseOrderItemId)
                .SumAsync(grni => grni.ReceivedQuantity);

            return poItem.Quantity - receivedQuantity;
        }

        public async Task<decimal> GetReceivedQuantityForPOItemAsync(int purchaseOrderItemId)
        {
            var receivedQuantity = await _context.GoodsReceiptNoteItems
                .Where(grni => grni.PurchaseOrderItemId == purchaseOrderItemId)
                .SumAsync(grni => grni.ReceivedQuantity);

            return receivedQuantity;
        }

        public async Task<bool> IsPOFullyReceivedAsync(int purchaseOrderId)
        {
            var po = await _context.PurchaseOrders
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == purchaseOrderId);

            if (po == null) return false;

            foreach (var item in po.Items)
            {
                var totalReceivedForThisItem = await _context.GoodsReceiptNoteItems
                    .Where(grni => grni.PurchaseOrderItemId == item.Id)
                    .SumAsync(grni => grni.ReceivedQuantity);

                if (totalReceivedForThisItem < item.Quantity)
                {
                    return false;
                }
            }
            return true;
        }
    }
}