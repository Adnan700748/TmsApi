using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Identity;

namespace TmsApi.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.HasMany(c => c.Enrollments)
            .WithOne(e => e.Course)
            .HasForeignKey(e => e.CourseId);

        // Instructor ownership
        builder.Property(c => c.InstructorId)
            .HasMaxLength(450);

        builder.HasOne<TmsUser>()
            .WithMany()
            .HasForeignKey(c => c.InstructorId)
            .HasPrincipalKey(u => u.Id)
            .OnDelete(DeleteBehavior.SetNull);
    }
}