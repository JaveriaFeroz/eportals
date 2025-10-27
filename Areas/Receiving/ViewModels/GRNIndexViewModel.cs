using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.Procurement.Models;
using ProcureToPay.Areas.UserManagement.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Receiving.ViewModels
{
    public class GRNIndexViewModel
    {
        public List<GRNListItemViewModel> GRNs { get; set; } = new List<GRNListItemViewModel>();
        public int TotalCount { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        // Inside the GoodsReceiptNote class
    }

    public class GRNListItemViewModel
    {
        public int Id { get; set; }
        public int POId { get; set; }
        public string GRNNumber { get; set; } = string.Empty;
        public string PONumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int StateId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string Owner { get; set; } = string.Empty;
        public string ReceivedByUserName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string CurrentStateName { get; set; } = string.Empty;
        public int ItemsCount { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class GRNDetailsViewModel
    {
        public int Id { get; set; }
        public string GRNNumber { get; set; } = string.Empty;
        public DateTime ReceiptDate { get; set; }
        public string? Remarks { get; set; }

        // Added from the updated GoodsReceiptNote model
        public string? BillDCNo { get; set; }
        public DateTime? BillDCDate { get; set; }
        public bool? GRNVaryFromPO { get; set; }
        public double? GRNVaryRate { get; set; }
        public bool IsImported { get; set; }
        public short CurrencyId { get; set; }
        public double ExRate { get; set; }

        public decimal TotalAmount { get; set; }
        public string CurrentStateName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }

        public short StateId { get; set; }
        public int? CreatedByUserId { get; set; }
        public string? CreatedByUser { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        // Purchase Order Details
        public int PurchaseOrderId { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public DateTime PODate { get; set; }
        public decimal POTotalAmount { get; set; }

        // Supplier Details
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierContactPerson { get; set; } = string.Empty;
        public string SupplierPhone { get; set; } = string.Empty;
        public string SupplierEmail { get; set; } = string.Empty;

        // User Details
        public string ReceivedByUserName { get; set; } = string.Empty;
        public string ReceivedByUserEmail { get; set; } = string.Empty;

        // GRN Items
        public List<GRNItemDetailsViewModel> Items { get; set; } = new List<GRNItemDetailsViewModel>();

        // Workflow History
        public List<WorkflowHistoryViewModel> WorkflowHistory { get; set; } = new List<WorkflowHistoryViewModel>();
    }

    public class GRNItemDetailsViewModel
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Remarks { get; set; }
        public string? Status { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;

        // Added from the updated GoodsReceiptNoteItem model
        public double RetailPrice { get; set; }
        public bool GSTonRP { get; set; }
        public double GSTRate { get; set; }
        public decimal? GSTAmount { get; set; }
        public double DiscRate { get; set; }
        public string? Narration { get; set; }
        public decimal NetAmount { get; set; }
    }

    public class WorkflowHistoryViewModel
    {
        public string ActionName { get; set; } = string.Empty;
        public string FromStateName { get; set; } = string.Empty;
        public string ToStateName { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public string ActionByUserName { get; set; } = string.Empty;
        public string ToUserName { get; set; } = string.Empty;
        public DateTime ActionDate { get; set; }
    }

    public class CreateGRNFromPOViewModel
    {
        public List<EligiblePOViewModel> EligiblePOs { get; set; } = new List<EligiblePOViewModel>();
        public string SearchTerm { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class EligiblePOViewModel
    {
        public int Id { get; set; }
        public int prId { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public string PRNumber { get; set; } = string.Empty;
        public DateTime PODate { get; set; }

        public string Owner { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string CurrentStateName { get; set; } = string.Empty;
        public int PendingItemsCount { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public List<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }

    public class CreateGRNViewModel
    {
        public int PurchaseOrderId { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public DateTime PODate { get; set; }
        public decimal POTotalAmount { get; set; }

        // Added properties from the updated GoodsReceiptNote model
        public string? BillDCNo { get; set; }
        public DateTime? BillDCDate { get; set; }
        public bool IsImported { get; set; }
        public short CurrencyId { get; set; }
        public double ExRate { get; set; }

        [Required]
        [Display(Name = "Receipt Date")]
        public DateTime ReceiptDate { get; set; } = DateTime.Now;

        [StringLength(1000)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
        public int ReceivedByUserId { get; set; }
        public string ReceivedByUserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "At least one item must be received.")]
        public List<CreateGRNItemViewModel> ReceivedItems { get; set; } = new List<CreateGRNItemViewModel>();
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal NetAmount { get; set; }
        public decimal TotalGRNAmount { get; set; }
        public int TotalItemsCount { get; set; }
        public bool GSTonRP { get; set; }

    }

    public class    CreateGRNItemViewModel
    {
        public int PurchaseOrderItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;

        // Renamed for clarity: This is the original quantity from the PO.
        public decimal POQuantity { get; set; }
        public decimal RemainingQuantity { get; set; } // Add this line
        public decimal ReceivedQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        // Added properties from the updated GoodsReceiptNoteItem model
        public double RetailPrice { get; set; }
        public bool GSTonRP { get; set; }
        public double GSTRate { get; set; }
        public decimal? GSTAmount { get; set; }
        public double DiscRate { get; set; }


        [StringLength(500)]
        [Display(Name = "Remarks")]
        public string? Narration { get; set; }
    }
    public class GRNSummaryViewModel { /* ... */ }
    public class RecentGRNViewModel { /* ... */ }
    public class PendingReceiptViewModel { /* ... */ }
}