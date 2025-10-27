using ProcureToPay.Areas.UserManagement.Models;

namespace ProcureToPay.Areas.Auth.Models
{
    public class AuthViewModelBase
    {
        public List<Update> Updates { get; set; } = new List<Update>();

    }
}
