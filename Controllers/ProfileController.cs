using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApp.Data;
using ProductApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ProductApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfileController(AppDbContext context, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Profile/MyProfile
        public async Task<IActionResult> MyProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // FIXED: Changed from Profiles to ProfileTabs
            var profile = await _context.ProfileTabs
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

            if (profile == null)
            {
                return RedirectToAction("Create");
            }

            var viewModel = new ProfileViewModel
            {
                Id = profile.Id,
                ExistingPhoto = profile.Photo,
                Designation = profile.Designation,
                PhoneNo = profile.PhoneNo,
                Address = profile.Address,
                UserId = profile.UserId,
                UserFullName = profile.User.FullName,
                UserEmail = profile.User.Email,
                UpdatedAt = profile.UpdatedAt
            };

            return View(viewModel);
        }

        // GET: Profile/Create
        public async Task<IActionResult> Create()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
                return NotFound();

            var viewModel = new ProfileViewModel
            {
                UserId = user.Id,
                UserFullName = user.FullName,
                UserEmail = user.Email
            };

            return View(viewModel);
        }

        // POST: Profile/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProfileViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var profile = new ProfileTab
                {
                    Designation = viewModel.Designation,
                    PhoneNo = viewModel.PhoneNo,
                    Address = viewModel.Address,
                    UserId = viewModel.UserId,
                    UpdatedAt = DateTime.Now
                };

                // Handle file upload using Photo property
                if (viewModel.PhotoFile != null && viewModel.PhotoFile.Length > 0)
                {
                    profile.Photo = await UploadFile(viewModel.PhotoFile);
                }

                // FIXED: Changed from Profiles to ProfileTabs
                _context.ProfileTabs.Add(profile);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profile created successfully!";
                return RedirectToAction("MyProfile");
            }

            // Reload user data
            var user = await _context.Users.FindAsync(viewModel.UserId);
            if (user != null)
            {
                viewModel.UserFullName = user.FullName;
                viewModel.UserEmail = user.Email;
            }

            return View(viewModel);
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // FIXED: Changed from Profiles to ProfileTabs
            var profile = await _context.ProfileTabs
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId.Value);

            if (profile == null)
                return NotFound();

            var viewModel = new ProfileViewModel
            {
                Id = profile.Id,
                ExistingPhoto = profile.Photo,
                Designation = profile.Designation,
                PhoneNo = profile.PhoneNo,
                Address = profile.Address,
                UserId = profile.UserId,
                UserFullName = profile.User.FullName,
                UserEmail = profile.User.Email,
                UpdatedAt = profile.UpdatedAt
            };

            return View(viewModel);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProfileViewModel viewModel)
        {
            if (id != viewModel.Id)
                return NotFound();

            var userId = GetCurrentUserId();
            if (userId == null || userId.Value != viewModel.UserId)
                return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    // FIXED: Changed from Profiles to ProfileTabs
                    var profile = await _context.ProfileTabs.FindAsync(id);
                    if (profile == null)
                        return NotFound();

                    profile.Designation = viewModel.Designation;
                    profile.PhoneNo = viewModel.PhoneNo;
                    profile.Address = viewModel.Address;
                    profile.UpdatedAt = DateTime.Now;

                    // Handle file upload using Photo property
                    if (viewModel.PhotoFile != null && viewModel.PhotoFile.Length > 0)
                    {
                        // Delete old file if exists
                        if (!string.IsNullOrEmpty(profile.Photo))
                        {
                            DeleteFile(profile.Photo);
                        }
                        profile.Photo = await UploadFile(viewModel.PhotoFile);
                    }

                    _context.Update(profile);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("MyProfile");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfileExists(viewModel.Id))
                        return NotFound();
                    throw;
                }
            }

            return View(viewModel);
        }

        // GET: Profile/DeletePhoto
        public async Task<IActionResult> DeletePhoto(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // FIXED: Changed from Profiles to ProfileTabs
            var profile = await _context.ProfileTabs
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId.Value);

            if (profile == null)
                return NotFound();

            if (!string.IsNullOrEmpty(profile.Photo))
            {
                DeleteFile(profile.Photo);
                profile.Photo = null;
                profile.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profile photo deleted successfully!";
            }

            return RedirectToAction("Edit", new { id });
        }

        private bool ProfileExists(int id)
        {
            // FIXED: Changed from Profiles to ProfileTabs
            return _context.ProfileTabs.Any(e => e.Id == id);
        }

        private int? GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdString, out int userId) ? userId : null;
        }

        private async Task<string> UploadFile(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profile");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return uniqueFileName; // This will be stored in Photo property
        }

        private void DeleteFile(string fileName)
        {
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", "profile", fileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        // Helper method to get photo URL
        private string GetPhotoUrl(string photoFileName)
        {
            if (string.IsNullOrEmpty(photoFileName))
                return null;

            return $"/uploads/profile/{photoFileName}";
        }
    }
}