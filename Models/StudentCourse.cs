using System.ComponentModel.DataAnnotations;

namespace SIMS_APDP.Models
{
    public class StudentCourse
    {
        public int Id { get; set; }
        
        // Foreign keys
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Grade information
        public decimal? Grade { get; set; } // Nullable grade
        
        [StringLength(2)]
        public string? LetterGrade { get; set; } // A, B, C, D, F, etc.
    }
}
