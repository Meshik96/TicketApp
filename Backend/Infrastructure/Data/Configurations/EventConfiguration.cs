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
            Status = "Active",
            ImageUrl = "/images/evento1.jpg"
        });

        builder.HasData(new Event
        {
            Id = 2,
            Name = "Fin de Año Varela",
            EventDate = new DateTime(2026, 12, 30, 21, 0, 0),
            Venue = "Estadio UNAJ",
            Status = "Active",
            ImageUrl = "/images/evento2.jpg"
        });

        builder.HasData(new Event
        {
            Id = 3,
            Name = "Taller de Backend C#",
            EventDate = new DateTime(2026, 05, 20, 18, 30, 0),
            Venue = "Sede Central UNAJ",
            Status = "Active",
            ImageUrl = "/images/evento3.jpg"
        });

        builder.HasData(new Event
        {
            Id = 4,
            Name = "Feria Gastronómica Varela",
            EventDate = new DateTime(2026, 06, 12, 12, 0, 0),
            Venue = "Parque Recreativo Municipal",
            Status = "Active",
            ImageUrl = "/images/evento4.jpg"
        });

        builder.HasData(new Event
        {
            Id = 5,
            Name = "Charla: Futuro de la IA",
            EventDate = new DateTime(2026, 07, 05, 10, 0, 0),
            Venue = "Auditorio Berazategui",
            Status = "Active",
            ImageUrl = "/images/evento5.jpg"
        });

        builder.HasData(new Event
        {
            Id = 6,
            Name = "Torneo Dota 2 Regional",
            EventDate = new DateTime(2026, 08, 22, 14, 0, 0),
            Venue = "Centro de Actividades Roberto De Vicenzo",
            Status = "Active",
            ImageUrl = "/images/evento6.jpg"
        });

        builder.HasData(new Event
        {
            Id = 7,
            Name = "Clase Magistral de Japonés",
            EventDate = new DateTime(2026, 09, 10, 17, 0, 0),
            Venue = "Biblioteca UNAJ",
            Status = "Active",
            ImageUrl = "/images/evento7.jpg"
        });

        builder.HasData(new Event
        {
            Id = 8,
            Name = "Expo Empleo Informática",
            EventDate = new DateTime(2026, 10, 15, 09, 0, 0),
            Venue = "Gimnasio UNAJ",
            Status = "Active",
            ImageUrl = "/images/evento8.jpg"
        });
    }
}