using Microsoft.EntityFrameworkCore;
using SIMS_APDP.Models;

namespace SIMS_APDP.Data
{
    public class SIMSContext : DbContext
    {
        public SIMSContext(DbContextOptions<SIMSContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Configure Student entity
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StudentId).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.StudentId).IsUnique();
                
                // One-to-one relationship with User
                entity.HasOne(e => e.User)
                      .WithOne(e => e.Student)
                      .HasForeignKey<Student>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Teacher entity
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TeacherId).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Department).HasMaxLength(200);
                entity.Property(e => e.Specialization).HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.TeacherId).IsUnique();
                
                // One-to-one relationship with User
                entity.HasOne(e => e.User)
                      .WithOne(e => e.Teacher)
                      .HasForeignKey<Teacher>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Course entity
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Code).IsUnique();
                
                // Many-to-one relationship with Teacher
                entity.HasOne(e => e.Teacher)
                      .WithMany(e => e.Courses)
                      .HasForeignKey(e => e.TeacherId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure StudentCourse entity (many-to-many)
            modelBuilder.Entity<StudentCourse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LetterGrade).HasMaxLength(2);
                entity.Property(e => e.Grade).HasPrecision(5, 2); // Allow grades like 95.50
                
                // Many-to-many relationship
                entity.HasOne(e => e.Student)
                      .WithMany(e => e.StudentCourses)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Course)
                      .WithMany(e => e.StudentCourses)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Ensure unique enrollment
                entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
            });
        }
    }
}
