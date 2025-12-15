using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApp.Data;
using ProductApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApp.Controllers
{
    public class RolesController : Controller
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: View all roles
        public IActionResult Index()
        {
            var roles = _context.Roles.ToList();
            return View(roles);
        }

        // GET: Create role form
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create a new role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role role)
        {
            if (ModelState.IsValid)
            {
                // Check if role name already exists
                if (_context.Roles.Any(r => r.Name == role.Name))
                {
                    ModelState.AddModelError("Name", "Role name already exists.");
                    return View(role);
                }

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Role created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // GET: Edit role
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: Edit role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Role role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Check if role name already exists (excluding current role)
                if (_context.Roles.Any(r => r.Name == role.Name && r.Id != id))
                {
                    ModelState.AddModelError("Name", "Role name already exists.");
                    return View(role);
                }

                try
                {
                    _context.Roles.Update(role);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Role updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoleExists(role.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // GET: Delete role
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(m => m.Id == id);

            if (role == null)
            {
                return NotFound();
            }

            // Check if role is assigned to any user
            var userCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == id);
            ViewBag.UserCount = userCount;

            return View(role);
        }

        // POST: Delete role
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                // Check if role is assigned to any user
                var userCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == id);
                if (userCount > 0)
                {
                    TempData["Error"] = "Cannot delete role because it is assigned to users. Remove all users from this role first.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Role deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool RoleExists(int id)
        {
            return _context.Roles.Any(e => e.Id == id);
        }
    }
}