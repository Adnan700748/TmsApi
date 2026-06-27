// TmsApi/Configurations/StudentConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.GPA)
            .IsRequired();

        builder.Property<DateTime>("LastUpdated");    

        builder.Property(s => s.Version)
            .IsRowVersion();

        // Soft delete filter
        builder.HasQueryFilter(s => s.IsActive);
        
        //  Index for performance
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => s.RegistrationNumber).IsUnique();
    }
}