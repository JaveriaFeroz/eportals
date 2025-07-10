using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProcureToPay.Areas.Master.Models
{
    [Table("InvoiceFormats")]
    public class InvoiceFormat
    {
        [Key]
        public short FormatId { get; set; }

        [Required]
        [StringLength(100)]
        public string FormatName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}