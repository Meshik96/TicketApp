namespace Application.DTOs.Reservations;

public class ReservationResponse
{
    public Guid ReservationId { get; set; }
    public int UserId { get; set; }
    public Guid SeatId { get; set; }
    public string SeatStatus { get; set; } = string.Empty;
    public DateTime ReservedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
