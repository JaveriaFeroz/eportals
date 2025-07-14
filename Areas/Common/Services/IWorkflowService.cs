using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Data;
using ProcureToPay.Areas.Inventory.Models; // Add this using directive

namespace ProcureToPay.Areas.Common.Services
{
    public interface IWorkflowService
    {
        Task<User> FindNextApproverAsync(IWorkflowEntity entity); // This will be refined to find the *next* in sequence
        Task<User> FindApproverForSequenceAsync(IWorkflowEntity entity, int approvalSeq); // New helper to find approver for a specific seq
        Task<string> GetStateNameAsync(short workFlowTypeId, short stateId);
        Task<List<WorkFlowState>> GetWorkflowStatesAsync(short workFlowTypeId);
        Task<FormHistory> AddFormHistoryAsync(short workFlowTypeId, int formId, short fromStateId, short toStateId, string action, string comments, int actionByUserId);
        Task<List<FormHistory>> GetFormHistoryAsync(short workFlowTypeId, int formId);
        Task<bool> CanUserPerformActionAsync(int userId, IWorkflowEntity entity, string action);
        Task<(bool Success, string Message, User NextApprover)> ProcessWorkflowActionAsync(IWorkflowEntity entity, string action, string comments, int actionByUserId);
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WorkflowService> _logger;

        // Define workflow states as constants
        public const short STATE_SAVED = 1;
        public const short STATE_SUBMITTED = 2; // Now means "In Approval"
        public const short STATE_APPROVED = 3;  // Fully approved
        public const short STATE_REJECTED = 4;
        public const short STATE_RETURNED = 5;
        public const short STATE_CANCELLED = 10001; // Example for a cancel state
        public const short STATE_COMPLETED = 99; // More generic completed state, distinct from approved if needed

        public WorkflowService(ApplicationDbContext context, ILogger<WorkflowService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Finds the next approver in the sequence for the given workflow entity.
        /// This assumes the entity (e.g., PurchaseRequisition) has a CurrentApprovalSequence property.
        /// </summary>
        /// <param name="entity">The workflow entity.</param>
        /// <returns>The next User to approve, or null if no next approver is found.</returns>
        public async Task<User> FindNextApproverAsync(IWorkflowEntity entity)
        {
            // IMPORTANT: Cast to PurchaseRequisition to access CurrentApprovalSequence
            if (!(entity is PurchaseRequisition pr))
            {
                _logger.LogWarning("FindNextApproverAsync called with non-PurchaseRequisition entity for sequential approval.");
                return null; // Or handle other workflow entity types differently
            }

            int nextSeq = pr.CurrentApprovalSequence + 1;

            return await FindApproverForSequenceAsync(entity, nextSeq);
        }

        /// <summary>
        /// Finds the approver for a specific approval sequence number based on entity criteria.
        /// </summary>
        /// <param name="entity">The workflow entity.</param>
        /// <param name="approvalSeq">The target approval sequence number.</param>
        /// <returns>The User for the specified sequence, or null if not found.</returns>
        public async Task<User> FindApproverForSequenceAsync(IWorkflowEntity entity, int approvalSeq)
        {
            try
            {
                var approvalSequence = await _context.WorkFlowApprovalSequences
                    .Include(ws => ws.Role)
                    .ThenInclude(r => r.UserRoles)
                    .ThenInclude(ur => ur.User)
                    .Where(ws => ws.WorkFlowTypeId == entity.WorkFlowTypeId &&
                                 ws.IsActive &&
                                 ws.ApprovalSeq == approvalSeq && // Targeting a specific sequence number
                                 (ws.RequestNatureId == null || ws.RequestNatureId == entity.RequestNatureId) &&
                                 (ws.RequestTypeId == null || ws.RequestTypeId == entity.RequestTypeId) &&
                                 (ws.DepartmentCode == null || ws.DepartmentCode == entity.DepartmentCode) &&
                                 (ws.BranchCode == null || ws.BranchCode == entity.BranchCode) &&
                                 (ws.CompanyCode == null || ws.CompanyCode == entity.CompanyCode) &&
                                 (ws.MinAmount == null || entity.TotalAmount >= ws.MinAmount) &&
                                 (ws.MaxAmount == null || entity.TotalAmount <= ws.MaxAmount))
                    .OrderBy(ws => ws.ApprovalSeq) // Ordering ensures consistency if multiple match (though ApprovalSeq should make it unique)
                    .FirstOrDefaultAsync();

                if (approvalSequence?.Role?.UserRoles?.Any() == true)
                {
                    // Get the first active user in the role
                    var approver = approvalSequence.Role.UserRoles
                        .FirstOrDefault(ur => ur.User.IsActive)?.User;

                    return approver;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding approver for WorkFlowTypeId: {WorkFlowTypeId}, FormId: {FormId}, Seq: {ApprovalSeq}",
                    entity.WorkFlowTypeId, entity.FormId, approvalSeq);
                return null;
            }
        }

        public async Task<string> GetStateNameAsync(short workFlowTypeId, short stateId)
        {
            var state = await _context.WorkFlowStates
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.WorkFlowTypeId == workFlowTypeId && s.StateId == stateId);

            return state?.StateName ?? "Unknown";
        }

        public async Task<List<WorkFlowState>> GetWorkflowStatesAsync(short workFlowTypeId)
        {
            return await _context.WorkFlowStates
                .AsNoTracking()
                .Where(s => s.WorkFlowTypeId == workFlowTypeId)
                .OrderBy(s => s.StateId)
                .ToListAsync();
        }

        public async Task<FormHistory> AddFormHistoryAsync(short workFlowTypeId, int formId, short fromStateId, short toStateId, string action, string comments, int actionByUserId)
        {
            var user = await _context.Users.FindAsync(actionByUserId);

            var history = new FormHistory
            {
                WorkFlowTypeId = workFlowTypeId,
                FormId = formId,
                FromStateId = fromStateId,
                FromStateName = await GetStateNameAsync(workFlowTypeId, fromStateId), // Store name for easier auditing
                ToStateId = toStateId,
                ToStateName = await GetStateNameAsync(workFlowTypeId, toStateId),     // Store name for easier auditing
                Action = action,
                Comments = comments,
                ActionByUserId = actionByUserId,
                ActionByUserName = user?.UserName ?? "Unknown User", // Handle null user more gracefully
                ActionDate = DateTime.UtcNow
            };

            _context.FormHistories.Add(history);
            // SaveChanges will be called by the controller after the main entity update
            // await _context.SaveChangesAsync(); 

            return history;
        }

        public async Task<List<FormHistory>> GetFormHistoryAsync(short workFlowTypeId, int formId)
        {
            return await _context.FormHistories
                .AsNoTracking()
                .Where(h => h.WorkFlowTypeId == workFlowTypeId && h.FormId == formId)
                .OrderBy(h => h.ActionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Determines if a user can perform a specific action on a workflow entity.
        /// </summary>
        public async Task<bool> CanUserPerformActionAsync(int userId, IWorkflowEntity entity, string action)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || !user.IsActive) return false;

            // IMPORTANT: Cast to PurchaseRequisition to access CreatedByUserId and CurrentApprovalSequence
            if (!(entity is PurchaseRequisition pr))
            {
                _logger.LogWarning("CanUserPerformActionAsync called with non-PurchaseRequisition entity.");
                return false; // Or handle other workflow entity types differently
            }

            bool isCreator = userId == pr.CreatedByUserId;

            switch (action.ToLower())
            {
                case "submit":
                    // Only the creator can submit, and only if in Saved or Returned state
                    return (pr.StateId == STATE_SAVED || pr.StateId == STATE_RETURNED) && isCreator;

                case "approve":
                case "reject":
                case "return":
                    // Must be in submitted state and the current user must be the expected approver
                    if (pr.StateId != STATE_SUBMITTED) return false;

                    // Find the role required for the current approval sequence
                    var currentApprovalSequence = await _context.WorkFlowApprovalSequences
                        .AsNoTracking()
                        .Where(ws => ws.WorkFlowTypeId == pr.WorkFlowTypeId &&
                                     ws.IsActive &&
                                     ws.ApprovalSeq == pr.CurrentApprovalSequence && // Match the entity's current sequence
                                     (ws.RequestNatureId == null || ws.RequestNatureId == pr.RequestNatureId) &&
                                     (ws.RequestTypeId == null || ws.RequestTypeId == pr.RequestTypeId) &&
                                     (ws.DepartmentCode == null || ws.DepartmentCode == pr.DepartmentCode) &&
                                     (ws.BranchCode == null || ws.BranchCode == pr.BranchCode) &&
                                     (ws.CompanyCode == null || ws.CompanyCode == pr.CompanyCode) &&
                                     (ws.MinAmount == null || pr.TotalAmount >= ws.MinAmount) &&
                                     (ws.MaxAmount == null || pr.TotalAmount <= ws.MaxAmount))
                        .Select(ws => ws.RoleID)
                        .FirstOrDefaultAsync();

                    if (currentApprovalSequence == 0) // No matching approval sequence found for the current step
                        return false;

                    // Check if the current user has the required role
                    return user.UserRoles.Any(ur => ur.RoleId == currentApprovalSequence);

                case "cancel":
                    // Only creator can cancel, and only if not already approved/rejected/completed
                    return (pr.StateId != STATE_APPROVED && pr.StateId != STATE_REJECTED && pr.StateId != STATE_COMPLETED) && isCreator;

                default:
                    return false;
            }
        }


        /// <summary>
        /// Processes a workflow action (submit, approve, reject, return) for a given entity.
        /// </summary>
        /// <param name="entity">The workflow entity (e.g., PurchaseRequisition).</param>
        /// <param name="action">The action to perform (e.g., "submit", "approve").</param>
        /// <param name="comments">Comments for the action.</param>
        /// <param name="actionByUserId">The ID of the user performing the action.</param>
        /// <returns>A tuple indicating success, a message, and the next approver (if applicable).</returns>
        public async Task<(bool Success, string Message, User NextApprover)> ProcessWorkflowActionAsync(IWorkflowEntity entity, string action, string comments, int actionByUserId)
        {
            try
            {
                // IMPORTANT: Cast to PurchaseRequisition here since this service is specifically for PR workflow logic
                if (!(entity is PurchaseRequisition pr))
                {
                    return (false, "Invalid entity type for Purchase Requisition workflow.", null);
                }

                short fromStateId = pr.StateId;
                short toStateId = fromStateId; // Default to current state, change below
                User nextApprover = null;

                switch (action.ToLower())
                {
                    case "submit":
                        if (pr.StateId != STATE_SAVED && pr.StateId != STATE_RETURNED)
                        {
                            return (false, "Purchase Requisition can only be submitted from 'Saved' or 'Returned' state.", null);
                        }

                        // Set initial approval sequence to 0 before finding the first approver (for seq 1)
                        pr.CurrentApprovalSequence = 0;
                        nextApprover = await FindNextApproverAsync(pr); // Find approver for sequence 1

                        if (nextApprover == null)
                        {
                            return (false, "No initial approver found for this Purchase Requisition based on configured workflow sequences.", null);
                        }

                        toStateId = STATE_SUBMITTED;
                        pr.StateId = toStateId;
                        pr.Owner = nextApprover.UserName;
                        pr.CurrentApprovalSequence = 1; // Set to the first approval sequence
                        pr.IsCompleted = false; // Ensure it's not marked completed
                        pr.Approved = false;
                        pr.Rejected = false;
                        break;

                    case "approve":
                        if (pr.StateId != STATE_SUBMITTED)
                        {
                            return (false, "Purchase Requisition can only be approved from 'Submitted' state.", null);
                        }

                        // Increment current approval sequence and try to find next approver
                        pr.CurrentApprovalSequence++;
                        nextApprover = await FindApproverForSequenceAsync(pr, pr.CurrentApprovalSequence);

                        if (nextApprover != null)
                        {
                            // Still more approvals in the sequence
                            toStateId = STATE_SUBMITTED; // Remains in "Submitted" state, but now owned by the next approver
                            pr.StateId = toStateId;
                            pr.Owner = nextApprover.UserName;
                        }
                        else
                        {
                            // No more approvers in the sequence, so it's fully APPROVED
                            toStateId = STATE_APPROVED;
                            pr.StateId = toStateId;
                            pr.Owner = "System"; // Or null, or original creator, indicating no pending approval
                            pr.IsCompleted = true;
                            pr.Approved = true;
                            pr.Rejected = false;
                        }
                        break;

                    case "reject":
                        if (pr.StateId != STATE_SUBMITTED)
                        {
                            return (false, "Purchase Requisition can only be rejected from 'Submitted' state.", null);
                        }
                        toStateId = STATE_REJECTED;
                        pr.StateId = toStateId;
                        pr.IsCompleted = true;
                        pr.Rejected = true;
                        pr.Approved = false;
                        pr.CurrentApprovalSequence = 0; // Reset sequence on rejection
                        // Set owner back to creator for review
                        var creatorUser = await _context.Users.FindAsync(pr.CreatedByUserId);
                        pr.Owner = creatorUser?.UserName ?? "Unknown Creator";
                        break;

                    case "return":
                        if (pr.StateId != STATE_SUBMITTED)
                        {
                            return (false, "Purchase Requisition can only be returned from 'Submitted' state.", null);
                        }
                        toStateId = STATE_RETURNED;
                        pr.StateId = toStateId;
                        pr.IsCompleted = false; // Can be resubmitted
                        pr.Approved = false;
                        pr.Rejected = false;
                        pr.CurrentApprovalSequence = 0; // Reset sequence on return
                        // Set owner back to creator for rework
                        var creatorUserForReturn = await _context.Users.FindAsync(pr.CreatedByUserId);
                        pr.Owner = creatorUserForReturn?.UserName ?? "Unknown Creator";
                        break;

                    case "cancel":
                        if (pr.StateId == STATE_APPROVED || pr.StateId == STATE_REJECTED || pr.StateId == STATE_COMPLETED)
                        {
                            return (false, "An already approved, rejected, or completed Purchase Requisition cannot be cancelled.", null);
                        }
                        toStateId = STATE_CANCELLED;
                        pr.StateId = toStateId;
                        pr.IsCompleted = true; // Mark as completed (cancelled)
                        pr.Approved = false;
                        pr.Rejected = false;
                        pr.CurrentApprovalSequence = 0; // Reset sequence on cancellation
                        pr.Owner = "System"; // Or null
                        break;

                    default:
                        return (false, "Invalid workflow action specified.", null);
                }

                // Add history entry (Note: _context.SaveChangesAsync() is expected to be called by the controller)
                await AddFormHistoryAsync(pr.WorkFlowTypeId, pr.PRNo, fromStateId, toStateId, action, comments, actionByUserId);

                return (true, $"Purchase Requisition {action}ed successfully.", nextApprover);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing workflow action: {Action} for WorkFlowTypeId: {WorkFlowTypeId}, FormId: {FormId}",
                    action, entity.WorkFlowTypeId, entity.FormId);
                return (false, "An unexpected error occurred while processing the workflow action.", null);
            }
        }
    }
}