namespace ProcureToPay.Areas.Finance.Models.DTOs
{
    public class CreateGRNBasedPaymentRequestDto
    {
        public int GoodsReceiptNoteId { get; set; }
        public DateTime RequiredDate { get; set; } = DateTime.UtcNow.AddDays(7);
        public string? PayeeName { get; set; }
        public short SupplierId { get; set; }
        public short PaymentModeId { get; set; } = 2;
        public short? PaymentTypeId { get; set; }
        public short PaymentNatureId { get; set; } = 1;
        public short? PaymentSubNatureId { get; set; }
        public short CurrencyId { get; set; } = 1;
        public bool SelfApplicant { get; set; } = true;
        public string? PIVNo { get; set; }
        public string? CSNo { get; set; }
        public string? Remarks { get; set; }
        public short? DepartmentId { get; set; }
        public short? BranchId { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }

        public short? RequestNatureId { get; set; }
        public short? RequestTypeId { get; set; }
        // GRN payment specific fields

        public List<PaymentRequestDetailDto> Details { get; set; } = new List<PaymentRequestDetailDto>();
        public List<PaymentRequestCostAllocationDto> CostAllocations { get; set; } = new List<PaymentRequestCostAllocationDto>();
        public List<PaymentRequestAttachmentDto> Attachments { get; set; } = new List<PaymentRequestAttachmentDto>();
    }

    public class CreateDirectPaymentRequestDto
    {
        public DateTime RequiredDate { get; set; } = DateTime.UtcNow.AddDays(7);
        public string PayeeName { get; set; } = string.Empty;
        public short SupplierId { get; set; }
        public short PaymentModeId { get; set; } = 2;
        public short? PaymentTypeId { get; set; }
        public short PaymentNatureId { get; set; } = 1;
        public short? PaymentSubNatureId { get; set; }
        public short CurrencyId { get; set; } = 1;
        public bool SelfApplicant { get; set; } = true;
        public string? PIVNo { get; set; }
        public string? CSNo { get; set; }
        public string? Remarks { get; set; }
        public int CreatedByUserId { get; set; }

        // Workflow properties
        public short? RequestNatureId { get; set; }
        public short? RequestTypeId { get; set; }
        public short? DepartmentId { get; set; }
        public short? BranchId { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;

        // Details and allocations
        public List<PaymentRequestDetailDto> Details { get; set; } = new List<PaymentRequestDetailDto>();
        public List<PaymentRequestCostAllocationDto> CostAllocations { get; set; } = new List<PaymentRequestCostAllocationDto>();
        public List<PaymentRequestAttachmentDto> Attachments { get; set; } = new List<PaymentRequestAttachmentDto>();
    }

    public class PaymentRequestDetailDto
    {
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? Description { get; set; }
        public decimal AmountExTax { get; set; }
        public decimal STRate { get; set; }
        public decimal OtherTax { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PaymentRequestCostAllocationDto
    {
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string? DepartmentCode { get; set; }
        public string? DepartmentName { get; set; }
        public decimal? Rate { get; set; }
    }
    public class PaymentRequestAttachmentDto
    {
        public short AttachmentTypeId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? FileContentType { get; set; }
        public decimal FileSizeKB { get; set; }
        public byte[]? FileContent { get; set; }
    }
}
