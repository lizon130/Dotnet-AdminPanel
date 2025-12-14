using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductApp.Models
{
    public class ProfileTab
    {
        [Key]
        public int Id { get; set; }

        [StringLength(500)]
        public string? Photo { get; set; }  // URL or file path to profile photo

        [StringLength(100)]
        public string? Designation { get; set; }  // Job title/position

        [StringLength(20)]
        public string? PhoneNo { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public string? PhotoUrl =>
        string.IsNullOrEmpty(Photo) ? null : $"/uploads/profile/{Photo}";
    }
}
