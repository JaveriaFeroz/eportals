using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace ProcureToPay.Areas.UserManagement.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UpdateService> _logger;

        public UpdateService(ApplicationDbContext context, ILogger<UpdateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Update>> GetActiveUpdatesAsync(string userRole = null, string userId = null)
        {
            IQueryable<Update> query = _context.Updates
                .Where(u => u.IsActive);

            // Optional: Filter by user role/permissions
            if (!string.IsNullOrEmpty(userRole))
            {
                query = query.Where(u => string.IsNullOrEmpty(u.VisibleToRoles) ||
                                       u.VisibleToRoles.Contains(userRole));
            }

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(u => string.IsNullOrEmpty(u.VisibleToUsers) ||
                                       u.VisibleToUsers.Contains(userId));
            }

            query = query.OrderByDescending(u => u.Priority)
                         .ThenByDescending(u => u.CreatedDate);

            return await query.Take(5).ToListAsync(); // Limit to 5 most recent
        }
        public async Task<Update> CreateUpdateAsync(Update update)
        {
            update.CreatedDate = DateTime.Now;
            _context.Updates.Add(update);
            await _context.SaveChangesAsync();
            return update;
        }

        public async Task<Update> UpdateUpdateAsync(Update update)
        {
            _context.Updates.Update(update);
            await _context.SaveChangesAsync();
            return update;
        }

        public async Task<bool> DeleteUpdateAsync(int id)
        {
            var update = await _context.Updates.FindAsync(id);
            if (update != null)
            {
                _context.Updates.Remove(update);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<Update>> GetAllUpdatesAsync()
        {
            return await _context.Updates
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();
        }
    }
}
