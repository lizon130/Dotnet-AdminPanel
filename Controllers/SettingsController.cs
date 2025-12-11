using Microsoft.AspNetCore.Mvc;
using ProductApp.Data;
using ProductApp.Models;

namespace ProductApp.Controllers
{
    public class SettingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SettingsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET
        public IActionResult Index()
        {
            var setting = _context.SiteSettings.FirstOrDefault();

            if (setting == null)
            {
                setting = new SiteSetting { SiteName = "ProductApp" };
                _context.SiteSettings.Add(setting);
                _context.SaveChanges();
            }

            return View(setting);
        }

        // POST
        [HttpPost]
        public IActionResult Index(SiteSetting model, IFormFile? LogoFile)
        {
            var setting = _context.SiteSettings.First();

            setting.SiteName = model.SiteName;

            if (LogoFile != null)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = Guid.NewGuid() + Path.GetExtension(LogoFile.FileName);
                var fullPath = Path.Combine(uploadPath, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                LogoFile.CopyTo(stream);

                setting.Logo = fileName;
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
