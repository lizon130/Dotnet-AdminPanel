using ProductApp.Data;
using System.Linq;

namespace ProductApp.Helpers
{
    public static class RoleHelper
    {
        public static bool UserHasRole(AppDbContext context, int userId, string roleName)
        {
            return context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.Name)
                .Any(name => name == roleName);
        }

        public static List<string> GetUserRoles(AppDbContext context, int userId)
        {
            return context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.Name)
                .ToList();
        }

        public static bool IsUserInRole(AppDbContext context, int userId, int roleId)
        {
            return context.UserRoles
                .Any(ur => ur.UserId == userId && ur.RoleId == roleId);
        }
    }
}