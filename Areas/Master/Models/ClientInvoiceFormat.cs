using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.Master.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProcureToPay.Areas.Master.Models
{
    [Table("ClientInvoiceFormats")]
    public class ClientInvoiceFormat : AuditableEntity
    {
        [Key]
        public int DetailId { get; set; }

        [Display(Name = "Client")]
        public short ClientId { get; set; }

        [Display(Name = "Format")]
        public short FormatId { get; set; }

        // Navigation properties
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        [ForeignKey("FormatId")]
        public virtual InvoiceFormat InvoiceFormat { get; set; }
    }
}