using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApp.Data;
using ProductApp.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace ProductApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: View profile
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please log in to view your profile.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login", "Account");
            }

            // Create profile if doesn't exist
            if (user.Profile == null)
            {
                user.Profile = new ProfileTab
                {
                    UserId = user.Id,
                    Designation = "User",
                    PhoneNumber = "",
                    Address = ""
                };
                await _context.ProfileTabs.AddAsync(user.Profile);
                await _context.SaveChangesAsync();
            }

            return View(user);
        }

        // GET: Edit profile
        public async Task<IActionResult> Edit()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await _context.ProfileTabs
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                // Create new profile if doesn't exist
                var user = await _context.Users.FindAsync(userId);
                profile = new ProfileTab
                {
                    UserId = userId.Value,
                    User = user
                };
            }

            return View(profile);
        }

        // POST: Update profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileTab profile, IFormFile? profilePhoto)
        {
            if (!ModelState.IsValid)
            {
                // Reload user data for the view
                profile.User = await _context.Users.FindAsync(profile.UserId);
                return View(profile);
            }

            try
            {
                // Handle profile photo upload
                if (profilePhoto != null && profilePhoto.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(profilePhoto.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ProfilePhoto", "Only image files (jpg, jpeg, png, gif) are allowed.");
                        profile.User = await _context.Users.FindAsync(profile.UserId);
                        return View(profile);
                    }

                    // Validate file size (max 5MB)
                    if (profilePhoto.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ProfilePhoto", "File size should not exceed 5MB.");
                        profile.User = await _context.Users.FindAsync(profile.UserId);
                        return View(profile);
                    }

                    // Delete old photo if exists
                    if (!string.IsNullOrEmpty(profile.ProfilePhoto))
                    {
                        var oldPhotoPath = Path.Combine(_environment.WebRootPath, "uploads", "profiles", profile.ProfilePhoto);
                        if (System.IO.File.Exists(oldPhotoPath))
                        {
                            System.IO.File.Delete(oldPhotoPath);
                        }
                    }

                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePhoto.CopyToAsync(stream);
                    }

                    profile.ProfilePhoto = fileName;
                }

                // Check if profile exists
                var existingProfile = await _context.ProfileTabs.FindAsync(profile.Id);

                if (existingProfile == null)
                {
                    // Create new profile
                    profile.CreatedAt = DateTime.Now;
                    profile.UpdatedAt = DateTime.Now;
                    await _context.ProfileTabs.AddAsync(profile);
                }
                else
                {
                    // Update existing profile
                    existingProfile.PhoneNumber = profile.PhoneNumber;
                    existingProfile.Address = profile.Address;
                    existingProfile.City = profile.City;
                    existingProfile.State = profile.State;
                    existingProfile.Country = profile.Country;
                    existingProfile.PostalCode = profile.PostalCode;
                    existingProfile.Designation = profile.Designation;
                    existingProfile.Department = profile.Department;
                    existingProfile.Bio = profile.Bio;
                    existingProfile.LinkedIn = profile.LinkedIn;
                    existingProfile.Twitter = profile.Twitter;
                    existingProfile.GitHub = profile.GitHub;
                    existingProfile.Website = profile.Website;
                    existingProfile.DateOfBirth = profile.DateOfBirth;
                    existingProfile.Gender = profile.Gender;
                    existingProfile.UpdatedAt = DateTime.Now;

                    // Only update photo if a new one was uploaded
                    if (!string.IsNullOrEmpty(profile.ProfilePhoto))
                    {
                        existingProfile.ProfilePhoto = profile.ProfilePhoto;
                    }

                    _context.ProfileTabs.Update(existingProfile);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                profile.User = await _context.Users.FindAsync(profile.UserId);
                return View(profile);
            }
        }

        // POST: Delete profile photo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhoto()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            var profile = await _context.ProfileTabs
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null || string.IsNullOrEmpty(profile.ProfilePhoto))
            {
                return Json(new { success = false, message = "No photo to delete" });
            }

            try
            {
                // Delete file from server
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "profiles", profile.ProfilePhoto);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Update database
                profile.ProfilePhoto = null;
                profile.UpdatedAt = DateTime.Now;
                _context.ProfileTabs.Update(profile);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Photo deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: View other user's profile (for admin)
        [Route("Profile/View/{id}")]
        public async Task<IActionResult> ViewProfile(int id)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user is admin
            var userRoles = HttpContext.Session.GetString("UserRoles")?.Split(',') ?? Array.Empty<string>();
            if (!userRoles.Contains("Admin") && !userRoles.Contains("SuperAdmin") && currentUserId != id)
            {
                TempData["ErrorMessage"] = "You don't have permission to view this profile.";
                return RedirectToAction("Index");
            }

            var user = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            return View("ViewUserProfile", user);
        }

        // SIMPLE TEST METHOD - Add this to ProfileController
        [HttpPost]
        public async Task<IActionResult> TestSimpleSave(string testPhone, string testAddress, string testDesignation)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Content("ERROR: No user logged in");
                }

                Console.WriteLine($"TestSimpleSave called for user {userId}");
                Console.WriteLine($"Phone: {testPhone}, Address: {testAddress}, Designation: {testDesignation}");

                // Check if profile exists
                var existingProfile = await _context.ProfileTabs
                    .FirstOrDefaultAsync(p => p.UserId == userId.Value);

                if (existingProfile == null)
                {
                    // Create new profile
                    var profile = new ProfileTab
                    {
                        UserId = userId.Value,
                        PhoneNumber = testPhone,
                        Address = testAddress,
                        Designation = testDesignation,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _context.ProfileTabs.Add(profile);
                    Console.WriteLine("Creating NEW profile");
                }
                else
                {
                    // Update existing
                    existingProfile.PhoneNumber = testPhone;
                    existingProfile.Address = testAddress;
                    existingProfile.Designation = testDesignation;
                    existingProfile.UpdatedAt = DateTime.Now;

                    _context.ProfileTabs.Update(existingProfile);
                    Console.WriteLine($"Updating existing profile ID: {existingProfile.Id}");
                }

                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChanges result: {result} rows affected");

                return Content($"SUCCESS! Rows affected: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in TestSimpleSave: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Content($"ERROR: {ex.Message}<br>{ex.InnerException?.Message}");
            }
        }
    }
}