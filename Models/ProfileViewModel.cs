using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ProductApp.Models
{
    public class ProfileViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Profile Photo")]
        public IFormFile? PhotoFile { get; set; }

        public string? ExistingPhoto { get; set; } // Store the existing photo filename

        [StringLength(100)]
        [Display(Name = "Designation")]
        public string? Designation { get; set; }

        [StringLength(20)]
        [Display(Name = "Phone Number")]
        [Phone]
        public string? PhoneNo { get; set; }

        [StringLength(500)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        public int UserId { get; set; }

        [Display(Name = "Full Name")]
        public string UserFullName { get; set; } = null!;

        [Display(Name = "Email")]
        public string UserEmail { get; set; } = null!;

        [Display(Name = "Last Updated")]
        public DateTime? UpdatedAt { get; set; }
    }
}