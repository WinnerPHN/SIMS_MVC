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
    public class TeachersController : ControllerBase
    {
        private readonly SIMSContext _context;
        private readonly AuthService _authService;

        public TeachersController(SIMSContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<TeacherDTO>>> GetTeachers()
        {
            var teachers = await _context.Teachers
                .Include(t => t.User)
                .Select(t => new TeacherDTO
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    Email = t.Email,
                    TeacherId = t.TeacherId,
                    PhoneNumber = t.PhoneNumber,
                    DateOfBirth = t.DateOfBirth,
                    Address = t.Address,
                    Department = t.Department,
                    Specialization = t.Specialization,
                    CreatedAt = t.CreatedAt,
                    Username = t.User.Username
                })
                .ToListAsync();

            return Ok(teachers);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TeacherDTO>> GetTeacher(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
                return NotFound();

            var teacherDto = new TeacherDTO
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Email = teacher.Email,
                TeacherId = teacher.TeacherId,
                PhoneNumber = teacher.PhoneNumber,
                DateOfBirth = teacher.DateOfBirth,
                Address = teacher.Address,
                Department = teacher.Department,
                Specialization = teacher.Specialization,
                CreatedAt = teacher.CreatedAt,
                Username = teacher.User.Username
            };

            return Ok(teacherDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TeacherDTO>> CreateTeacher([FromBody] CreateTeacherRequest request)
        {
            Console.WriteLine($"=== CreateTeacher API Called ===");
            Console.WriteLine($"Request data: FirstName={request.FirstName}, LastName={request.LastName}, Email={request.Email}, TeacherId={request.TeacherId}, Username={request.Username}");
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
                if (await _context.Teachers.AnyAsync(t => t.Email == request.Email))
                {
                    Console.WriteLine($"ERROR: Email {request.Email} already exists");
                    return BadRequest(new { message = "Email already exists. Please use a different email address." });
                }

                // Check if teacher ID already exists
                if (await _context.Teachers.AnyAsync(t => t.TeacherId == request.TeacherId))
                {
                    Console.WriteLine($"ERROR: Teacher ID {request.TeacherId} already exists");
                    return BadRequest(new { message = "Teacher ID already exists. Please check the teacher ID." });
                }

                // Validate date of birth (must be in the past and reasonable age)
                if (request.DateOfBirth.HasValue)
                {
                    var today = DateTime.Today;
                    if (request.DateOfBirth.Value > today)
                    {
                        Console.WriteLine($"ERROR: Date of birth {request.DateOfBirth.Value} is in the future");
                        return BadRequest(new { message = "Date of birth cannot be in the future. Please enter a valid date." });
                    }
                    
                    // Calculate age
                    var age = today.Year - request.DateOfBirth.Value.Year;
                    if (request.DateOfBirth.Value > today.AddYears(-age)) age--;
                    
                    Console.WriteLine($"Calculated age: {age}");
                    
                    if (age < 22 || age > 100)
                    {
                        Console.WriteLine($"ERROR: Age {age} is not valid (must be 22-100)");
                        return BadRequest(new { message = "Teacher must be at least 22 years old. Please check the date of birth." });
                    }
                }

                // Create user
                var user = new User
                {
                    Username = request.Username,
                    PasswordHash = _authService.HashPassword(request.Password),
                    Role = "Teacher",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create teacher
                var teacher = new Teacher
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    TeacherId = request.TeacherId,
                    PhoneNumber = request.PhoneNumber,
                    DateOfBirth = request.DateOfBirth,
                    Address = request.Address,
                    Department = request.Department,
                    Specialization = request.Specialization,
                    CreatedAt = DateTime.UtcNow,
                    UserId = user.Id
                };

                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();

                Console.WriteLine($"SUCCESS: Teacher created with ID {teacher.Id}");

                var teacherDto = new TeacherDTO
                {
                    Id = teacher.Id,
                    FirstName = teacher.FirstName,
                    LastName = teacher.LastName,
                    Email = teacher.Email,
                    TeacherId = teacher.TeacherId,
                    PhoneNumber = teacher.PhoneNumber,
                    DateOfBirth = teacher.DateOfBirth,
                    Address = teacher.Address,
                    Department = teacher.Department,
                    Specialization = teacher.Specialization,
                    CreatedAt = teacher.CreatedAt,
                    Username = user.Username
                };

                return CreatedAtAction(nameof(GetTeacher), new { id = teacher.Id }, teacherDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Exception occurred: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while creating the teacher. Please try again." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTeacher(int id, [FromBody] UpdateTeacherRequest request)
        {
            Console.WriteLine($"=== UpdateTeacher API Called ===");
            Console.WriteLine($"Request data: FirstName={request.FirstName}, LastName={request.LastName}, Email={request.Email}, TeacherId={request.TeacherId}");
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

                var teacher = await _context.Teachers
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (teacher == null)
                    return NotFound(new { message = "Teacher not found" });

                // Check if email already exists (excluding current teacher)
                if (await _context.Teachers.AnyAsync(t => t.Email == request.Email && t.Id != id))
                    return BadRequest(new { message = "Email already exists. Please use a different email address." });

                // Check if teacher ID already exists (excluding current teacher)
                if (await _context.Teachers.AnyAsync(t => t.TeacherId == request.TeacherId && t.Id != id))
                    return BadRequest(new { message = "Teacher ID already exists. Please check the teacher ID." });

                // Check if username already exists (excluding current teacher)
                if (await _context.Users.AnyAsync(u => u.Username == request.Username && u.Id != teacher.UserId))
                    return BadRequest(new { message = "Username already exists. Please choose a different username." });

                // Validate date of birth (must be in the past and reasonable age)
                if (request.DateOfBirth.HasValue)
                {
                    var today = DateTime.Today;
                    if (request.DateOfBirth.Value > today)
                    {
                        Console.WriteLine($"ERROR: Date of birth {request.DateOfBirth.Value} is in the future");
                        return BadRequest(new { message = "Date of birth cannot be in the future. Please enter a valid date." });
                    }
                    
                    // Calculate age
                    var age = today.Year - request.DateOfBirth.Value.Year;
                    if (request.DateOfBirth.Value > today.AddYears(-age)) age--;
                    
                    Console.WriteLine($"Calculated age: {age}");
                    
                    if (age < 22 || age > 100)
                    {
                        Console.WriteLine($"ERROR: Age {age} is not valid (must be 22-100)");
                        return BadRequest(new { message = "Teacher must be at least 22 years old. Please check the date of birth." });
                    }
                }

                teacher.FirstName = request.FirstName;
                teacher.LastName = request.LastName;
                teacher.Email = request.Email;
                teacher.TeacherId = request.TeacherId;
                teacher.PhoneNumber = request.PhoneNumber;
                teacher.DateOfBirth = request.DateOfBirth;
                teacher.Address = request.Address;
                teacher.Department = request.Department;
                teacher.Specialization = request.Specialization;

                // Update user information
                teacher.User.Username = request.Username;
                if (!string.IsNullOrEmpty(request.Password))
                {
                    teacher.User.PasswordHash = _authService.HashPassword(request.Password);
                }

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating teacher: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while updating the teacher. Please try again." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
                return NotFound();

            _context.Users.Remove(teacher.User);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("my-courses")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<IEnumerable<TeacherCourseDTO>>> GetMyCourses()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
                return NotFound();

            var courses = await _context.Courses
                .Where(c => c.TeacherId == teacher.Id)
                .Select(c => new TeacherCourseDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    Description = c.Description,
                    Credits = c.Credits,
                    CreatedAt = c.CreatedAt,
                    EnrolledStudentsCount = c.StudentCourses.Count
                })
                .ToListAsync();

            return Ok(courses);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<TeacherDTO>> GetMe()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
                return NotFound(new { message = "Teacher profile not found" });

            var dto = new TeacherDTO
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Email = teacher.Email,
                TeacherId = teacher.TeacherId,
                PhoneNumber = teacher.PhoneNumber,
                DateOfBirth = teacher.DateOfBirth,
                Address = teacher.Address,
                Department = teacher.Department,
                Specialization = teacher.Specialization,
                CreatedAt = teacher.CreatedAt,
                Username = teacher.User.Username
            };

            return Ok(dto);
        }
    }
}
