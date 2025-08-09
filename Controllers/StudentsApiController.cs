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
    public class StudentsApiController : ControllerBase
    {
        private readonly SIMSContext _context;
        private readonly AuthService _authService;

        public StudentsApiController(SIMSContext context, AuthService authService)
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
                    EnrolledAt = sc.EnrolledAt,
                    Grade = sc.Grade,
                    LetterGrade = sc.LetterGrade
                })
                .ToListAsync();

            return Ok(courses);
        }
    }
}
