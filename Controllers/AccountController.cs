using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApp.Data;
using ProductApp.Helpers;
using ProductApp.Models;
using System.Linq;

namespace ProductApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ REGISTER
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(User model)
        {
            if (_context.Users.Any(x => x.Email == model.Email))
            {
                ViewBag.Error = "Email already exists!";
                return View();
            }

            model.PasswordHash = PasswordHelper.HashPassword(model.PasswordHash);

            _context.Users.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        // ✅ LOGIN
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var hash = PasswordHelper.HashPassword(password);

            // IMPORTANT: Include UserRoles and Role
            var user = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .SingleOrDefault(x => x.Email == email && x.PasswordHash == hash);

            if (user == null)
            {
                ViewBag.Error = "Invalid Email or Password!";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);

            // Store roles in Session
            var roles = user.UserRoles.Select(ur => ur.Role?.Name).Where(name => !string.IsNullOrEmpty(name)).ToList();
            HttpContext.Session.SetString("UserRoles", string.Join(",", roles));

            // Also set UserRole for backward compatibility
            HttpContext.Session.SetString("UserRole", roles.FirstOrDefault() ?? "User");

            return RedirectToAction("Index", "Home");
        }

        // ✅ LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


        // ✅ VIEW ALL USERS (Admin/Management)
        public IActionResult Users()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        // ✅ CHANGE PASSWORD
        public IActionResult ChangePassword()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please log in to change your password.";
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please log in to change your password.";
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Get current user
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Verify current password
            var currentPasswordHash = PasswordHelper.HashPassword(model.CurrentPassword);
            if (user.PasswordHash != currentPasswordHash)
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            // Check if new password is same as current password
            var newPasswordHash = PasswordHelper.HashPassword(model.NewPassword);
            if (user.PasswordHash == newPasswordHash)
            {
                ModelState.AddModelError("NewPassword", "New password must be different from current password.");
                return View(model);
            }

            // Update password
            user.PasswordHash = newPasswordHash;
            _context.Users.Update(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("ChangePassword", "Account"); 
        }

        // ✅ FORGOT PASSWORD (Optional)
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            // This is a simple version - in production, you'd send an email
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
                // In a real app, send password reset email here
                TempData["InfoMessage"] = "If an account exists with this email, password reset instructions have been sent.";
            }
            else
            {
                // Don't reveal that the user doesn't exist for security
                TempData["InfoMessage"] = "If an account exists with this email, password reset instructions have been sent.";
            }

            return View();
        }

        // ✅ RESET PASSWORD (Optional)
        public IActionResult ResetPassword(string token)
        {
            // Verify token and show reset form
            // This requires a token system which is more complex
            return View();
        }

    }
}