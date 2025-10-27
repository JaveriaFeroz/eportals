using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Procurement.ViewModels
{
    public class BidEvaluationCreateViewModel
    {
        [Required]
        public int PRNo { get; set; }

        public string PurchaseRequestNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        [Display(Name = "Justification")]
        public string Justification { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Evaluation Date")]
        public DateTime EvaluationDate { get; set; } = DateTime.Now;

        [Display(Name = "Submission Deadline")]
        public DateTime? SubmissionDeadline { get; set; }

        [Required]
        [Display(Name = "Estimated Amount")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Estimated Amount must be greater than 0")]
        public decimal EstimatedAmount { get; set; }

        public List<PurchaseRequestItemDto> PurchaseRequestItems { get; set; } = new List<PurchaseRequestItemDto>();
    }

    public class PurchaseRequestItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public string UoMName { get; set; } = string.Empty;
    }

    public class BidCreateViewModel
    {
        [Required]
        public int BidNo { get; set; }

        public string BidEvaluationNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Supplier")]
        public short SupplierId { get; set; }

        [StringLength(100)]
        [Display(Name = "Quotation Number")]
        public string? QuotationNumber { get; set; }

        [Required]
        [Display(Name = "Submission Date")]
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        [Display(Name = "Tax Amount")]
        [Range(0, double.MaxValue)]
        public decimal TaxAmount { get; set; }

        [Required]
        [Display(Name = "Delivery Days")]
        [Range(1, 365)]
        public int DeliveryDays { get; set; } = 30;

        [StringLength(200)]
        [Display(Name = "Payment Terms")]
        public string? PaymentTerms { get; set; } = "Net 30";

        [StringLength(50)]
        [Display(Name = "Validity Period")]
        public string? ValidityPeriod { get; set; } = "30 Days";

        [Display(Name = "Meets Requirements")]
        public bool MeetsRequirements { get; set; } = true;

        [Display(Name = "Is Responsive")]
        public bool IsResponsive { get; set; } = true;

        [StringLength(1000)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public List<BidItemViewModel> BidItems { get; set; } = new List<BidItemViewModel>();
    }

    public class BidItemViewModel
    {
        public int? PurchaseRequestItemId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Specifications")]
        public string? Specifications { get; set; }

        [Required]
        [Display(Name = "Quantity")]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Display(Name = "Unit of Measure")]
        public string UOM { get; set; } = string.Empty;

        public short? UoMId { get; set; }

        [Required]
        [Display(Name = "Unit Price")]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Total Price")]
        public decimal TotalPrice { get; set; }

        [StringLength(500)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Meets Specifications")]
        public bool MeetsSpecifications { get; set; } = true;
    }


    public class BidComparisonViewModel
    {
        public int BidEvaluationId { get; set; }
        public string BidEvaluationNumber { get; set; } = string.Empty;
        public List<BidSummary> Bids { get; set; } = new List<BidSummary>();
        public List<string> ItemNames { get; set; } = new List<string>();
        public int? SelectedBidId { get; set; }
        public string? SelectionJustification { get; set; }


        public class BidSummary
        {
            public int BidId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public string? QuotationNumber { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal TaxAmount { get; set; }
            public decimal GrandTotal { get; set; }
            public int DeliveryDays { get; set; }
            public bool MeetsRequirements { get; set; }
            public bool IsResponsive { get; set; }
            public int Rank { get; set; }  // Changed from int? to int
            public List<BidItemDetail> Items { get; set; } = new List<BidItemDetail>();
        }

        public class BidItemDetail
        {
            public string ItemName { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
            public bool MeetsSpecifications { get; set; }
        }
    }
}