using ProcureToPay.Areas.Procurement.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Procurement.Models.DTOs
{
    public class CreatePOFromPRRequest
    {
        [Required]
        public int PurchaseRequestId { get; set; } // The ID of the PR this PO is being created from

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Expected Delivery Date")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }

        [Required(ErrorMessage = "Vendor is required")]
        public int VendorId { get; set; } // Assuming this maps to Supplier.Id

        [StringLength(1000)]
        public string? VendorAddress { get; set; } // Matches PurchaseOrder model's SupplierAddress

        [StringLength(50)]
        public string? VendorContact { get; set; } // Matches PurchaseOrder model's SupplierContact

        [Required]
        public PurchaseNatureType PurchaseNature { get; set; }

        [Required]
        public PurchaseItemType PurchaseType { get; set; }

        [StringLength(1000)]
        public string? Terms { get; set; }

        public short? PaymentDays { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Required]
        public decimal MappedAmount { get; set; } // The total amount from the PR being covered by this PO

        [StringLength(500)]
        public string? MappingNotes { get; set; }

        public List<CreatePOFromPRItemRequest> SelectedItems { get; set; } = new List<CreatePOFromPRItemRequest>();
    }

    
}