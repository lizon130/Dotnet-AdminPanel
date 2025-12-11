using System.ComponentModel.DataAnnotations;

namespace ProductApp.Models
{
    public class SiteSetting
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string? SiteName { get; set; }

        [StringLength(255)]
        public string? Logo { get; set; }   // Image file name
    }
}
