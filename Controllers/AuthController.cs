using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SIMS_APDP.Data;
using SIMS_APDP.DTOs;
using SIMS_APDP.Models;
using SIMS_APDP.Services;
using System.Security.Claims;

namespace SIMS_APDP.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly SIMSContext _context;

        public AuthController(AuthService authService, SIMSContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Always show the login page first, even if a JWT cookie exists
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RedirectAfterLogin()
        {
            Console.WriteLine($"RedirectAfterLogin called. IsAuthenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"Authentication Type: {User.Identity?.AuthenticationType}");
            Console.WriteLine($"Username: {User.Identity?.Name}");
            
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            Console.WriteLine($"Role: {role}");
            
            if (User.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(role))
            {
                switch (role)
                {
                    case "Admin":
                        return RedirectToAction("Dashboard", "Admin");
                    case "Student":
                        return RedirectToAction("Dashboard", "Student");
                    case "Teacher":
                        return RedirectToAction("Dashboard", "Teacher");
                    default:
                        Console.WriteLine($"Unknown role: {role}");
                        return RedirectToAction("Login");
                }
            }
            else
            {
                Console.WriteLine("User not authenticated or role not found");
                return RedirectToAction("Login");
            }
        }

        [HttpGet("check-auth")]
        public ActionResult CheckAuth()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Ok(new
                {
                    isAuthenticated = true,
                    username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
                    role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
                    userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                });
            }
            else
            {
                return Unauthorized(new { isAuthenticated = false });
            }
        }
    }

    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly SIMSContext _context;
        private readonly IConfiguration _configuration;

        public AuthApiController(AuthService authService, SIMSContext context, IConfiguration configuration)
        {
            _authService = authService;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                Console.WriteLine($"Login attempt for username: {request.Username}");
                
                if (request == null)
                {
                    Console.WriteLine("Login failed: Request is null");
                    return BadRequest(new { message = "Invalid request data" });
                }
                
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    Console.WriteLine("Login failed: Username or password is empty");
                    return BadRequest(new { message = "Username and password are required" });
                }
                
                var user = await _authService.AuthenticateUserAsync(request.Username, request.Password);
                
                if (user == null)
                {
                    Console.WriteLine($"Login failed for username: {request.Username}");
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                Console.WriteLine($"Login successful for user: {user.Username} (ID: {user.Id}, Role: {user.Role})");
                
                var token = _authService.GenerateJwtToken(user);
                
                // Set JWT token as cookie for MVC authentication
                Response.Cookies.Append("JWTToken", token, new CookieOptions
                {
                    HttpOnly = false, // Allow JavaScript to read
                    Secure = false, // Allow HTTP for localhost
                    SameSite = SameSiteMode.Lax, // Less restrictive for localhost
                    Expires = DateTime.UtcNow.AddMinutes(60)
                });
                
                return Ok(new AuthResponse
                {
                    Token = token,
                    Username = user.Username,
                    Role = user.Role,
                    UserId = user.Id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An error occurred during login. Please try again." });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                Console.WriteLine($"Registration attempt for username: {request.Username}, email: {request.Email}");

                // 1) Validate model
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    Console.WriteLine($"Validation errors: {System.Text.Json.JsonSerializer.Serialize(errors)}");
                    return BadRequest(new { message = "Validation failed", errors });
                }

                // 2) Business validations with field-level error shapes
                if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                {
                    return BadRequest(new { message = "Username already exists", errors = new { Username = new[] { "Username already exists. Please choose a different username." } } });
                }
                if (await _context.Students.AnyAsync(s => s.Email == request.Email))
                {
                    return BadRequest(new { message = "Email already exists", errors = new { Email = new[] { "Email already exists. Please use a different email address." } } });
                }

                var today = DateTime.Today;
                if (request.DateOfBirth > today)
                {
                    return BadRequest(new { message = "Date of birth cannot be in the future.", errors = new { DateOfBirth = new[] { "Date of birth cannot be in the future." } } });
                }
                var age = today.Year - request.DateOfBirth.Year;
                if (request.DateOfBirth > today.AddYears(-age)) age--;
                if (age < 18 || age > 100)
                {
                    return BadRequest(new { message = "Student must be at least 18 years old.", errors = new { DateOfBirth = new[] { "Age must be between 18 and 100." } } });
                }

                // 3) Generate StudentId if missing and ensure uniqueness
                string generatedStudentId = string.IsNullOrWhiteSpace(request.StudentId)
                    ? $"S{DateTime.UtcNow:yyyyMMddHHmmssfff}"
                    : request.StudentId.Trim();
                if (await _context.Students.AnyAsync(s => s.StudentId == generatedStudentId))
                {
                    generatedStudentId = $"{generatedStudentId}-{Guid.NewGuid().ToString("N").Substring(4,4)}";
                }

                // 4) Persist user + student
                var user = new User
                {
                    Username = request.Username,
                    PasswordHash = _authService.HashPassword(request.Password),
                    Role = "Student",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var student = new Student
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    StudentId = generatedStudentId,
                    PhoneNumber = request.PhoneNumber,
                    DateOfBirth = request.DateOfBirth,
                    Address = request.Address,
                    CreatedAt = DateTime.UtcNow,
                    UserId = user.Id
                };
                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Registration successful for user: {user.Username} (ID: {user.Id})");

                var token = _authService.GenerateJwtToken(user);
                return Ok(new AuthResponse { Token = token, Username = user.Username, Role = user.Role, UserId = user.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during registration. Please try again." });
            }
        }

        [HttpPost("create-admin")]
        public async Task<ActionResult> CreateDefaultAdmin()
        {
            try
            {
                Console.WriteLine("Creating default admin user...");
                
                var created = await _authService.CreateDefaultAdminAsync();
                
                if (created)
                {
                    Console.WriteLine("Default admin created successfully");
                    return Ok(new { message = "Default admin created successfully. Username: admin, Password: admin123" });
                }
                
                Console.WriteLine("Admin user already exists");
                return BadRequest(new { message = "Admin user already exists" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create admin error: {ex.Message}");
                return StatusCode(500, new { message = "Database error: " + ex.Message });
            }
        }

        [HttpGet("test-db")]
        public async Task<ActionResult> TestDatabase()
        {
            try
            {
                var userCount = await _context.Users.CountAsync();
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                
                return Ok(new { 
                    message = "Database connection successful",
                    userCount = userCount,
                    adminExists = adminUser != null,
                    adminRole = adminUser?.Role
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database test error: {ex.Message}");
                return StatusCode(500, new { message = "Database connection failed: " + ex.Message });
            }
        }

        [HttpPost("setup-db")]
        public async Task<ActionResult> SetupDatabase()
        {
            try
            {
                Console.WriteLine("Setting up database...");
                
                // Ensure database is created
                await _context.Database.EnsureCreatedAsync();
                
                // Create admin user
                var created = await _authService.CreateDefaultAdminAsync();
                
                var userCount = await _context.Users.CountAsync();
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                
                return Ok(new { 
                    message = "Database setup completed",
                    databaseCreated = true,
                    adminCreated = created,
                    userCount = userCount,
                    adminExists = adminUser != null,
                    adminRole = adminUser?.Role
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database setup error: {ex.Message}");
                return StatusCode(500, new { message = "Database setup failed: " + ex.Message });
            }
        }

        [HttpGet("debug-admin")]
        public async Task<ActionResult> DebugAdmin()
        {
            try
            {
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                
                if (adminUser == null)
                {
                    return Ok(new { 
                        message = "Admin user not found",
                        adminExists = false,
                        suggestion = "Try calling /api/auth/setup-db to create admin user"
                    });
                }
                
                var passwordValid = BCrypt.Net.BCrypt.Verify("admin123", adminUser.PasswordHash);
                
                return Ok(new { 
                    message = "Admin user found",
                    adminExists = true,
                    adminId = adminUser.Id,
                    adminRole = adminUser.Role,
                    passwordValid = passwordValid,
                    passwordHash = adminUser.PasswordHash.Substring(0, 10) + "..."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug admin error: {ex.Message}");
                return StatusCode(500, new { message = "Debug failed: " + ex.Message });
            }
        }

        [HttpGet("test-token")]
        [Authorize]
        public ActionResult TestToken()
        {
            var user = User;
            return Ok(new
            {
                message = "Token is valid",
                username = user.Identity?.Name,
                claims = user.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList()
            });
        }

        [HttpGet("test-auth")]
        public ActionResult TestAuth()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Ok(new
                {
                    message = "User is authenticated",
                    username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
                    role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
                    userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    authenticationType = User.Identity.AuthenticationType
                });
            }
            else
            {
                return Unauthorized(new { message = "User is not authenticated" });
            }
        }

        [HttpGet("debug-identity")]
        public ActionResult DebugIdentity()
        {
            return Ok(new
            {
                isAuthenticated = User.Identity?.IsAuthenticated,
                authenticationType = User.Identity?.AuthenticationType,
                name = User.Identity?.Name,
                claims = User.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList()
            });
        }

        [HttpGet("debug-jwt")]
        public ActionResult DebugJwt()
        {
            var jwtToken = Request.Cookies["JWTToken"];
            return Ok(new
            {
                hasJwtToken = !string.IsNullOrEmpty(jwtToken),
                jwtTokenLength = jwtToken?.Length ?? 0,
                jwtTokenPreview = !string.IsNullOrEmpty(jwtToken) ? jwtToken.Substring(0, Math.Min(50, jwtToken.Length)) + "..." : "Not found"
            });
        }

        [HttpGet("validate-jwt")]
        public ActionResult ValidateJwt()
        {
            try
            {
                var jwtToken = Request.Cookies["JWTToken"];
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Ok(new { isValid = false, message = "No JWT token found in cookies" });
                }

                // Try to validate the token
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!);
                
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidAudience = _configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                var principal = tokenHandler.ValidateToken(jwtToken, validationParameters, out var validatedToken);
                
                return Ok(new
                {
                    isValid = true,
                    message = "JWT token is valid",
                    username = principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
                    role = principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
                    userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                });
            }
            catch (Exception ex)
            {
                return Ok(new { isValid = false, message = $"JWT token validation failed: {ex.Message}" });
            }
        }

        [HttpGet("check-auth")]
        public ActionResult CheckAuth()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Ok(new
                {
                    isAuthenticated = true,
                    username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
                    role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
                    userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                });
            }
            else
            {
                return Unauthorized(new { isAuthenticated = false });
            }
        }

        [HttpGet("debug-cookies")]
        public ActionResult DebugCookies()
        {
            var cookies = Request.Cookies.ToDictionary(c => c.Key, c => c.Value);
            return Ok(new
            {
                message = "Cookies debug",
                cookies = cookies,
                hasJwtToken = Request.Cookies.ContainsKey("JWTToken"),
                jwtToken = Request.Cookies.ContainsKey("JWTToken") ? Request.Cookies["JWTToken"].Substring(0, 20) + "..." : "Not found"
            });
        }

        [HttpPost("logout")]
        public ActionResult Logout()
        {
            Response.Cookies.Delete("JWTToken");
            return Ok(new { message = "Logged out successfully" });
        }
    }
}
