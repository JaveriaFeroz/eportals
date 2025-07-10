namespace ProcureToPay.Areas.UserManagement.Models
{
    public class Update
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Priority { get; set; } // Higher number = higher priority
        public string CreatedBy { get; set; }

        // Optional: Role-based visibility
        public string VisibleToRoles { get; set; } = string.Empty; 
        public string VisibleToUsers { get; set; } = string.Empty;
    }
}
