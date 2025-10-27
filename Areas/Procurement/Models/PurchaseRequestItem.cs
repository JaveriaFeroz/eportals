using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.Master.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Required for IValidatableObject
using System; // For Math.Round

namespace ProcureToPay.Areas.Procurement.Models
{
    [Table("PurchaseRequestDetails")]
    public class PurchaseRequestItem : AuditableEntity, IValidatableObject // Implemented IValidatableObject
    {
        [Key]
        [Column("DetailId")]
        public int DetailId { get; set; }

        [Column("PRNo")]
        public int PurchaseRequestId { get; set; }

        [ForeignKey("PurchaseRequestId")]
        public virtual PurchaseRequest? PurchaseRequest { get; set; }


        // --- PRODUCT / SERVICE SELECTION ---
        [Display(Name = "Product")]
        public short? ProductId { get; set; } // Nullable

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; } // Nullable nav prop

        [Display(Name = "Service")]
        public short? ServiceId { get; set; } // Nullable

        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; } // Nullable nav prop


        // --- ITEM DETAILS (Core data to be stored in DB) ---
        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        [Display(Name = "Remarks")]
        public string Narration { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.01, (double)99999999999.99, ErrorMessage = "Quantity must be greater than 0.")]
        [Column(TypeName = "decimal(18,4)")] // Specify precision for DB column
        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }

        [Display(Name = "Unit of Measure")]
        public short? UoMId { get; set; }

        [ForeignKey("UoMId")]
        public virtual UoM? UoM { get; set; }

        [StringLength(200, ErrorMessage = "Item Name cannot exceed 200 characters.")]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Unit Price is required.")]
        [Column(TypeName = "decimal(18,2)")] // Store with 2 decimal places for currency
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "GST Rate (%)")]
        [Column(TypeName = "decimal(5,2)")] // e.g., 99.99
        [Range(0.00, 100.00, ErrorMessage = "GST Rate must be between 0 and 100.")]
        public decimal GSTRate { get; set; } = 0M;

        [Display(Name = "VAT Rate (%)")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0.00, 100.00, ErrorMessage = "VAT Rate must be between 0 and 100.")]
        public decimal VATRate { get; set; } = 0M;


        // --- CALCULATED PROPERTIES (WILL BE MAPPED TO DB COLUMNS) ---
        // These now have get; set; which tells EF Core to map them.
        // You MUST populate these values in your C# code before saving.

        [Display(Name = "Gross Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // Will be mapped as a regular column

        [Display(Name = "GST Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GSTAmount { get; set; } // Will be mapped as a regular column

        [Display(Name = "VAT Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal VATAmount { get; set; } // Will be mapped as a regular column

        [Display(Name = "Total Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }


        // --- OTHER PROPERTIES ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderedQuantity { get; set; } = 0M;

        public void CalculateAmounts()
        {
            // Calculate Gross Amount
            Amount = Math.Round(Quantity * UnitPrice, 2);

            // Calculate GST Amount
            // Ensure division by 100M for percentage
            GSTAmount = Math.Round(Amount * (GSTRate / 100M), 2);

            // Calculate VAT Amount
            VATAmount = Math.Round(Amount * (VATRate / 100M), 2);

            // Calculate Total Amount
            TotalAmount = Math.Round(Amount + GSTAmount + VATAmount, 2);
        }
        // --- IValidatableObject Implementation for Conditional Validation ---
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Scenario 1: Cannot select both Product and Service for the same item.
            if (ProductId.HasValue && ProductId > 0 && ServiceId.HasValue && ServiceId > 0)
            {
                yield return new ValidationResult(
                    "Cannot select both a Product and a Service for the same item.",
                    new[] { nameof(ProductId), nameof(ServiceId) }
                );
            }
            // New Logic: A Product or a Service must be selected. Remarks are no longer a fallback option.
            else if ((!ProductId.HasValue || ProductId <= 0) && (!ServiceId.HasValue || ServiceId <= 0))
            {
                yield return new ValidationResult(
                    "Either a Product or a Service must be provided for each item.",
                    new[] { nameof(ProductId), nameof(ServiceId) }
                );
            }
            // If a product or service is selected, no further validation is needed here.
        }
    }
}