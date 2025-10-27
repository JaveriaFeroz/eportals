using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Common.Enums
{
    /// <summary>
    /// Payment Request States based on WorkFlow States
    /// Note: States 11, 21, 22, 23 are specific to Operational Payment Nature and involve Finance team
    /// </summary>
    public enum PaymentRequestState : short
    {
        [Display(Name = "Saved")]
        Saved = 1,

        [Display(Name = "Submitted For Approval")]
        SubmittedForApproval = 2,

        [Display(Name = "Approved")]
        Approved = 3,

        [Display(Name = "Rejected")]
        Rejected = 4,

        [Display(Name = "Returned")]
        Returned = 5,

        [Display(Name = "Interfaced to Control")]
        InterfacedToControl = 6,

        [Display(Name = "Funds Disbursement")]
        FundsDisbursement = 7,

        [Display(Name = "PIV Credit Interfaced to Control")]
        PIVCreditInterfacedToControl = 8,

        // Finance team states (Only for Operational Payment Nature)
        [Display(Name = "Submitted for Verification")]
        SubmittedForVerification = 11,

        [Display(Name = "Submitted to PIV Desk")]
        SubmittedToPIVDesk = 21,

        [Display(Name = "Submitted for PIV Verification")]
        SubmittedForPIVVerification = 22,

        [Display(Name = "Returned To PIV Desk")]
        ReturnedToPIVDesk = 23,

        [Display(Name = "Approval Cancelled")]
        ApprovalCancelled = 24,

        [Display(Name = "Cheque Dispatch")]
        ChequeDispatch = 98,

        [Display(Name = "Request Completed")]
        RequestCompleted = 99,

        [Display(Name = "Cancelled")]
        Cancelled = 10001
    }

    /// <summary>
    /// Payment Nature types
    /// </summary>
    public enum PaymentNature : short
    {
        [Display(Name = "Operational Payments")]
        Operational = 1,

        [Display(Name = "Other")]
        Other = 99
    }

    /// <summary>
    /// Payment Sub Nature types
    /// </summary>
    public enum PaymentSubNature : short
    {
        [Display(Name = "PO/WO")]
        POWO = 1,

        [Display(Name = "Others")]
        Others = 99,
    }

    /// <summary>
    /// Payment Type
    /// </summary>
    public enum PaymentType : short
    {
        [Display(Name = "Advance")]
        Advance = 1,

        [Display(Name = "Expense")]
        Expense = 99,
    }

    /// <summary>
    /// Payment Mode types
    /// </summary>
    public enum PaymentMode : short
    {
        [Display(Name = "Cash")]
        Cash = 1,

        [Display(Name = "Cheque")]
        Cheque = 2,

        [Display(Name = "Bank Transfer")]
        BankTransfer = 3,

        [Display(Name = "Online Transfer")]
        OnlineTransfer = 4,

        [Display(Name = "Credit Card")]
        CreditCard = 5
    }

    /// <summary>
    /// Currency types
    /// </summary>
    public enum Currency : short
    {
        [Display(Name = "PKR")]
        PKR = 1,

        [Display(Name = "USD")]
        USD = 2,
    }
}