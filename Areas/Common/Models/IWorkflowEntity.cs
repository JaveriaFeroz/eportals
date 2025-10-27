namespace ProcureToPay.Areas.Common.Models
{
    public interface IWorkflowEntity
    {
        short WorkFlowTypeId { get; }
        int FormId { get; }
        short StateId { get; set; }
        string Owner { get; set; }
        short? RequestNatureId { get; }
        short? RequestTypeId { get; }
        string DepartmentCode { get; }
        string BranchCode { get; }
        string CompanyCode { get; }

        int CurrentApprovalSequence { get; set; }
        int CreatedByUserId { get; }
        bool IsCompleted { get; set; }
        bool Approved { get; set; }
        bool Rejected { get; set; }

    }
}
