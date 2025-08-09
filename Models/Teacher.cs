using System.ComponentModel.DataAnnotations;

namespace SIMS_APDP.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string TeacherId { get; set; } = string.Empty; // Teacher ID number
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        [StringLength(200)]
        public string? Department { get; set; }
        
        [StringLength(100)]
        public string? Specialization { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign key to User
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        // Navigation property for courses
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
