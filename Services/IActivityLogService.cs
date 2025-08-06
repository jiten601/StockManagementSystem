using StockManagementSystem.Models;

namespace StockManagementSystem.Services
{
    public interface IActivityLogService
    {
        Task LogActivityAsync(string userId, string action, string entityType, int? entityId = null, string? description = null, string? ipAddress = null);
        Task<List<ActivityLog>> GetRecentActivitiesAsync(int count = 10);
        Task<List<ActivityLog>> GetUserActivitiesAsync(string userId, int count = 50);
    }
} 