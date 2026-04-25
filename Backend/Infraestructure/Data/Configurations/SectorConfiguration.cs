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
            },
            new Sector { Id = 3, EventId = 2, Name = "VIP Lateral Izquierdo", Price = 150m, Capacity = 150, GridX = -1, GridY = 0, Orientation = "Horizontal" },
            new Sector { Id = 4, EventId = 2, Name = "VIP Central", Price = 200m, Capacity = 150, GridX = 0, GridY = 0, Orientation = "Horizontal" },
            new Sector { Id = 5, EventId = 2, Name = "VIP Lateral Derecho", Price = 150m, Capacity = 150, GridX = 1, GridY = 0, Orientation = "Horizontal" },
            new Sector { Id = 6, EventId = 2, Name = "General Lateral Izquierdo", Price = 75m, Capacity = 150, GridX = -1, GridY = 1, Orientation = "Horizontal" },
            new Sector { Id = 7, EventId = 2, Name = "General Central", Price = 100m, Capacity = 150, GridX = 0, GridY = 1, Orientation = "Horizontal" },
            new Sector { Id = 8, EventId = 2, Name = "General Lateral Derecho", Price = 75m, Capacity = 150, GridX = 1, GridY = 1, Orientation = "Horizontal" }
        );
    }
}
