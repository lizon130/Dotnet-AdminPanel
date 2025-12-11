using System.ComponentModel.DataAnnotations;

namespace ProductApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = null!;

        [Required, StringLength(150)]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

