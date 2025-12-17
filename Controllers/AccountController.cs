using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApp.Data;
using ProductApp.Helpers;
using ProductApp.Models;
using ProductApp.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ✅ REGISTER
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            if (_context.Users.Any(x => x.Email == model.Email))
            {
                ViewBag.Error = "Email already exists!";
                return View();
            }

            model.PasswordHash = PasswordHelper.HashPassword(model.PasswordHash);

            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            // Send welcome email
            try
            {
                // Get app URL
                var appUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

                await SendWelcomeEmail(model.Email, model.FullName, appUrl);

                TempData["SuccessMessage"] = $"Registration successful! A welcome email has been sent to {model.Email}";
            }
            catch (Exception ex)
            {
                // Log the error but don't fail registration
                Console.WriteLine($"Failed to send welcome email: {ex.Message}");
                TempData["SuccessMessage"] = "Registration successful! You can now login. (Email sending failed)";
            }

            return RedirectToAction("Login");
        }

        private async Task SendWelcomeEmail(string email, string name, string appUrl)
        {
            var subject = "Welcome to ProductApp!";

            var message = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }}
                        .header {{ background: linear-gradient(180deg, #2c3e50 0%, #1a252f 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .button {{ background: #2c3e50; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 15px 0; }}
                        .footer {{ margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; font-size: 12px; text-align: center; }}
                        .info-box {{ background: #e8f4fd; border-left: 4px solid #2c3e50; padding: 15px; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>Welcome to ProductApp!</h1>
                    </div>
                    <div class='content'>
                        <h2>Hello {name},</h2>
                        <p>Thank you for registering with <strong>ProductApp</strong>. Your account has been successfully created and is ready to use!</p>
                        
                        <div class='info-box'>
                            <p><strong>Your Login Details:</strong></p>
                            <p>Email: <strong>{email}</strong></p>
                            <p>You can now log in to start managing your products.</p>
                        </div>
                        
                        <p style='text-align: center;'>
                            <a href='{appUrl}/Account/Login' class='button'>Login to Your Account</a>
                        </p>
                        
                        <p>With your new account, you can:</p>
                        <ul>
                            <li>Manage products and inventory</li>
                            <li>Track sales and orders</li>
                            <li>Generate reports</li>
                            <li>And much more!</li>
                        </ul>
                        
                        <p>If you have any questions or need assistance, please don't hesitate to contact our support team.</p>
                        
                        <p>Best regards,<br>
                        <strong>The ProductApp Team</strong></p>
                    </div>
                    <div class='footer'>
                        <p>This is an automated message, please do not reply to this email.</p>
                        <p>© {DateTime.Now.Year} ProductApp. All rights reserved.</p>
                    </div>
                </body>
                </html>";

            await _emailService.SendEmailAsync(email, subject, message);
        }

        // ✅ LOGIN
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var hash = PasswordHelper.HashPassword(password);

            // IMPORTANT: Include UserRoles and Role
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(x => x.Email == email && x.PasswordHash == hash);

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
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
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
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
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
            var user = await _context.Users.FindAsync(userId);
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
            await _context.SaveChangesAsync();

            // Send password change confirmation email
            try
            {
                var appUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

                // FIX: Use the same approach as welcome email
                var subject = "Your Password Has Been Changed - ProductApp";
                var changeTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                var message = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }}
                    .header {{ background: #f8d7da; color: #721c24; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; border: 1px solid #f5c6cb; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                    .info-box {{ background: #d4edda; border-left: 4px solid #28a745; padding: 15px; margin: 20px 0; }}
                    .warning-box {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
                    .footer {{ margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; font-size: 12px; text-align: center; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>Password Changed Successfully</h1>
                </div>
                <div class='content'>
                    <h2>Hello {user.FullName},</h2>
                    <p>Your ProductApp account password has been successfully changed.</p>
                    
                    <div class='info-box'>
                        <p><strong>Password Change Details:</strong></p>
                        <p><strong>Account:</strong> {user.Email}</p>
                        <p><strong>Change Time:</strong> {changeTime}</p>
                        <p><strong>IP Address:</strong> {ipAddress}</p>
                    </div>
                    
                    <div class='warning-box'>
                        <p><strong>⚠️ Security Notice:</strong></p>
                        <p>If you did not make this change, please reset your password immediately.</p>
                    </div>
                    
                    <p>Best regards,<br>
                    <strong>The ProductApp Security Team</strong></p>
                </div>
                <div class='footer'>
                    <p>This is an automated security notification.</p>
                    <p>© {DateTime.Now.Year} ProductApp. All rights reserved.</p>
                </div>
            </body>
            </html>";

                // FIX: Use the email service directly (same as welcome email)
                await _emailService.SendEmailAsync(user.Email, subject, message);

                TempData["SuccessMessage"] = "Password changed successfully! A confirmation email has been sent.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send password change email: {ex.Message}");
                // Don't fail the password change if email fails
                TempData["SuccessMessage"] = "Password changed successfully! (Email notification failed to send)";
            }

            return RedirectToAction("ChangePassword");
        }

        private async Task SendPasswordChangeEmail(string email, string name, string appUrl)
        {
            var subject = "Your Password Has Been Changed - ProductApp";

            // Get current date/time and IP address (optional)
            var changeTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var message = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }}
                        .header {{ background: #f8d7da; color: #721c24; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; border: 1px solid #f5c6cb; }}
                        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .info-box {{ background: #d4edda; border-left: 4px solid #28a745; padding: 15px; margin: 20px 0; }}
                        .warning-box {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
                        .footer {{ margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; font-size: 12px; text-align: center; }}
                        .details-table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
                        .details-table td {{ padding: 8px; border-bottom: 1px solid #ddd; }}
                        .details-table td:first-child {{ font-weight: bold; width: 40%; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>Password Changed Successfully</h1>
                    </div>
                    <div class='content'>
                        <h2>Hello {name},</h2>
                        <p>Your ProductApp account password has been successfully changed.</p>
                        
                        <div class='info-box'>
                            <p><strong>Password Change Details:</strong></p>
                            <table class='details-table'>
                                <tr>
                                    <td>Account:</td>
                                    <td>{email}</td>
                                </tr>
                                <tr>
                                    <td>Change Time:</td>
                                    <td>{changeTime} (UTC)</td>
                                </tr>
                                <tr>
                                    <td>IP Address:</td>
                                    <td>{ipAddress}</td>
                                </tr>
                            </table>
                        </div>
                        
                        <div class='warning-box'>
                            <p><strong>⚠️ Security Notice:</strong></p>
                            <p>If you did not make this change:</p>
                            <ol>
                                <li>Immediately reset your password by visiting: <a href='{appUrl}/Account/ForgotPassword'>Reset Password</a></li>
                                <li>Contact our support team if you need assistance</li>
                                <li>Check your account for any suspicious activity</li>
                            </ol>
                        </div>
                        
                        <p>If you made this change, no further action is required.</p>
                        
                        <p>For security reasons, please remember:</p>
                        <ul>
                            <li>Never share your password with anyone</li>
                            <li>Use a strong, unique password</li>
                            <li>Enable two-factor authentication if available</li>
                            <li>Regularly update your password</li>
                        </ul>
                        
                        <p>Need help? <a href='{appUrl}/Home/Contact'>Contact our support team</a></p>
                        
                        <p>Best regards,<br>
                        <strong>The ProductApp Security Team</strong></p>
                    </div>
                    <div class='footer'>
                        <p>This is an automated security notification. Please do not reply to this email.</p>
                        <p>© {DateTime.Now.Year} ProductApp. All rights reserved.</p>
                    </div>
                </body>
                </html>";

            await _emailService.SendEmailAsync(email, subject, message);
        }
    }
}