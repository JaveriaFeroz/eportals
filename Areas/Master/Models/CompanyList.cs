using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProcureToPay.Areas.Master.Models
{
    [Table("Companies")]
    public class CompanyList
    {
        [Key]
        public short CompanyId { get; set; }
        public string CompanyName { get; set; }
        public bool IsActive { get; set; }
    }
}