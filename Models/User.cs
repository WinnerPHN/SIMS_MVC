using System.ComponentModel.DataAnnotations;

namespace SIMS_APDP.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty; // "Admin", "Student", or "Teacher"
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Student? Student { get; set; }
        public Teacher? Teacher { get; set; }
    }
}
