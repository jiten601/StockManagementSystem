using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockManagementSystem.Models;
using StockManagementSystem.Services;

namespace StockManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IActivityLogService _activityLogService;

        public UserController(
            UserManager<ApplicationUser> userManager,
            IActivityLogService activityLogService)
        {
            _userManager = userManager;
            _activityLogService = activityLogService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var userViewModels = new List<object>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.IsActive,
                    user.CreatedAt,
                    user.LastLoginAt,
                    Roles = string.Join(", ", roles)
                });
            }

            return View(userViewModels);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;

            return View(user);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,FullName,Email,Role,IsActive")] ApplicationUser model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    var currentUser = await _userManager.GetUserAsync(User);
                    var oldRole = user.Role;
                    var oldActiveStatus = user.IsActive;

                    user.FullName = model.FullName;
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    user.Role = model.Role;
                    user.IsActive = model.IsActive;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        // Update roles if role changed
                        if (oldRole != model.Role)
                        {
                            var currentRoles = await _userManager.GetRolesAsync(user);
                            await _userManager.RemoveFromRolesAsync(user, currentRoles);
                            await _userManager.AddToRoleAsync(user, model.Role);
                        }

                        await _activityLogService.LogActivityAsync(
                            currentUser!.Id, "Update", "User", null, 
                            $"Updated user: {user.FullName} (Role: {oldRole} -> {model.Role}, Active: {oldActiveStatus} -> {model.IsActive})", 
                            HttpContext.Connection.RemoteIpAddress?.ToString());

                        TempData["Success"] = "User updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var oldStatus = user.IsActive;
            user.IsActive = !user.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _activityLogService.LogActivityAsync(
                    currentUser!.Id, "ToggleActive", "User", null, 
                    $"Changed user status: {user.FullName} ({oldStatus} -> {user.IsActive})", 
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["Success"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to update user status.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent deleting the current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (user.Id == currentUser!.Id)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            // Prevent deleting admin users
            if (user.Role == "Admin")
            {
                TempData["Error"] = "Admin users cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent deleting the current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (user.Id == currentUser!.Id)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            // Prevent deleting admin users
            if (user.Role == "Admin")
            {
                TempData["Error"] = "Admin users cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            var userName = user.FullName;
            var result = await _userManager.DeleteAsync(user);
            
            if (result.Succeeded)
            {
                await _activityLogService.LogActivityAsync(
                    currentUser.Id, "Delete", "User", null, 
                    $"Deleted user: {userName}", 
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["Success"] = $"User '{userName}' has been deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete user. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _userManager.Users.Any(e => e.Id == id);
        }
    }
} 