namespace Application.DTOs.Seats;

public class SeatStateResponse
{
    public Guid Id { get; set; }
    public int SectorId { get; set; }
    public string SectorName { get; set; } = string.Empty;
    public string RowIdentifier { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public string Status { get; set; } = string.Empty; // Available, Reserved, Sold
    public decimal Price { get; set; }
    public int SectorGridX { get; set; }
    public int SectorGridY { get; set; }
}
