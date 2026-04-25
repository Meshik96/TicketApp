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

        string[] vipRows = { "A", "B", "C", "D", "E" };
        string[] generalRows = { "F", "G", "H", "I", "J" };

        // VIP Sector (SectorId: 1)
        foreach (var row in vipRows)
        {
            for (int s = 1; s <= 10; s++)
            {
                seats.Add(new Seat
                {
                    Id = CreateGuidFromName($"VIP-{row}-{s}"),
                    SectorId = 1,
                    RowIdentifier = row,
                    SeatNumber = s,
                    Status = "Available",
                    Version = 1
                });
            }
        }

        // General Sector (SectorId: 2)
        foreach (var row in generalRows)
        {
            for (int s = 1; s <= 10; s++)
            {
                seats.Add(new Seat
                {
                    Id = CreateGuidFromName($"GEN-{row}-{s}"),
                    SectorId = 2,
                    RowIdentifier = row,
                    SeatNumber = s,
                    Status = "Available",
                    Version = 1
                });
            }
        }
        int[] sectorIds = { 3, 4, 5, 6, 7, 8 };
        int seatsPerSector = 150;
        int seatsPerRow = 15;
        int totalRows = seatsPerSector / seatsPerRow; // 10 filas

        foreach (var sectorId in sectorIds)
        {
            for (int r = 1; r <= totalRows; r++)
            {
                string rowLabel = ((char)('A' + (r - 1))).ToString();

                for (int s = 1; s <= seatsPerRow; s++)
                {
                    seats.Add(new Seat
                    {
                        Id = CreateGuidFromName($"EV1-SEC{sectorId}-R{rowLabel}-S{s}"),
                        SectorId = sectorId,
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