namespace ProcureToPay.Areas.Receiving.Models.DTOs
{
    public class CreateGRNRequest
    {
        public int PurchaseOrderId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string? Remarks { get; set; }
        public int ReceivedByUserId { get; set; }
        public List<CreateGRNItemRequest> ReceivedItems { get; set; } = new List<CreateGRNItemRequest>();

        // New fields for GoodsReceiptNote
        public string? BillDCNo { get; set; }
        public DateTime? BillDCDate { get; set; }
        public bool IsImported { get; set; }
        public short CurrencyId { get; set; }
        public double ExRate { get; set; }
    }

    public class CreateGRNItemRequest
    {
        public int PurchaseOrderItemId { get; set; }
        public decimal Quantity { get; set; }
        public string? Remarks { get; set; }

        // New fields for GoodsReceiptNoteItem
        public double RetailPrice { get; set; }
        public bool GRNVaryFromPO { get; set; }
        public bool GSTonRP { get; set; }
        public double GSTRate { get; set; }
        public decimal? GSTAmount { get; set; }

        public double DiscRate { get; set; }
        public string? Narration { get; set; }
    }
}