using ProcureToPay.Areas.Receiving.Models;

namespace ProcureToPay.Areas.Finance.Models
{
    public class PaymentRequestGRN
    {
        // These two properties form the composite primary key
        public int PaymentRequestId { get; set; }
        public int GoodsReceiptNoteId { get; set; }

        // Navigation properties to the related entities
        public PaymentRequest PaymentRequest { get; set; }
        public GoodsReceiptNote GoodsReceiptNote { get; set; }
    }
}
