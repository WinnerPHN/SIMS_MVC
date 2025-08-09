using System.ComponentModel.DataAnnotations;

namespace SIMS_APDP.Models
{
    public class Student
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
        public string StudentId { get; set; } = string.Empty; // Student ID number
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign key to User
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        // Navigation property for course enrollments
        public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
    }
}
