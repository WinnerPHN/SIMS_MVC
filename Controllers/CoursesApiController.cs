using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS_APDP.Data;
using SIMS_APDP.DTOs;
using SIMS_APDP.Models;

namespace SIMS_APDP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CoursesApiController : ControllerBase
    {
        private readonly SIMSContext _context;

        public CoursesApiController(SIMSContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CourseDTO>>> GetCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Teacher)
                .Select(c => new CourseDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    Description = c.Description,
                    Credits = c.Credits,
                    TeacherId = c.TeacherId,
                    TeacherName = c.Teacher != null ? $"{c.Teacher.FirstName} {c.Teacher.LastName}" : null,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(courses);
        }

        [HttpGet("enrollments")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<StudentCourseDTO>>> GetEnrollments([FromQuery] int? courseId = null)
        {
            var query = _context.StudentCourses
                .Include(sc => sc.Student)
                .Include(sc => sc.Course)
                .AsQueryable();

            if (courseId.HasValue)
                query = query.Where(sc => sc.CourseId == courseId.Value);

            var enrollments = await query
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

            return Ok(enrollments);
        }
    }
}
