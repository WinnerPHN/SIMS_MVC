using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SIMS_APDP.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Teacher")]
    public class TeacherController : Controller
    {
        public IActionResult Dashboard()
        {
            // Get user info from claims
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            ViewBag.Username = username;
            ViewBag.UserId = userId;
            ViewBag.Role = role;
            
            return View();
        }
    }
}
