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
                
            }
        );
    }
}