using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockManagementSystem.Data;
using StockManagementSystem.Models;
using StockManagementSystem.Models.ViewModels;
using StockManagementSystem.Services;

namespace StockManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IActivityLogService _activityLogService;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IActivityLogService activityLogService)
        {
            _context = context;
            _userManager = userManager;
            _activityLogService = activityLogService;
        }

        public async Task<IActionResult> Index()
        {
            var dashboard = new DashboardViewModel();

            // Get summary statistics
            dashboard.TotalStockItems = await _context.StockItems.CountAsync();
            dashboard.LowStockItemsCount = await _context.StockItems.Where(s => s.Quantity < 5).CountAsync();
            dashboard.TotalValue = await _context.StockItems.SumAsync(s => s.Price * s.Quantity);
            dashboard.TotalCategories = await _context.Categories.Where(c => c.IsActive).CountAsync();
            dashboard.TotalUsers = await _userManager.Users.Where(u => u.IsActive).CountAsync();

            // Get category summaries
            dashboard.CategorySummaries = await _context.StockItems
                .Include(s => s.Category)
                .GroupBy(s => s.Category)
                .Select(g => new CategorySummary
                {
                    CategoryName = g.Key.Name,
                    ItemCount = g.Count(),
                    TotalValue = g.Sum(s => s.Price * s.Quantity)
                })
                .ToListAsync();

            // Get recent items
            dashboard.RecentItems = await _context.StockItems
                .Include(s => s.Category)
                .Include(s => s.CreatedByUser)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .Select(s => new StockItemViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryId = s.CategoryId,
                    CategoryName = s.Category.Name,
                    Quantity = s.Quantity,
                    Price = s.Price,
                    PurchaseDate = s.PurchaseDate,
                    Supplier = s.Supplier,
                    Description = s.Description,
                    CreatedByUserName = s.CreatedByUser.FullName
                })
                .ToListAsync();

            // Get low stock items
            dashboard.LowStockItems = await _context.StockItems
                .Include(s => s.Category)
                .Where(s => s.Quantity < 5)
                .OrderBy(s => s.Quantity)
                .Take(10)
                .Select(s => new StockItemViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryId = s.CategoryId,
                    CategoryName = s.Category.Name,
                    Quantity = s.Quantity,
                    Price = s.Price,
                    PurchaseDate = s.PurchaseDate,
                    Supplier = s.Supplier,
                    Description = s.Description
                })
                .ToListAsync();

            // Get recent activities
            dashboard.RecentActivities = await _activityLogService.GetRecentActivitiesAsync(10);

            return View(dashboard);
        }

        public async Task<IActionResult> ActivityLogs(int page = 1)
        {
            int pageSize = 20;
            var activities = await _context.ActivityLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalActivities = await _context.ActivityLogs.CountAsync();
            var totalPages = (int)Math.Ceiling(totalActivities / (double)pageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.TotalActivities = totalActivities;

            return View(activities);
        }
    }
} 