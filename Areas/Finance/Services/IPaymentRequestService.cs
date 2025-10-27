using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Common.Services;
using ProcureToPay.Areas.Finance.Models;
using ProcureToPay.Areas.Finance.Models.DTOs;
using ProcureToPay.Areas.Receiving.Models;
using ProcureToPay.Areas.Common.Enums;
using ProcureToPay.Data;
using static ProcureToPay.Areas.Common.Services.WorkflowService;
using ProcureToPay.Helpers;

namespace ProcureToPay.Areas.Finance.Services
{
    public interface IPaymentRequestService
    {
        Task<List<GoodsReceiptNote>> GetEligibleGRNsForPaymentRequestAsync(int userId);
        Task<PaymentRequest> CreateGRNBasedPaymentRequestAsync(CreateGRNBasedPaymentRequestDto request);
        Task<PaymentRequest> CreateDirectPaymentRequestAsync(CreateDirectPaymentRequestDto request);
       
        Task<string> GeneratePaymentRequestNumberAsync();
        Task<bool> IsGRNFullyProcessedForPaymentAsync(int grnId);
        Task<decimal> GetTotalPaymentRequestAmountForGRNAsync(int grnId);
        Task<PaymentRequest?> GetPaymentRequestByIdAsync(int id);
        Task<List<PaymentRequest>> GetPaymentRequestsAsync(int? userId = null, short? stateId = null);
        Task<bool> UpdatePaymentRequestAsync(PaymentRequest paymentRequest);
        Task<bool> DeletePaymentRequestAsync(int id);
        Task<List<AttachmentType>> GetAttachmentTypesForPaymentRequestAsync(short? paymentNatureId = null, short? stateId = null);
        Task<PaymentRequest> SubmitPaymentRequestAsync(int paymentRequestId, int userId);
        Task<bool> CanTransitionToFinanceWorkflowAsync(PaymentRequest paymentRequest);
        Task<List<PaymentRequestState>> GetValidNextStatesAsync(PaymentRequest paymentRequest);
        Task<List<short>> GetMandatoryAttachmentTypesForPaymentNatureAsync(short? paymentNatureId);

        Task<bool> UpdatePivNoAsync(int paymentRequestId, string pivNumber); // NEW method
        Task<bool> UpdateCSNoAsync(int paymentRequestId, string csNumber); // NEW method

    }

    public class PaymentRequestService : IPaymentRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentRequestService> _logger;
        private readonly IWorkflowService _workflowService;

        public PaymentRequestService(
            ApplicationDbContext context,
            ILogger<PaymentRequestService> logger,
            IWorkflowService workflowService)
        {
            _context = context;
            _logger = logger;
            _workflowService = workflowService;
        }

        public async Task<List<GoodsReceiptNote>> GetEligibleGRNsForPaymentRequestAsync(int userId)
        {
            _logger.LogInformation("Getting eligible GRNs for Payment Request creation for user {UserId}", userId);

            // Get GRNs that are completed/approved and not fully paid
            var eligibleGRNs = await _context.GoodsReceiptNotes
                .Include(g => g.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .Include(g => g.Items)
                .Where(g => g.StateId == (short)PaymentRequestState.Saved)
                .ToListAsync();

            return eligibleGRNs.OrderByDescending(g => g.ReceiptDate).ToList();
        }

        public async Task<PaymentRequest> CreateGRNBasedPaymentRequestAsync(CreateGRNBasedPaymentRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var grn = await _context.GoodsReceiptNotes
                    .Include(g => g.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                    .FirstOrDefaultAsync(g => g.Id == request.GoodsReceiptNoteId);

                if (grn == null)
                    throw new InvalidOperationException("Goods Receipt Note not found.");

                if (grn.StateId != (short)PaymentRequestState.Saved)
                    throw new InvalidOperationException("Payment Request can only be created for acknowledged GRNs.");

                var paymentRequest = new PaymentRequest
                {
                    PRQNumber = await GeneratePaymentRequestNumberAsync(),
                    RequiredDate = request.RequiredDate,
                    GoodsReceiptNoteId = grn.Id,
                    PayeeName = request.PayeeName,
                    SupplierId = request.SupplierId,
                    PaymentModeId = request.PaymentModeId,
                    PaymentTypeId = request.PaymentTypeId,
                    PaymentNatureId = request.PaymentNatureId,
                    PaymentSubNatureId = request.PaymentSubNatureId,
                    CurrencyId = request.CurrencyId,
                    SelfApplicant = request.SelfApplicant,
                    PIVNo = request.PIVNo,
                    CSNo = request.CSNo,
                    CreatedByUserId = request.CreatedByUserId,
                    CreatedOn = DateTime.UtcNow,
                    StateId = (short)PaymentRequestState.Saved,
                    Owner = (await _context.Users.FindAsync(request.CreatedByUserId))?.UserName ?? "Unknown",

                    // Inherit workflow properties from GRN
                    RequestNatureId = request.RequestNatureId,
                    RequestTypeId = request.RequestTypeId,
                    DepartmentId = request.DepartmentId,
                    BranchId = request.BranchId,
                    DepartmentCode = request.DepartmentCode,
                    BranchCode = request.BranchCode,
                    CompanyCode = request.CompanyCode
                };

                foreach (var detailDto in request.Details)
                {
                    var detail = new PaymentRequestDetail
                    {
                        InvoiceNo = detailDto.InvoiceNo,
                        InvoiceDate = detailDto.InvoiceDate,
                        Description = detailDto.Description,
                        AmountExTax = detailDto.AmountExTax,
                        STRate = detailDto.STRate,
                        OtherTax = detailDto.OtherTax,
                        TotalAmount = detailDto.TotalAmount
                    };

                    paymentRequest.Details.Add(detail);
                }

                // Add cost allocations if any
                foreach (var allocationDto in request.CostAllocations)
                {
                    // Find the department and branch names based on the submitted codes
                    var department = await _context.Departments
                        .FirstOrDefaultAsync(d => d.DepartmentId.ToString() == allocationDto.DepartmentCode);

                    var branch = await _context.Branches
                        .FirstOrDefaultAsync(b => b.BranchId.ToString() == allocationDto.BranchCode);

                    // Create the new PaymentRequestCostAllocation object
                    var allocation = new PaymentRequestCostAllocation
                    {
                        BranchCode = allocationDto.BranchCode,
                        // Populate the name from the lookup
                        BranchName = branch?.BranchName ?? "Unknown Branch",
                        DepartmentCode = allocationDto.DepartmentCode,
                        // Populate the name from the lookup
                        DepartmentName = department?.DepartmentName ?? "Unknown Department",
                        Rate = allocationDto.Rate
                    };

                    paymentRequest.CostAllocations.Add(allocation);
                }

                foreach (var attachmentDto in request.Attachments)
                {
                    // Check if the AttachmentTypeId exists in the database
                    bool attachmentTypeExists = await _context.AttachmentTypes.AnyAsync(at => at.Id == attachmentDto.AttachmentTypeId);

                    if (attachmentTypeExists)
                    {
                        var attachment = new PaymentRequestAttachment
                        {
                            AttachmentTypeId = attachmentDto.AttachmentTypeId,
                            FileName = attachmentDto.FileName,
                            FileContentType = attachmentDto.FileContentType,
                            FileSizeKB = attachmentDto.FileSizeKB,
                            FileContent = attachmentDto.FileContent,
                            CreatedOn = DateTime.UtcNow,
                            CreatedByUserId = request.CreatedByUserId
                        };
                        paymentRequest.Attachments.Add(attachment);
                    }
                    else
                    {
                        // Log a warning or handle the error gracefully,
                        // perhaps by skipping the attachment or returning a specific error message.
                        _logger.LogWarning("Invalid AttachmentTypeId {AttachmentTypeId} found. Skipping attachment.", attachmentDto.AttachmentTypeId);
                    }
                }

                _context.PaymentRequests.Add(paymentRequest);
                await _context.SaveChangesAsync();

                // Add workflow history
                await _workflowService.AddFormHistoryAsync(
                    paymentRequest.WorkFlowTypeId,
                    paymentRequest.FormId,
                    0,
                    (short)PaymentRequestState.Saved,
                    "Created",
                    $"Payment Request created for GRN: {grn.GRNNumber}. Amount: {paymentRequest.TotalAmount:C}",
                    request.CreatedByUserId,
                    null
                );

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return paymentRequest;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating GRN-based Payment Request for GRN {GRNId}", request.GoodsReceiptNoteId);
                throw;
            }
        }

        public async Task<PaymentRequest> CreateDirectPaymentRequestAsync(CreateDirectPaymentRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var paymentRequest = new PaymentRequest
                {
                    PRQNumber = await GeneratePaymentRequestNumberAsync(),
                    RequiredDate = request.RequiredDate,
                    PayeeName = request.PayeeName,
                    SupplierId = request.SupplierId,
                    PaymentModeId = request.PaymentModeId,
                    PaymentTypeId = request.PaymentTypeId,
                    PaymentNatureId = request.PaymentNatureId,
                    PaymentSubNatureId = request.PaymentSubNatureId,
                    CurrencyId = request.CurrencyId,
                    SelfApplicant = request.SelfApplicant,
                    PIVNo = request.PIVNo,
                    CSNo = request.CSNo,
                    CreatedByUserId = request.CreatedByUserId,
                    CreatedOn = DateTime.UtcNow,
                    StateId = (short)PaymentRequestState.Saved,
                    Owner = (await _context.Users.FindAsync(request.CreatedByUserId))?.UserName ?? "Unknown",

                    // Set workflow properties from request
                    RequestNatureId = request.RequestNatureId,
                    RequestTypeId = request.RequestTypeId,
                    DepartmentId = request.DepartmentId,
                    BranchId = request.BranchId,
                    DepartmentCode = request.DepartmentCode,
                    BranchCode = request.BranchCode,
                    CompanyCode = request.CompanyCode
                };

                // Add details
                foreach (var detailDto in request.Details)
                {
                    var detail = new PaymentRequestDetail
                    {
                        InvoiceNo = detailDto.InvoiceNo,
                        InvoiceDate = detailDto.InvoiceDate,
                        Description = detailDto.Description,
                        AmountExTax = detailDto.AmountExTax,
                        STRate = detailDto.STRate,
                        OtherTax = detailDto.OtherTax,
                        TotalAmount = detailDto.TotalAmount
                    };

                    paymentRequest.Details.Add(detail);
                }

                // Add cost allocations if any
                foreach (var allocationDto in request.CostAllocations)
                {
                    // Find the department and branch names based on the submitted codes
                    var department = await _context.Departments
                        .FirstOrDefaultAsync(d => d.DepartmentId.ToString() == allocationDto.DepartmentCode);

                    var branch = await _context.Branches
                        .FirstOrDefaultAsync(b => b.BranchId.ToString() == allocationDto.BranchCode);

                    // Create the new PaymentRequestCostAllocation object
                    var allocation = new PaymentRequestCostAllocation
                    {
                        BranchCode = allocationDto.BranchCode,
                        // Populate the name from the lookup
                        BranchName = branch?.BranchName ?? "Unknown Branch",
                        DepartmentCode = allocationDto.DepartmentCode,
                        // Populate the name from the lookup
                        DepartmentName = department?.DepartmentName ?? "Unknown Department",
                        Rate = allocationDto.Rate
                    };

                    paymentRequest.CostAllocations.Add(allocation);
                }

                foreach (var attachmentDto in request.Attachments)
                {
                    // Check if the AttachmentTypeId exists in the database
                    bool attachmentTypeExists = await _context.AttachmentTypes.AnyAsync(at => at.Id == attachmentDto.AttachmentTypeId);

                    if (attachmentTypeExists)
                    {
                        var attachment = new PaymentRequestAttachment
                        {
                            AttachmentTypeId = attachmentDto.AttachmentTypeId,
                            FileName = attachmentDto.FileName,
                            FileContentType = attachmentDto.FileContentType,
                            FileSizeKB = attachmentDto.FileSizeKB,
                            FileContent = attachmentDto.FileContent,
                            CreatedOn = DateTime.UtcNow,
                            CreatedByUserId = request.CreatedByUserId
                        };
                        paymentRequest.Attachments.Add(attachment);
                    }
                    else
                    {
                        // Log a warning or handle the error gracefully,
                        // perhaps by skipping the attachment or returning a specific error message.
                        _logger.LogWarning("Invalid AttachmentTypeId {AttachmentTypeId} found. Skipping attachment.", attachmentDto.AttachmentTypeId);
                    }
                }

                _context.PaymentRequests.Add(paymentRequest);
                await _context.SaveChangesAsync();

                // Add workflow history
                await _workflowService.AddFormHistoryAsync(
                    paymentRequest.WorkFlowTypeId,
                    paymentRequest.FormId,
                    0,
                    (short)PaymentRequestState.Saved,
                    "Created",
                    $"Direct Payment Request created. Amount: {paymentRequest.TotalAmount:C}",
                    request.CreatedByUserId,
                    null
                );

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return paymentRequest;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating direct Payment Request for user {UserId}", request.CreatedByUserId);
                throw;
            }
        }

        public async Task<PaymentRequest> SubmitPaymentRequestAsync(int paymentRequestId, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var paymentRequest = await GetPaymentRequestByIdAsync(paymentRequestId);
                if (paymentRequest == null)
                    throw new InvalidOperationException("Payment Request not found.");

                if (paymentRequest.StateId != (short)PaymentRequestState.Saved)
                    throw new InvalidOperationException("Only saved payment requests can be submitted.");

                var currentState = (PaymentRequestState)paymentRequest.StateId;
                var nextState = PaymentRequestState.SubmittedForApproval;

                // Check if this is an operational payment that should go to finance
                if (paymentRequest.PaymentNatureId == (short)PaymentNature.Operational &&
                    await CanTransitionToFinanceWorkflowAsync(paymentRequest))
                {
                    nextState = PaymentRequestState.SubmittedForVerification;
                }

                paymentRequest.StateId = (short)nextState;
                paymentRequest.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                await _context.SaveChangesAsync();

                // Add workflow history
                var action = nextState == PaymentRequestState.SubmittedForVerification ?
                    "Submitted for Finance Verification" : "Submitted for Approval";

                await _workflowService.AddFormHistoryAsync(
                    paymentRequest.WorkFlowTypeId,
                    paymentRequest.FormId,
                    (short)currentState,
                    (short)nextState,
                    action,
                    $"Payment Request submitted. Amount: {paymentRequest.TotalAmount:C}",
                    userId,
                    null
                );

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Payment Request {PRId} submitted to state {State} by user {UserId}",
                    paymentRequestId, nextState, userId);

                return paymentRequest;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error submitting Payment Request {PRId}", paymentRequestId);
                throw;
            }
        }

        public async Task<bool> CanTransitionToFinanceWorkflowAsync(PaymentRequest paymentRequest)
        {
            // Business logic to determine if operational payment should go through finance
            // You can add more conditions here based on your business rules
            return paymentRequest.PaymentNatureId == (short)PaymentNature.Operational;
        }

        public async Task<List<PaymentRequestState>> GetValidNextStatesAsync(PaymentRequest paymentRequest)
        {
            var currentState = (PaymentRequestState)paymentRequest.StateId;
            var validNextStates = new List<PaymentRequestState>();

            switch (currentState)
            {
                case PaymentRequestState.Saved:
                    validNextStates.Add(PaymentRequestState.SubmittedForApproval);
                    if (paymentRequest.PaymentNatureId == (short)PaymentNature.Operational)
                    {
                        validNextStates.Add(PaymentRequestState.SubmittedForVerification);
                    }
                    break;

                case PaymentRequestState.SubmittedForApproval:
                    validNextStates.Add(PaymentRequestState.Approved);
                    validNextStates.Add(PaymentRequestState.Rejected);
                    validNextStates.Add(PaymentRequestState.Returned);
                    break;

                case PaymentRequestState.SubmittedForVerification:
                    validNextStates.Add(PaymentRequestState.SubmittedToPIVDesk);
                    validNextStates.Add(PaymentRequestState.Returned);
                    break;

                case PaymentRequestState.SubmittedToPIVDesk:
                    validNextStates.Add(PaymentRequestState.SubmittedForPIVVerification);
                    validNextStates.Add(PaymentRequestState.ReturnedToPIVDesk);
                    break;

                case PaymentRequestState.SubmittedForPIVVerification:
                    validNextStates.Add(PaymentRequestState.Approved);
                    validNextStates.Add(PaymentRequestState.ReturnedToPIVDesk);
                    break;

                case PaymentRequestState.Approved:
                    validNextStates.Add(PaymentRequestState.InterfacedToControl);
                    break;

                case PaymentRequestState.InterfacedToControl:
                    validNextStates.Add(PaymentRequestState.FundsDisbursement);
                    break;

                case PaymentRequestState.FundsDisbursement:
                    validNextStates.Add(PaymentRequestState.ChequeDispatch);
                    validNextStates.Add(PaymentRequestState.RequestCompleted);
                    break;

                case PaymentRequestState.ChequeDispatch:
                    validNextStates.Add(PaymentRequestState.RequestCompleted);
                    break;

                case PaymentRequestState.Returned:
                case PaymentRequestState.ReturnedToPIVDesk:
                    validNextStates.Add(PaymentRequestState.Saved);
                    break;
            }

            return validNextStates;
        }

        public async Task<string> GeneratePaymentRequestNumberAsync()
        {
            var date = DateTime.Now;
            var prefix = $"PRQ{date:yyyyMMdd}";

            var lastPR = await _context.PaymentRequests
                .Where(pr => pr.PRQNumber != null && pr.PRQNumber.StartsWith(prefix))
                .OrderByDescending(pr => pr.PRQNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastPR != null && lastPR.PRQNumber != null)
            {
                var lastSequence = lastPR.PRQNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"{prefix}{sequence:D3}";
        }

        public async Task<bool> IsGRNFullyProcessedForPaymentAsync(int grnId)
        {
            var grn = await _context.GoodsReceiptNotes.FindAsync(grnId);
            if (grn == null) return false;

            var totalPaymentRequested = await GetTotalPaymentRequestAmountForGRNAsync(grnId);
            return totalPaymentRequested >= grn.TotalAmount;
        }

        public async Task<decimal> GetTotalPaymentRequestAmountForGRNAsync(int grnId)
        {
            var relevantDetails = await _context.PaymentRequests
                .Where(pr => pr.GoodsReceiptNoteId == grnId &&
                             pr.StateId != (short)PaymentRequestState.Rejected &&
                             pr.StateId != (short)PaymentRequestState.Cancelled)
                .SelectMany(pr => pr.Details)
                .ToListAsync(); // This forces client-side evaluation

            return relevantDetails.Sum(d => d.TotalAmount);
        }

        public async Task<PaymentRequest?> GetPaymentRequestByIdAsync(int id)
        {
            return await _context.PaymentRequests
                .Include(pr => pr.GoodsReceiptNote)
                    .ThenInclude(grn => grn.PurchaseOrder)
                        .ThenInclude(po => po.Supplier)
                .Include(pr => pr.CreatedByUser)
                .Include(pr => pr.Details)
                .Include(pr => pr.Attachments)
                .Include(pr => pr.CostAllocations)
                .FirstOrDefaultAsync(pr => pr.Id == id);
        }

        public async Task<List<PaymentRequest>> GetPaymentRequestsAsync(int? userId = null, short? stateId = null)
        {
            var query = _context.PaymentRequests
                .Include(pr => pr.GoodsReceiptNote)
                .Include(pr => pr.CreatedByUser)
                .Include(pr => pr.Details)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(pr => pr.CreatedByUserId == userId.Value);
            }

            if (stateId.HasValue)
            {
                query = query.Where(pr => pr.StateId == stateId.Value);
            }

            return await query.OrderByDescending(pr => pr.CreatedOn).ToListAsync();
        }

        public async Task<bool> UpdatePaymentRequestAsync(PaymentRequest paymentRequest)
        {
            try
            {
                _context.PaymentRequests.Update(paymentRequest);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Payment Request {PRId}", paymentRequest.Id);
                return false;
            }
        }

        public async Task<bool> DeletePaymentRequestAsync(int id)
        {
            try
            {
                var paymentRequest = await _context.PaymentRequests.FindAsync(id);
                if (paymentRequest == null) return false;

                if (paymentRequest.StateId != (short)PaymentRequestState.Saved)
                {
                    throw new InvalidOperationException("Only saved payment requests can be deleted.");
                }

                _context.PaymentRequests.Remove(paymentRequest);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Payment Request {PRId}", id);
                return false;
            }
        }

        public async Task<List<AttachmentType>> GetAttachmentTypesForPaymentRequestAsync(short? paymentNatureId = null, short? stateId = null)
        {
            var query = _context.AttachmentTypes
                .Where(at => at.IsActive &&
                           at.WorkFlowTypeId == (short)WorkFlowType.PaymentRequest)
                .AsQueryable();

            if (paymentNatureId.HasValue)
            {
                query = query.Where(at => at.PaymentNatureId == null || at.PaymentNatureId == paymentNatureId.Value);
            }

            if (stateId.HasValue)
            {
                query = query.Where(at => at.StateId == null || at.StateId == stateId.Value);
            }

            return await query.OrderBy(at => at.Name).ToListAsync();
        }

        public async Task<List<short>> GetMandatoryAttachmentTypesForPaymentNatureAsync(short? paymentNatureId)
        {
            _logger.LogInformation("Getting mandatory attachment types for Payment Nature ID {PaymentNatureId}", paymentNatureId);

            var query = _context.AttachmentTypes
                .Where(at =>
                    at.IsActive &&
                    at.IsMandatory &&
                    at.WorkFlowTypeId == (short)WorkFlowType.PaymentRequest)
                .AsQueryable();

            // If a specific payment nature is provided, filter by it.
            // The condition `at.PaymentNatureId == null` is for attachments mandatory for ALL payment natures.
            if (paymentNatureId.HasValue)
            {
                query = query.Where(at =>
                    at.PaymentNatureId == null ||
                    at.PaymentNatureId == paymentNatureId.Value);
            }

            return await query.Select(at => at.Id).ToListAsync();
        }

        public async Task<bool> UpdatePivNoAsync(int paymentRequestId, string pivNumber)
        {
            try
            {
                // Step 1: Check if the PIV number already exists for another payment request.
                // We exclude the current request ID from the check.
                var isDuplicate = await _context.PaymentRequests
                                                .AnyAsync(pr => pr.PIVNo == pivNumber && pr.Id != paymentRequestId);

                if (isDuplicate)
                {
                    _logger.LogWarning("Attempted to update with a duplicate PIV Number: {PivNumber}", pivNumber);
                    // Return false to signal that the update failed due to a duplicate number.
                    return false;
                }

                // Step 2: Find the payment request to update.
                var paymentRequest = await _context.PaymentRequests.FindAsync(paymentRequestId);
                if (paymentRequest == null)
                {
                    _logger.LogWarning("Attempted to update PIV No. for non-existent Payment Request {Id}", paymentRequestId);
                    return false;
                }

                // Step 3: Update the PIV number and other relevant fields.
                paymentRequest.PIVNo = pivNumber;
                paymentRequest.UpdatedOn = DateTime.Now;
                // Add logic to set UpdatedByUserId if you have it in your model

                // Step 4: Save the changes to the database.
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating PIV No. for Payment Request {Id}", paymentRequestId);
                return false;
            }
        }

        public async Task<bool> UpdateCSNoAsync(int paymentRequestId, string csNumber)
        {
            try
            {
                // Step 1: Check if the CS number already exists for another payment request.
                // We exclude the current request ID from the check.
                var isDuplicate = await _context.PaymentRequests
                                                .AnyAsync(pr => pr.CSNo == csNumber && pr.Id != paymentRequestId);

                if (isDuplicate)
                {
                    _logger.LogWarning("Attempted to update with a duplicate CS Number: {CsNumber}", csNumber);
                    // Return false to signal that the update failed due to a duplicate number.
                    return false;
                }

                // Step 2: Find the payment request to update.
                var paymentRequest = await _context.PaymentRequests.FindAsync(paymentRequestId);
                if (paymentRequest == null)
                {
                    _logger.LogWarning("Attempted to update CS No. for non-existent Payment Request {Id}", paymentRequestId);
                    return false;
                }

                // Step 3: Update the CS number and other relevant fields.
                paymentRequest.CSNo = csNumber;
                paymentRequest.UpdatedOn = DateTime.Now;
                // Add logic to set UpdatedByUserId if you have it in your model

                // Step 4: Save the changes to the database.
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating CS No. for Payment Request {Id}", paymentRequestId);
                return false;
            }
        }
    }
}