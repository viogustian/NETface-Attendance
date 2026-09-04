using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NETFace.Attendance.Domain.Entities;

namespace NETFace.Attendance.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EmployeeCode)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(e => e.EmployeeCode)
            .IsUnique();

        builder.Property(e => e.ProfileDetails)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.AdminFlag)
            .IsRequired();

        builder.OwnsMany(e => e.FaceEmbeddings, fe => 
        {
            fe.ToTable("FaceEmbeddings");
            fe.HasKey(f => f.Id);
            fe.WithOwner().HasForeignKey(f => f.EmployeeId);
            
            fe.Property(f => f.Vector)
                .IsRequired();

            fe.Property(f => f.CapturedAt)
                .IsRequired();
        });
    }
}
