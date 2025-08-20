using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StockManagementSystem.Models;
using StockManagementSystem.Models.ViewModels;
using StockManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;

namespace StockManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IActivityLogService _activityLogService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IActivityLogService activityLogService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _activityLogService = activityLogService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                ProfileImage = user.ProfileImage
            };

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                ProfileImage = user.ProfileImage
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EditProfile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                // Handle profile image upload
                if (model.NewProfileImage != null && model.NewProfileImage.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(model.NewProfileImage.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("NewProfileImage", "Only JPG, PNG, and GIF files are allowed.");
                        return View(model);
                    }
                    
                    // Validate file size (5MB limit)
                    if (model.NewProfileImage.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("NewProfileImage", "File size must be less than 5MB.");
                        return View(model);
                    }
                    
                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profile-images");
                    
                    // Ensure directory exists
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }
                    
                    var filePath = Path.Combine(uploadPath, fileName);
                    
                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.NewProfileImage.CopyToAsync(stream);
                    }
                    
                    user.ProfileImage = $"/profile-images/{fileName}";
                }

                // Update other fields if needed
                if (user.FullName != model.FullName)
                {
                    user.FullName = model.FullName;
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Refresh session values to reflect profile changes
                    HttpContext.Session.SetString("FullName", user.FullName);
                    if (!string.IsNullOrEmpty(user.ProfileImage))
                    {
                        HttpContext.Session.SetString("ProfileImage", user.ProfileImage);
                    }
                    else
                    {
                        HttpContext.Session.Remove("ProfileImage");
                    }

                    await _activityLogService.LogActivityAsync(user.Id, "UpdateProfile", "User", null, "Profile updated", HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("Profile");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        user.LastLoginAt = DateTime.Now;
                        await _userManager.UpdateAsync(user);
                        await _activityLogService.LogActivityAsync(user.Id, "Login", "User", null, "User logged in successfully", HttpContext.Connection.RemoteIpAddress?.ToString());

                        // Store user info in session
                        HttpContext.Session.SetString("FullName", user.FullName);
                        HttpContext.Session.SetString("Role", user.Role);
                        if (!string.IsNullOrEmpty(user.ProfileImage))
                        {
                            HttpContext.Session.SetString("ProfileImage", user.ProfileImage);
                        }
                    }
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                string? profileImagePath = null;
                
                // Handle profile image upload
                if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(model.ProfileImage.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ProfileImage", "Only JPG, PNG, and GIF files are allowed.");
                        return View(model);
                    }
                    
                    // Validate file size (5MB limit)
                    if (model.ProfileImage.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ProfileImage", "File size must be less than 5MB.");
                        return View(model);
                    }
                    
                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profile-images");
                    
                    // Ensure directory exists
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }
                    
                    var filePath = Path.Combine(uploadPath, fileName);
                    
                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfileImage.CopyToAsync(stream);
                    }
                    
                    profileImagePath = $"/profile-images/{fileName}";
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    Role = model.Role,
                    ProfileImage = profileImagePath
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _activityLogService.LogActivityAsync(user.Id, "Register", "User", null, "New user registered", HttpContext.Connection.RemoteIpAddress?.ToString());
                    
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Initialize session for the newly registered user
                    HttpContext.Session.SetString("FullName", user.FullName);
                    HttpContext.Session.SetString("Role", user.Role);
                    if (!string.IsNullOrEmpty(user.ProfileImage))
                    {
                        HttpContext.Session.SetString("ProfileImage", user.ProfileImage);
                    }

                    return RedirectToLocal(returnUrl);
                }
                AddErrors(result);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await _activityLogService.LogActivityAsync(user.Id, "Logout", "User", null, "User logged out", HttpContext.Connection.RemoteIpAddress?.ToString());
            }
            
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CheckRoles()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = roles;
            ViewBag.UserEmail = user.Email;
            ViewBag.UserName = user.FullName;
            
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AssignAdminRole()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Admin role has been assigned to your account.";
                await _activityLogService.LogActivityAsync(user.Id, "Role Assignment", "User", null, "Admin role assigned", HttpContext.Connection.RemoteIpAddress?.ToString());
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to assign Admin role. You may already have this role.";
            }

            return RedirectToAction("CheckRoles");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
} 