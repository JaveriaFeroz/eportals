using ProcureToPay.Areas.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("WorkOrderTypes")]
public class WorkOrderType : AuditableEntity
{
    [Key]
    public short WorkOrderTypeId { get; set; }

    [Required(ErrorMessage = "Work Order Type Name is required")]
    [Display(Name = "Work Order Type Name")]
    [StringLength(100)]
    public string WorkOrderTypeName { get; set; }

    [Required(ErrorMessage = "Work Order Type Code is required")]
    [Display(Name = "Work Order Type Code")]
    [StringLength(20)]
    public string WorkOrderTypeCode { get; set; }

    [Required]
    [Display(Name = "Workflow ID")]
    public short WorkFlowId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; } = 0;

    // Navigation property (for WorkFlow entity)
    // [ForeignKey("WorkFlowId")]
    // public virtual WorkFlow WorkFlow { get; set; }
}