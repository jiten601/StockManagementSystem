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
    [Authorize]
    public class StockController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IActivityLogService _activityLogService;

        public StockController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IActivityLogService activityLogService)
        {
            _context = context;
            _userManager = userManager;
            _activityLogService = activityLogService;
        }

        public async Task<IActionResult> Index(string? searchTerm, int? categoryId, string? sortOrder, int page = 1)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["QuantitySortParm"] = sortOrder == "Quantity" ? "quantity_desc" : "Quantity";
            ViewData["CurrentFilter"] = searchTerm;
            ViewData["CurrentCategory"] = categoryId;

            var query = _context.StockItems
                .Include(s => s.Category)
                .Include(s => s.CreatedByUser)
                .AsQueryable();

            // Apply search filter
            if (!String.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.Name.Contains(searchTerm) || 
                                       s.Supplier.Contains(searchTerm) || 
                                       s.Category.Name.Contains(searchTerm));
            }

            // Apply category filter
            if (categoryId.HasValue)
            {
                query = query.Where(s => s.CategoryId == categoryId);
            }

            // Apply sorting
            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(s => s.Name),
                "Date" => query.OrderBy(s => s.PurchaseDate),
                "date_desc" => query.OrderByDescending(s => s.PurchaseDate),
                "Quantity" => query.OrderBy(s => s.Quantity),
                "quantity_desc" => query.OrderByDescending(s => s.Quantity),
                _ => query.OrderBy(s => s.Name),
            };

            // Pagination
            int pageSize = 10;
            var stockItems = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.TotalItems = totalItems;

            // Get categories for filter dropdown
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();

            return View(stockItems);
        }

        public async Task<IActionResult> Details(int id)
        {
            var stockItem = await _context.StockItems
                .Include(s => s.Category)
                .Include(s => s.CreatedByUser)
                .Include(s => s.UpdatedByUser)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stockItem == null)
            {
                return NotFound();
            }

            return View(stockItem);
        }

        [HttpGet]
        public async Task<IActionResult> Buy(int id)
        {
            var item = await _context.StockItems.FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
            if (item == null)
            {
                return NotFound();
            }

            var model = new BuyItemViewModel
            {
                StockItemId = item.Id,
                ItemName = item.Name,
                UnitPrice = item.Price,
                AvailableQuantity = item.Quantity,
                QuantityToBuy = 1
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(BuyItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var item = await _context.StockItems.FirstOrDefaultAsync(s => s.Id == model.StockItemId);
            if (item == null)
            {
                return NotFound();
            }

            if (model.QuantityToBuy <= 0)
            {
                ModelState.AddModelError(nameof(model.QuantityToBuy), "Quantity must be at least 1.");
                return View(model);
            }

            if (model.QuantityToBuy > item.Quantity)
            {
                ModelState.AddModelError(nameof(model.QuantityToBuy), "Quantity exceeds available stock.");
                model.AvailableQuantity = item.Quantity;
                model.ItemName = item.Name;
                model.UnitPrice = item.Price;
                return View(model);
            }

            // Decrease quantity
            item.Quantity -= model.QuantityToBuy;
            item.UpdatedAt = DateTime.Now;
            var user = await _userManager.GetUserAsync(User);
            item.UpdatedBy = user?.Id;
            await _context.SaveChangesAsync();

            await _activityLogService.LogActivityAsync(
                user!.Id, "Buy", "StockItem", item.Id,
                $"Bought {model.QuantityToBuy} x {item.Name} (remaining {item.Quantity})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = $"You purchased {model.QuantityToBuy} unit(s) of {item.Name}.";
            return RedirectToAction(nameof(Details), new { id = item.Id });
        }

        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create(StockItemViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var stockItem = new StockItem
                {
                    Name = model.Name,
                    CategoryId = model.CategoryId,
                    Quantity = model.Quantity,
                    Price = model.Price,
                    PurchaseDate = model.PurchaseDate,
                    Supplier = model.Supplier,
                    Description = model.Description,
                    Location = model.Location,
                    IsActive = model.IsActive,
                    CreatedBy = user!.Id
                };

                _context.Add(stockItem);
                await _context.SaveChangesAsync();

                await _activityLogService.LogActivityAsync(
                    user.Id, "Create", "StockItem", stockItem.Id, 
                    $"Created stock item: {stockItem.Name}", 
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["Success"] = "Stock item created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var stockItem = await _context.StockItems.FindAsync(id);
            if (stockItem == null)
            {
                return NotFound();
            }

            var model = new StockItemViewModel
            {
                Id = stockItem.Id,
                Name = stockItem.Name,
                CategoryId = stockItem.CategoryId,
                Quantity = stockItem.Quantity,
                Price = stockItem.Price,
                PurchaseDate = stockItem.PurchaseDate,
                Supplier = stockItem.Supplier,
                Description = stockItem.Description,
                Location = stockItem.Location,
                IsActive = stockItem.IsActive
            };

            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, StockItemViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var stockItem = await _context.StockItems.FindAsync(id);
                if (stockItem == null)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);
                var oldName = stockItem.Name;

                stockItem.Name = model.Name;
                stockItem.CategoryId = model.CategoryId;
                stockItem.Quantity = model.Quantity;
                stockItem.Price = model.Price;
                stockItem.PurchaseDate = model.PurchaseDate;
                stockItem.Supplier = model.Supplier;
                stockItem.Description = model.Description;
                stockItem.Location = model.Location;
                stockItem.IsActive = model.IsActive;
                stockItem.UpdatedBy = user!.Id;
                stockItem.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                await _activityLogService.LogActivityAsync(
                    user.Id, "Update", "StockItem", stockItem.Id, 
                    $"Updated stock item: {oldName} -> {stockItem.Name}", 
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["Success"] = "Stock item updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var stockItem = await _context.StockItems
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stockItem == null)
            {
                return NotFound();
            }

            return View(stockItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stockItem = await _context.StockItems.FindAsync(id);
            if (stockItem == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var itemName = stockItem.Name;

            _context.StockItems.Remove(stockItem);
            await _context.SaveChangesAsync();

            await _activityLogService.LogActivityAsync(
                user!.Id, "Delete", "StockItem", id, 
                $"Deleted stock item: {itemName}", 
                HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = "Stock item deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
} 