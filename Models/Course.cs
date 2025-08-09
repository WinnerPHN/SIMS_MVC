using System.ComponentModel.DataAnnotations;

namespace SIMS_APDP.Models
{
    public class Course
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty; // Course code like "CS101"
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Range(1, 999999999)]
        public int Credits { get; set; }
        
        // Foreign key to Teacher (optional)
        public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property for student enrollments
        public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
    }
}
