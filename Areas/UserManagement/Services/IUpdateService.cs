using ProcureToPay.Areas.UserManagement.Models;

namespace ProcureToPay.Areas.UserManagement.Services
{
    public interface IUpdateService
    {
        Task<List<Update>> GetActiveUpdatesAsync(string userRole = null, string userId = null);
        Task<Update> CreateUpdateAsync(Update update);
        Task<Update> UpdateUpdateAsync(Update update);
        Task<bool> DeleteUpdateAsync(int id);
        Task<List<Update>> GetAllUpdatesAsync();
    }
}
