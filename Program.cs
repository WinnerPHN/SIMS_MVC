using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SIMS_APDP.Data;
using SIMS_APDP.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework
builder.Services.AddDbContext<SIMSContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "AuthCookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        NameClaimType = System.Security.Claims.ClaimTypes.Name
    };
    
    // Handle JWT token from Authorization header and cookies
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Check Authorization header first
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Bearer "))
                {
                    context.Token = authHeader.Substring("Bearer ".Length);
                    return Task.CompletedTask;
                }
            }
            
            // Then check cookies
            if (context.Request.Cookies.ContainsKey("JWTToken"))
            {
                context.Token = context.Request.Cookies["JWTToken"];
            }
            return Task.CompletedTask;
        }
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// Add Services
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Custom middleware to handle JWT token from cookies for MVC views
app.Use(async (context, next) =>
{
    // Skip for API routes
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        await next();
        return;
    }

    // Handle JWT token from cookies for MVC views
    if (context.Request.Cookies.ContainsKey("JWTToken"))
    {
        var token = context.Request.Cookies["JWTToken"];
        if (!string.IsNullOrEmpty(token))
        {
            // Set Authorization header for JWT Bearer authentication
            context.Request.Headers["Authorization"] = $"Bearer {token}";
        }
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Apply migrations and create default admin user
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<SIMSContext>();
        await context.Database.MigrateAsync();

        var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
        await authService.CreateDefaultAdminAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Startup DB initialization error: {ex.Message}");
        // Do not crash the app on migration/seed failure; admin/setup endpoints can be used later
    }
}

app.Run();
