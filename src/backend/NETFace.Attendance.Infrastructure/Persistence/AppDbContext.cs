using Microsoft.EntityFrameworkCore;
using NETFace.Attendance.Domain.Entities;

namespace NETFace.Attendance.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<AttendanceSession> AttendanceSessions { get; set; } = null!;
    public DbSet<RecognitionLog> RecognitionLogs { get; set; } = null!;
    public DbSet<EnrollmentLog> EnrollmentLogs => Set<EnrollmentLog>();
    public DbSet<SystemSetting> SystemSettings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SystemSetting>(b =>
        {
            b.HasKey(s => s.Key);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeCode)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.HasIndex(e => e.EmployeeCode)
                  .IsUnique();

            entity.Property(e => e.FullName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.OwnsMany(e => e.FaceEmbeddings, emb =>
            {
                emb.HasKey(fe => fe.Id);
                emb.Property(fe => fe.Id).ValueGeneratedNever();
                emb.Property(fe => fe.Vector).IsRequired();
            });
        });

        modelBuilder.Entity<AttendanceSession>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.DepartmentName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.OwnsMany(s => s.Entries, entry =>
            {
                entry.HasKey(e => e.Id);
                entry.Property(e => e.Id).ValueGeneratedNever();

                entry.Property(e => e.EmployeeCode)
                     .IsRequired()
                     .HasMaxLength(50);

                entry.Property(e => e.EmployeeName)
                     .IsRequired()
                     .HasMaxLength(200);

                entry.Property(e => e.MarkedAt);
            });
        });

        modelBuilder.Entity<RecognitionLog>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.MatchedEmployeeCode)
                  .HasMaxLength(50);

            entity.Property(r => r.ErrorMessage)
                  .HasMaxLength(500);

            entity.Property(r => r.AttemptedAt)
                  .IsRequired();
        });

        modelBuilder.Entity<EnrollmentLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasConversion<string>();
            entity.Property(e => e.PerformedBy).HasMaxLength(100);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
        });
    }
}

