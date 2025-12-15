using System;
using System.ComponentModel.DataAnnotations;

namespace ProductApp.Models
{
    public class ProfileTab
    {
        public int Id { get; set; }

        // Foreign key to User
        public int UserId { get; set; }

        // Personal Information
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

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Property
        public virtual User ? User { get; set; } 
    }
}