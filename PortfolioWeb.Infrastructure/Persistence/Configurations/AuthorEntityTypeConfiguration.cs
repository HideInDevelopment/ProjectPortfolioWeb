using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Infrastructure.Persistence.Configurations;

public class AuthorEntityTypeConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("authors");

        builder.HasKey(author => author.Id);

        builder.Property(author => author.Id)
            .ValueGeneratedNever();

        builder.Property(author => author.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Navigation(author => author.Projects)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(author => author.Projects)
            .WithOne()
            .HasForeignKey(project => project.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
