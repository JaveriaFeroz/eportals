namespace ProcureToPay.Areas.Finance.Models.DTOs
{
    public class PaymentRequestSubmission
    {
        public int FormId { get; set; }
        public string? Comments { get; set; }
        public short StateId { get; set; }
        public string? PIVNo { get; set; }
        public string? CreditNoteNo { get; set; }
        public string? CSNo { get; set; }
        public string? UserId { get; set; }
        public string? Owner { get; set; }
        public bool Completed { get; set; }
        public bool Rejected { get; set; }
        public bool Approved { get; set; }
    }
}
