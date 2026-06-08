using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Infrastructure.Persistence.Configurations;

public class ProjectEntityTypeConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(project => project.Id);

        builder.Property(project => project.Id)
            .ValueGeneratedNever();

        builder.Property(project => project.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(project => project.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(project => project.ReleaseDate)
            .IsRequired();

        builder.Property(project => project.Version)
            .IsRequired();

        builder.Property(project => project.AuthorId)
            .IsRequired();

        builder.Property(project => project.IsInDevelopment)
            .IsRequired();
    }
}
