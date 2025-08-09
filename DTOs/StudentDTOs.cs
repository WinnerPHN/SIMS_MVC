using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SIMS_APDP.DTOs
{
    public class StudentDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Username { get; set; } = string.Empty;
    }

    public class CreateStudentRequest
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

        [Required(ErrorMessage = "Student ID is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Student ID must be between 3 and 20 characters")]
        public string StudentId { get; set; } = string.Empty;

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
        public DateTime DateOfBirth { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }
    }

    public class UpdateStudentRequest
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

        [Required(ErrorMessage = "Student ID is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Student ID must be between 3 and 20 characters")]
        public string StudentId { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters")]
        public string Username { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }
    }
}
