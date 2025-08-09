using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS_APDP.Data;
using SIMS_APDP.DTOs;
using SIMS_APDP.Models;
using SIMS_APDP.Services;

namespace SIMS_APDP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly SIMSContext _context;
        private readonly AuthService _authService;

        public StudentsController(SIMSContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<StudentDTO>>> GetStudents()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Select(s => new StudentDTO
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email,
                    StudentId = s.StudentId,
                    PhoneNumber = s.PhoneNumber,
                    DateOfBirth = s.DateOfBirth,
                    Address = s.Address,
                    CreatedAt = s.CreatedAt,
                    Username = s.User.Username
                })
                .ToListAsync();

            return Ok(students);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StudentDTO>> GetStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return NotFound();

            var studentDto = new StudentDTO
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                StudentId = student.StudentId,
                PhoneNumber = student.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                Address = student.Address,
                CreatedAt = student.CreatedAt,
                Username = student.User.Username
            };

            return Ok(studentDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StudentDTO>> CreateStudent([FromBody] CreateStudentRequest request)
        {
            Console.WriteLine($"=== CreateStudent API Called ===");
            Console.WriteLine($"Request data: FirstName={request.FirstName}, LastName={request.LastName}, Email={request.Email}, StudentId={request.StudentId}, DateOfBirth={request.DateOfBirth}");
            Console.WriteLine($"Request body: {System.Text.Json.JsonSerializer.Serialize(request)}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"ModelState errors count: {ModelState.ErrorCount}");
            
            try
            {
                // Validate request
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    Console.WriteLine($"Validation errors: {System.Text.Json.JsonSerializer.Serialize(errors)}");
                    return BadRequest(new { message = "Validation failed", errors = errors });
                }

                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                {
                    Console.WriteLine($"ERROR: Username {request.Username} already exists");
                    return BadRequest(new { message = "Username already exists. Please choose a different username." });
                }

                // Check if email already exists
                if (await _context.Students.AnyAsync(s => s.Email == request.Email))
                {
                    Console.WriteLine($"ERROR: Email {request.Email} already exists");
                    return BadRequest(new { message = "Email already exists. Please use a different email address." });
                }

                // Check if student ID already exists
                if (await _context.Students.AnyAsync(s => s.StudentId == request.StudentId))
                {
                    Console.WriteLine($"ERROR: Student ID {request.StudentId} already exists");
                    return BadRequest(new { message = "Student ID already exists. Please check the student ID." });
                }

                // Validate date of birth (must be in the past and reasonable age)
                var today = DateTime.Today;
                
                // Check if date of birth is in the future
                if (request.DateOfBirth > today)
                {
                    Console.WriteLine($"ERROR: Date of birth {request.DateOfBirth} is in the future");
                    return BadRequest(new { message = "Date of birth cannot be in the future. Please enter a valid date." });
                }
                
                // Calculate age
                var age = today.Year - request.DateOfBirth.Year;
                if (request.DateOfBirth > today.AddYears(-age)) age--;
                
                Console.WriteLine($"Calculated age: {age}");
                
                if (age < 18 || age > 100)
                {
                    Console.WriteLine($"ERROR: Age {age} is not valid (must be 18-100)");
                    return BadRequest(new { message = "Student must be at least 18 years old. Please check the date of birth." });
                }

            // Create user
            var user = new User
            {
                Username = request.Username,
                PasswordHash = _authService.HashPassword(request.Password),
                Role = "Student",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create student
            var student = new Student
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                StudentId = request.StudentId,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                Address = request.Address,
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var studentDto = new StudentDTO
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                StudentId = student.StudentId,
                PhoneNumber = student.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                Address = student.Address,
                CreatedAt = student.CreatedAt,
                Username = user.Username
            };

                            Console.WriteLine($"SUCCESS: Student created with ID {student.Id}");
                return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, studentDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Exception occurred: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An error occurred while creating the student. Please try again." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentRequest request)
        {
            try
            {
                // Validate request
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { message = "Validation failed", errors = errors });
                }

                var student = await _context.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (student == null)
                    return NotFound(new { message = "Student not found" });

                // Check if email already exists (excluding current student)
                if (await _context.Students.AnyAsync(s => s.Email == request.Email && s.Id != id))
                    return BadRequest(new { message = "Email already exists. Please use a different email address." });

                // Check if student ID already exists (excluding current student)
                if (await _context.Students.AnyAsync(s => s.StudentId == request.StudentId && s.Id != id))
                    return BadRequest(new { message = "Student ID already exists. Please check the student ID." });

                // Check if username already exists (excluding current student)
                if (await _context.Users.AnyAsync(u => u.Username == request.Username && u.Id != student.UserId))
                    return BadRequest(new { message = "Username already exists. Please choose a different username." });

                // Validate date of birth (must be in the past and reasonable age)
                var today = DateTime.Today;
                
                // Check if date of birth is in the future
                if (request.DateOfBirth > today)
                    return BadRequest(new { message = "Date of birth cannot be in the future. Please enter a valid date." });
                
                // Calculate age
                var age = today.Year - request.DateOfBirth.Year;
                if (request.DateOfBirth > today.AddYears(-age)) age--;
                
                if (age < 18 || age > 100)
                    return BadRequest(new { message = "Student must be at least 18 years old. Please check the date of birth." });

                // Update student information
                student.FirstName = request.FirstName;
                student.LastName = request.LastName;
                student.Email = request.Email;
                student.StudentId = request.StudentId;
                student.PhoneNumber = request.PhoneNumber;
                student.DateOfBirth = request.DateOfBirth;
                student.Address = request.Address;

                // Update user information
                student.User.Username = request.Username;
                if (!string.IsNullOrEmpty(request.Password))
                {
                    student.User.PasswordHash = _authService.HashPassword(request.Password);
                }

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating student: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while updating the student. Please try again." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return NotFound();

            _context.Users.Remove(student.User);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("my-courses")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<StudentCourseDTO>>> GetMyCourses()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
                return NotFound();

            var courses = await _context.StudentCourses
                .Include(sc => sc.Course)
                .Include(sc => sc.Course.Teacher)
                .Include(sc => sc.Student)
                .Where(sc => sc.StudentId == student.Id)
                .Select(sc => new StudentCourseDTO
                {
                    Id = sc.Id,
                    StudentId = sc.StudentId,
                    CourseId = sc.CourseId,
                    StudentName = $"{sc.Student.FirstName} {sc.Student.LastName}",
                    CourseName = sc.Course.Name,
                    CourseCode = sc.Course.Code,
                        TeacherName = sc.Course.Teacher != null ? ($"{sc.Course.Teacher.FirstName} {sc.Course.Teacher.LastName}") : null,
                    EnrolledAt = sc.EnrolledAt,
                    Grade = sc.Grade,
                    LetterGrade = sc.LetterGrade
                })
                .ToListAsync();

            return Ok(courses);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<StudentDTO>> GetMe()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
                return NotFound(new { message = "Student profile not found" });

            var dto = new StudentDTO
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                StudentId = student.StudentId,
                PhoneNumber = student.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                Address = student.Address,
                CreatedAt = student.CreatedAt,
                Username = student.User.Username
            };

            return Ok(dto);
        }
    }
}
