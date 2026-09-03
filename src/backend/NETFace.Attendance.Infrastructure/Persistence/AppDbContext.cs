using Microsoft.EntityFrameworkCore;
using NETFace.Attendance.Domain.Entities;

namespace NETFace.Attendance.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
                emb.Property(fe => fe.Vector).IsRequired();
            });
        });
    }
}
