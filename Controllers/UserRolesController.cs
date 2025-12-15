using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApp.Data;
using ProductApp.Models;
using ProductApp.ViewModels;  // Add this line
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApp.Controllers
{
    public class UserRolesController : Controller
    {
        private readonly AppDbContext _context;

        public UserRolesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: View all users with their roles
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();

            return View(users);
        }

        // GET: Manage roles for a specific user
        public async Task<IActionResult> Manage(int? userId)
        {
            if (userId == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var allRoles = await _context.Roles.ToListAsync();

            var model = new UserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.FullName,
                UserEmail = user.Email,
                UserRoles = user.UserRoles.Select(ur => ur.RoleId).ToList(),
                AllRoles = allRoles.Select(r => new RoleCheckbox
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    IsSelected = user.UserRoles.Any(ur => ur.RoleId == r.Id)
                }).ToList()
            };

            return View(model);
        }

        // POST: Update user roles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(UserRolesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload roles for the view
                model.AllRoles = await _context.Roles
                    .Select(r => new RoleCheckbox
                    {
                        RoleId = r.Id,
                        RoleName = r.Name,
                        IsSelected = model.SelectedRoleIds != null && model.SelectedRoleIds.Contains(r.Id)
                    })
                    .ToListAsync();

                return View(model);
            }

            // Get current user with roles
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == model.UserId);

            if (user == null)
            {
                return NotFound();
            }

            // Remove existing roles
            var existingUserRoles = _context.UserRoles.Where(ur => ur.UserId == model.UserId);
            _context.UserRoles.RemoveRange(existingUserRoles);

            // Add selected roles
            if (model.SelectedRoleIds != null && model.SelectedRoleIds.Any())
            {
                foreach (var roleId in model.SelectedRoleIds)
                {
                    var userRole = new UserRole
                    {
                        UserId = model.UserId,
                        RoleId = roleId
                    };
                    _context.UserRoles.Add(userRole);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "User roles updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: View users in a specific role
        public async Task<IActionResult> RoleUsers(int? roleId)
        {
            if (roleId == null)
            {
                return NotFound();
            }

            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                return NotFound();
            }

            var users = await _context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .Include(ur => ur.User)
                .Select(ur => ur.User)
                .ToListAsync();

            ViewBag.RoleName = role.Name;
            return View(users);
        }
    }
}