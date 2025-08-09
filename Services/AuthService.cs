using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SIMS_APDP.Data;
using SIMS_APDP.Models;

namespace SIMS_APDP.Services
{
    public class AuthService
    {
        private readonly SIMSContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(SIMSContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            Console.WriteLine($"Authenticating user: {username}");
            
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                Console.WriteLine($"User not found: {username}");
                return null;
            }

            Console.WriteLine($"User found: {user.Username} (ID: {user.Id}, Role: {user.Role})");

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                Console.WriteLine($"Password verification failed for user: {username}");
                return null;
            }

            Console.WriteLine($"Authentication successful for user: {username}");
            return user;
        }

        public string GenerateJwtToken(User user)
        {
            Console.WriteLine($"Generating JWT token for user: {user.Username}");
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationInMinutes"])),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            Console.WriteLine($"JWT token generated successfully for user: {user.Username}");
            return tokenString;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<bool> CreateDefaultAdminAsync()
        {
            Console.WriteLine("Checking if admin user exists...");
            
            if (await _context.Users.AnyAsync(u => u.Username == "admin"))
            {
                Console.WriteLine("Admin user already exists");
                return false;
            }

            Console.WriteLine("Creating admin user...");
            
            var adminUser = new User
            {
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"Admin user created successfully: {adminUser.Username} (ID: {adminUser.Id})");
            return true;
        }
    }
}
