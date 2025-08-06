namespace StockManagementSystem.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalStockItems { get; set; }
        public int LowStockItemsCount { get; set; }
        public decimal TotalValue { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
        
        public List<CategorySummary> CategorySummaries { get; set; } = new();
        public List<StockItemViewModel> RecentItems { get; set; } = new();
        public List<StockItemViewModel> LowStockItems { get; set; } = new();
        public List<ActivityLog> RecentActivities { get; set; } = new();
    }

    public class CategorySummary
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal TotalValue { get; set; }
    }
} 