using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SIMS_APDP.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            Console.WriteLine("Admin Dashboard called");
            Console.WriteLine($"IsAuthenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"Authentication Type: {User.Identity?.AuthenticationType}");
            Console.WriteLine($"Username: {User.Identity?.Name}");
            
            // Get user info from claims
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            Console.WriteLine($"Username from claims: {username}");
            Console.WriteLine($"UserId from claims: {userId}");
            Console.WriteLine($"Role from claims: {role}");
            
            ViewBag.Username = username;
            ViewBag.UserId = userId;
            ViewBag.Role = role;
            
            Console.WriteLine("Returning Admin Dashboard view");
            return View();
        }
    }
}
