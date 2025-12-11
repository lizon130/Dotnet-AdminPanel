using Microsoft.AspNetCore.Mvc;
using ProductApp.Data;
using ProductApp.Helpers;
using ProductApp.Models;

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

            var user = _context.Users.SingleOrDefault(x =>
                x.Email == email && x.PasswordHash == hash);

            if (user == null)
            {
                ViewBag.Error = "Invalid Email or Password!";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);

            return RedirectToAction("Index", "Home");
        }

        // ✅ LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}