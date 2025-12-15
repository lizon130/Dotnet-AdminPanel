using System;
using System.ComponentModel.DataAnnotations;

namespace ProductApp.ViewModels
{
    public class ProfileViewModel
    {
        // User Info
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        // Profile Info
        public string? ProfilePhoto { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }

        [DataType(DataType.PostalCode)]
        public string? PostalCode { get; set; }

        // Professional Information
        [Required(ErrorMessage = "Designation is required")]
        public string Designation { get; set; } = string.Empty;

        public string? Department { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Bio { get; set; }

        // Social Links
        [Url(ErrorMessage = "Invalid URL")]
        public string? LinkedIn { get; set; }

        [Url(ErrorMessage = "Invalid URL")]
        public string? Twitter { get; set; }

        [Url(ErrorMessage = "Invalid URL")]
        public string? GitHub { get; set; }

        [Url(ErrorMessage = "Invalid URL")]
        public string? Website { get; set; }

        // Additional Info
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        // For file upload
        public IFormFile? ProfilePhotoFile { get; set; }
    }
}