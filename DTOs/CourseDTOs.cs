using System.ComponentModel.DataAnnotations;

namespace SIMS_APDP.DTOs
{
    public class CourseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Credits { get; set; }
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCourseRequest
    {
        [Required(ErrorMessage = "Course name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Course name must be between 3 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course code is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Course code must be between 2 and 20 characters")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Credits is required")]
        [Range(1, 999999999, ErrorMessage = "Credits must be between 1 and 999999999")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "Teacher is required")]
        public int TeacherId { get; set; }
    }

    public class UpdateCourseRequest
    {
        [Required(ErrorMessage = "Course name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Course name must be between 3 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course code is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Course code must be between 2 and 20 characters")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Credits is required")]
        [Range(1, 999999999, ErrorMessage = "Credits must be between 1 and 999999999")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "Teacher is required")]
        public int TeacherId { get; set; }
    }

    public class StudentCourseDTO
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string? TeacherName { get; set; }
        public DateTime EnrolledAt { get; set; }
        public decimal? Grade { get; set; }
        public string? LetterGrade { get; set; }
    }

    public class EnrollStudentRequest
    {
        [Required(ErrorMessage = "Student is required")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Course is required")]
        public int CourseId { get; set; }
    }

    public class AssignTeacherRequest
    {
        [Required(ErrorMessage = "Teacher is required")]
        public int TeacherId { get; set; }
    }
}
