using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Procurement.Enums
{
    public enum PurchaseOrderStatus
    {
        [Display(Name = "Draft")]
        Draft = 0,

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

        [Display(Name = "Issued")]
        Issued = 6,

        [Display(Name = "Received")]
        Received = 7,

        [Display(Name = "Closed")]
        Closed = 8,

        [Display(Name = "Partially Received")]
        PartiallyReceived = 9,

        [Display(Name = "Cancelled")]
        Cancelled = 10001
    }
}