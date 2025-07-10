
using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.ViewModels
{
    public class UoMViewModel
    {
        public short? UoMId { get; set; }

        [Required(ErrorMessage = "UoM Name is required")]
        [Display(Name = "Unit of Measure")]
        [StringLength(100)]
        public string UoMName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Audit fields for display
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class UoMListViewModel
    {
        public short UoMId { get; set; }
        public string UoMName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class UoMIndexViewModel
    {
        public List<UoMListViewModel> UoMs { get; set; } = new List<UoMListViewModel>();
        public bool ShowInactiveOnly { get; set; }
        public string SearchTerm { get; set; }
    }
}
