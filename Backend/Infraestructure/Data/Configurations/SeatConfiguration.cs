using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Security.Cryptography;
using System.Text;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        var seats = new List<Seat>();

        // Definición de la configuración de todos los sectores: { Id, Filas, AsientosPorFila }
        var allSectorConfigs = new[]
        {
        // Evento 1
        new { Id = 1, Rows = 5, PerRow = 10 },  // VIP
        new { Id = 2, Rows = 5, PerRow = 10 },  // General
        // Evento 2
        new { Id = 3, Rows = 10, PerRow = 15 },
        new { Id = 4, Rows = 10, PerRow = 15 },
        new { Id = 5, Rows = 10, PerRow = 15 },
        new { Id = 6, Rows = 10, PerRow = 15 },
        new { Id = 7, Rows = 10, PerRow = 15 },
        new { Id = 8, Rows = 10, PerRow = 15 },
        // Evento 3
        new { Id = 9, Rows = 2, PerRow = 10 },
        new { Id = 10, Rows = 3, PerRow = 10 },
        // Evento 4
        new { Id = 11, Rows = 5, PerRow = 20 },
        new { Id = 12, Rows = 10, PerRow = 20 },
        // Evento 5
        new { Id = 13, Rows = 4, PerRow = 10 },
        new { Id = 14, Rows = 6, PerRow = 10 },
        new { Id = 15, Rows = 4, PerRow = 10 },
        // Evento 6
        new { Id = 16, Rows = 2, PerRow = 10 },
        new { Id = 17, Rows = 8, PerRow = 15 },
        new { Id = 18, Rows = 8, PerRow = 15 },
        // Evento 7
        new { Id = 19, Rows = 8, PerRow = 10 },
        // Evento 8
        new { Id = 20, Rows = 3, PerRow = 10 },
        new { Id = 21, Rows = 7, PerRow = 10 },
        new { Id = 22, Rows = 7, PerRow = 10 }
    };

        foreach (var config in allSectorConfigs)
        {
            for (int r = 1; r <= config.Rows; r++)
            {
                // Genera etiqueta de fila (A, B, C...)
                string rowLabel = ((char)('A' + (r - 1))).ToString();

                for (int s = 1; s <= config.PerRow; s++)
                {
                    seats.Add(new Seat
                    {
                        // ID único basado en Sector, Fila y Asiento para evitar colisiones de GUID
                        Id = CreateGuidFromName($"SEC{config.Id}-R{rowLabel}-S{s}"),
                        SectorId = config.Id,
                        RowIdentifier = rowLabel,
                        SeatNumber = s,
                        Status = "Available",
                        Version = 1
                    });
                }
            }
        }

        builder.HasData(seats);
    }

    // Genera un GUID consistente basado en un string de entrada
    private Guid CreateGuidFromName(string name)
    {
        byte[] hash = SHA1.HashData(Encoding.UTF8.GetBytes(name));
        byte[] guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        return new Guid(guidBytes);
    }
}