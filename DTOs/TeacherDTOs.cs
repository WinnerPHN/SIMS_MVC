using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SIMS_APDP.DTOs
{
    public class TeacherDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TeacherId { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Department { get; set; }
        public string? Specialization { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Username { get; set; } = string.Empty;
    }

    public class CreateTeacherRequest
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Teacher ID is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Teacher ID must be between 3 and 20 characters")]
        public string TeacherId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [Range(typeof(DateTime), "1900-01-01", "2020-12-31", ErrorMessage = "Date of birth must be between 1900 and 2020")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public string? Department { get; set; }

        [StringLength(100, ErrorMessage = "Specialization cannot exceed 100 characters")]
        public string? Specialization { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }
    }

    public class UpdateTeacherRequest
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Teacher ID is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Teacher ID must be between 3 and 20 characters")]
        public string TeacherId { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [Range(typeof(DateTime), "1900-01-01", "2020-12-31", ErrorMessage = "Date of birth must be between 1900 and 2020")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public string? Department { get; set; }

        [StringLength(100, ErrorMessage = "Specialization cannot exceed 100 characters")]
        public string? Specialization { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters")]
        public string Username { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }
    }

    public class TeacherCourseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Credits { get; set; }
        public DateTime CreatedAt { get; set; }
        public int EnrolledStudentsCount { get; set; }
    }
}
