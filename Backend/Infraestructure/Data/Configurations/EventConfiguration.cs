using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasData(new Event
        {
            Id = 1,
            Name = "Concierto de Rock UNAJ",
            EventDate = new DateTime(2026, 11, 15, 21, 0, 0),
            Venue = "Estadio Municipal de Berazategui",
            Status = "Active"
        });
    }
}