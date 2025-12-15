using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProductApp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = null!;

        [Required, StringLength(150)]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ProfileTab? Profile { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        // Helper method to get role names
        public List<string> GetRoleNames()
        {
            return UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
        }

        public string GetProfilePhotoUrl()
        {
            if (Profile?.ProfilePhoto != null)
            {
                return $"/uploads/profiles/{Profile.ProfilePhoto}";
            }
            return "/images/default-avatar.png"; // Default avatar
        }

    }
}

