using System;
using System.Collections.Generic;

namespace ProductApp.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation property for many-to-many relationship
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}