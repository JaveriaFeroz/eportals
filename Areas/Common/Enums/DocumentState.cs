namespace ProcureToPay.Areas.Common.Enums
{
    public enum DocumentState : short
    {
        Draft = 1,
        Submitted = 2,
        Approved = 3,
        Rejected = 4,
        Returned = 5,
        Cancelled = 10001
    }
}