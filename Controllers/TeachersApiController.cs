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
    public class TeachersApiController : ControllerBase
    {
        private readonly SIMSContext _context;
        private readonly AuthService _authService;

        public TeachersApiController(SIMSContext context, AuthService authService)
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
                    Address = t.Address,
                    Department = t.Department,
                    Specialization = t.Specialization,
                    CreatedAt = t.CreatedAt,
                    Username = t.User.Username
                })
                .ToListAsync();

            return Ok(teachers);
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
    }
}
