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
                GridY = 0
            },
            new Sector
            {
                Id = 2,
                EventId = 1,
                Name = "General",
                Price = 75.00m,
                Capacity = 50,
                GridX = 0,
                GridY = 1
            },
            new Sector { Id = 3, EventId = 2, Name = "VIP Lateral Izquierdo", Price = 150m, Capacity = 150, GridX = -1, GridY = 0},
            new Sector { Id = 4, EventId = 2, Name = "VIP Central", Price = 200m, Capacity = 150, GridX = 0, GridY = 0},
            new Sector { Id = 5, EventId = 2, Name = "VIP Lateral Derecho", Price = 150m, Capacity = 150, GridX = 1, GridY = 0},
            new Sector { Id = 6, EventId = 2, Name = "General Lateral Izquierdo", Price = 75m, Capacity = 150, GridX = -1, GridY = 1},
            new Sector { Id = 7, EventId = 2, Name = "General Central", Price = 100m, Capacity = 150, GridX = 0, GridY = 1},
            new Sector { Id = 8, EventId = 2, Name = "General Lateral Derecho", Price = 75m, Capacity = 150, GridX = 1, GridY = 1},

            // Evento 3: Taller de Backend C# (Aula pequeña - Distribución frontal)
            new Sector { Id = 9, EventId = 3, Name = "Filas Delanteras", Price = 50.00m, Capacity = 20, GridX = 0, GridY = 0 },
            new Sector { Id = 10, EventId = 3, Name = "Filas Posteriores", Price = 30.00m, Capacity = 30, GridX = 0, GridY = 1 },

            // Evento 4: Feria Gastronómica Varela (Anfiteatro al aire libre)
            new Sector { Id = 11, EventId = 4, Name = "Platea Baja", Price = 40.00m, Capacity = 100, GridX = 0, GridY = 0 },
            new Sector { Id = 12, EventId = 4, Name = "Graderías Generales", Price = 20.00m, Capacity = 200, GridX = 0, GridY = 1 },

            // Evento 5: Charla: Futuro de la IA (Auditorio Universitario - 3 sectores horizontales)
            new Sector { Id = 13, EventId = 5, Name = "Sector A (Izquierdo)", Price = 60.00m, Capacity = 40, GridX = -1, GridY = 0 },
            new Sector { Id = 14, EventId = 5, Name = "Sector B (Central)", Price = 80.00m, Capacity = 60, GridX = 0, GridY = 0 },
            new Sector { Id = 15, EventId = 5, Name = "Sector C (Derecho)", Price = 60.00m, Capacity = 40, GridX = 1, GridY = 0 },

            // Evento 6: Torneo Dota 2 Regional (Estadio de E-sports - VIP adelante, General atrás)
            new Sector { Id = 16, EventId = 6, Name = "Palcos Pro-Player", Price = 120.00m, Capacity = 20, GridX = 0, GridY = 0 },
            new Sector { Id = 17, EventId = 6, Name = "Tribuna Lateral Izquierda", Price = 45.00m, Capacity = 120, GridX = -1, GridY = 1 },
            new Sector { Id = 18, EventId = 6, Name = "Tribuna Lateral Derecha", Price = 45.00m, Capacity = 120, GridX = 1, GridY = 1 },

            // Evento 7: Clase Magistral de Japonés (Salón de usos múltiples - Sector único)
            new Sector { Id = 19, EventId = 7, Name = "Platea Única", Price = 25.00m, Capacity = 80, GridX = 0, GridY = 0 },

            // Evento 8: Expo Empleo Informática (Auditorio de conferencias)
            new Sector { Id = 20, EventId = 8, Name = "Sector Ejecutivo (Frontal)", Price = 100.00m, Capacity = 30, GridX = 0, GridY = 0 },
            new Sector { Id = 21, EventId = 8, Name = "Platea General Derecha", Price = 40.00m, Capacity = 70, GridX = 1, GridY = 1 },
            new Sector { Id = 22, EventId = 8, Name = "Platea General Izquierda", Price = 40.00m, Capacity = 70, GridX = -1, GridY = 1 }
        );
    }
}
