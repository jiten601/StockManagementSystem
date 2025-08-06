using System.ComponentModel.DataAnnotations;

namespace StockManagementSystem.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // Create, Update, Delete, Login, etc.
        
        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty; // StockItem, Category, User, etc.
        
        public int? EntityId { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        [StringLength(45)]
        public string? IpAddress { get; set; }
        
        // Navigation property
        public virtual ApplicationUser User { get; set; } = null!;
        public string UserName { get; set; }
    }
} 