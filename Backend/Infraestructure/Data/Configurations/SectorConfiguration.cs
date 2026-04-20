using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Data.Configurations;

public class SectorConfiguration : IEntityTypeConfiguration<Sector>
{
    public void Configure(EntityTypeBuilder<Sector> builder)
    {
        builder.HasData(
            new Sector
            {
                Id = 1,
                EventId = 1,
                Name = "VIP",
                Price = 150.00m,
                Capacity = 50,
                GridX = 0,
                GridY = 0,
                Orientation = "Horizontal"
            },
            new Sector
            {
                Id = 2,
                EventId = 1,
                Name = "General",
                Price = 75.00m,
                Capacity = 50,
                GridX = 0,
                GridY = 1,
                Orientation = "Horizontal"
            }
        );
    }
}
