using System.ComponentModel.DataAnnotations;
namespace ProcureToPay.Areas.Master.ViewModels
{
    public class WorkOrderTypeViewModel
{
    public short? WorkOrderTypeId { get; set; }

    [Required(ErrorMessage = "Work Order Type Name is required")]
    [Display(Name = "Work Order Type Name")]
    [StringLength(100)]
    public string WorkOrderTypeName { get; set; }

    [Required(ErrorMessage = "Work Order Type Code is required")]
    [Display(Name = "Work Order Type Code")]
    [StringLength(20)]
    public string WorkOrderTypeCode { get; set; }

    [Required(ErrorMessage = "Workflow ID is required")]
    [Display(Name = "Workflow ID")]
    public short WorkFlowId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; } = 0;

    // Read-only audit fields for display
    [Display(Name = "Created By")]
    public string? CreatedBy { get; set; }

    [Display(Name = "Created On")]
    public DateTime? CreatedOn { get; set; }

    [Display(Name = "Updated By")]
    public string? UpdatedBy { get; set; }

    [Display(Name = "Updated On")]
    public DateTime? UpdatedOn { get; set; }
}

public class WorkOrderTypeListViewModel
{
    public short WorkOrderTypeId { get; set; }
    public string WorkOrderTypeName { get; set; }
    public string WorkOrderTypeCode { get; set; }
    public short WorkFlowId { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
}

public class WorkOrderTypeIndexViewModel
{
    public List<WorkOrderTypeListViewModel> WorkOrderTypes { get; set; } = new List<WorkOrderTypeListViewModel>();
    public bool ShowInactiveOnly { get; set; } = false;
    public string SearchTerm { get; set; }
    public short? WorkFlowId { get; set; }
    public string WorkFlowName { get; set; } // For display purposes
}

// For dropdown lists and simple selections
public class WorkOrderTypeSelectListItem
{
    public short WorkOrderTypeId { get; set; }
    public string WorkOrderTypeName { get; set; }
    public string WorkOrderTypeCode { get; set; }
    public short WorkFlowId { get; set; }
}
}
