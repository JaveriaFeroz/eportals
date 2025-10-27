namespace ProcureToPay.ViewModels
{
    public class AllRequestsViewModel
    {
        public List<ProcureToPay.Areas.Procurement.Models.PurchaseRequest> PurchaseRequests { get; set; }
        public List<ProcureToPay.Areas.Procurement.Models.PurchaseOrder> PurchaseOrders { get; set; }
        public List<ProcureToPay.Areas.Finance.Models.PaymentRequest> PaymentRequests { get; set; }

        public int? SelectedModuleId { get; set; }
        public int? SelectedStatusId { get; set; }
        public string Title { get; set; }
    }

    // This is the key change. Make the base class abstract.
    public abstract class BaseRequestViewModel
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CurrentOwner { get; set; } = string.Empty;
        public int WorkFlowTypeId { get; set; }
    }

    // And add the concrete class that inherits from it.
    public class RequestDetailViewModel : BaseRequestViewModel
    {
    }
}