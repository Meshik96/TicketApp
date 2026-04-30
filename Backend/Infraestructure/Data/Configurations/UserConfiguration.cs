using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasData(
            new User
            {
                Id = 1,
                Name = "Admin Test",
                Email = "admin@test.com",
                PasswordHash = "1234"
            },
            new User
            {
                Id = 2,
                Name = "John Code",
                Email = "john.code@example.com",
                PasswordHash = "hashedPassword123"
            },
            new User
            {
                Id = 3,
                Name = "Maria Garcia",
                Email = "maria.garcia@example.com",
                PasswordHash = "hashedPassword456"
            },
            new User
            {
                Id = 4,
                Name = "Carlos Rodriguez",
                Email = "carlos.rodriguez@example.com",
                PasswordHash = "hashedPassword789"
            },
            new User
            {
                Id = 5,
                Name = "Sofia Martinez",
                Email = "sofia.martinez@example.com",
                PasswordHash = "hashedPassword101112"
            }
        );
    }
}