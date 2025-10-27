using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ProcureToPay.Areas.Procurement.Enums
{
    /// <summary>
    /// Represents the various states of a Purchase Request throughout its lifecycle
    /// Used across multiple Purchase Request types (Standard, Fleet, etc.)
    /// </summary>
    public enum PurchaseRequestState
    {
        [Display(Name = "Saved")]
        Saved = 1,

        [Display(Name = "Submitted")]
        Submitted = 2,

        [Display(Name = "Approved")]
        Approved = 3,

        [Display(Name = "Rejected")]
        Rejected = 4,

        [Display(Name = "Returned")]
        Returned = 5,

        [Display(Name = "Cancelled")]
        Cancelled = 10001
    }

    /// <summary>
    /// Purchase Nature Type - Operational Expense vs Capital Expense
    /// </summary>
    public enum PurchaseNatureType
    {
        [Display(Name = "Opex")]
        Opex = 1,

        [Display(Name = "Capex")]
        Capex = 2
    }

    /// <summary>
    /// Purchase Item Type - Goods vs Services
    /// </summary>
    public enum PurchaseItemType
    {
        [Display(Name = "Goods")]
        Goods = 1,

        [Display(Name = "Service")]
        Service = 2
    }

    /// <summary>
    /// General Request Status enum (alternative to PurchaseRequestState if needed)
    /// </summary>
    public enum RequestStatus
    {
        [Display(Name = "Draft")]
        Draft = 0,

        [Display(Name = "Submitted")]
        Submitted = 1,

        [Display(Name = "Under Review")]
        UnderReview = 2,

        [Display(Name = "Approved")]
        Approved = 3,

        [Display(Name = "Rejected")]
        Rejected = 4,

        [Display(Name = "Cancelled")]
        Cancelled = 5
    }


    /// <summary>
    /// Attachment Types for document categorization
    /// </summary>
    public enum AttachmentType
    {
        [Display(Name = "Quotation")]
        Quotation = 1,

        [Display(Name = "Specification")]
        Specification = 2,

        [Display(Name = "Budget Approval")]
        BudgetApproval = 3,

        [Display(Name = "Technical Document")]
        TechnicalDocument = 4,

        [Display(Name = "Other")]
        Other = 99
    }

    /// <summary>
    /// Priority levels for requests
    /// </summary>
    public enum RequestPriority
    {
        [Display(Name = "Low")]
        Low = 1,

        [Display(Name = "Normal")]
        Normal = 2,

        [Display(Name = "High")]
        High = 3,

        [Display(Name = "Urgent")]
        Urgent = 4,

        [Display(Name = "Critical")]
        Critical = 5
    }

    /// <summary>
    /// Helper extension methods for enum operations
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the Display Name attribute value for an enum
        /// </summary>
        /// <param name="enumValue">The enum value</param>
        /// <returns>Display name or enum string representation</returns>
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .FirstOrDefault()?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .Name ?? enumValue.ToString();
        }

        /// <summary>
        /// Gets all enum values with their display names
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <returns>Dictionary of enum values and display names</returns>
        public static Dictionary<T, string> GetEnumDisplayValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                      .Cast<T>()
                      .ToDictionary(e => e, e => e.GetDisplayName());
        }

        /// <summary>
        /// Checks if the purchase request state allows editing
        /// </summary>
        /// <param name="state">Purchase request state</param>
        /// <returns>True if editable, false otherwise</returns>
        public static bool IsEditable(this PurchaseRequestState state)
        {
            return state == PurchaseRequestState.Saved || state == PurchaseRequestState.Returned;
        }

        /// <summary>
        /// Checks if the purchase request state is final (no further changes expected)
        /// </summary>
        /// <param name="state">Purchase request state</param>
        /// <returns>True if final state, false otherwise</returns>
        public static bool IsFinalState(this PurchaseRequestState state)
        {
            return state == PurchaseRequestState.Approved ||
                   state == PurchaseRequestState.Rejected ||
                   state == PurchaseRequestState.Cancelled;
        }

        /// <summary>
        /// Gets the next possible states from current state
        /// </summary>
        /// <param name="currentState">Current purchase request state</param>
        /// <returns>List of possible next states</returns>
        public static List<PurchaseRequestState> GetNextPossibleStates(this PurchaseRequestState currentState)
        {
            return currentState switch
            {
                PurchaseRequestState.Saved => new List<PurchaseRequestState>
                {
                    PurchaseRequestState.Submitted,
                    PurchaseRequestState.Cancelled
                },
                PurchaseRequestState.Submitted => new List<PurchaseRequestState>
                {
                    PurchaseRequestState.Approved,
                    PurchaseRequestState.Rejected,
                    PurchaseRequestState.Returned
                },
                PurchaseRequestState.Returned => new List<PurchaseRequestState>
                {
                    PurchaseRequestState.Submitted,
                    PurchaseRequestState.Cancelled
                },
                _ => new List<PurchaseRequestState>() // Final states have no next states
            };
        }
    }
}
