namespace Application.DTOs.Reservations;

public class ReservationResponse
{
    public Guid ReservationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string SeatStatus { get; set; } = string.Empty;
}
