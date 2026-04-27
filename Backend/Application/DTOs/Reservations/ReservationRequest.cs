namespace Application.DTOs.Reservations;

public class ReservationRequest
{
    public int UserId { get; set; }
    public Guid SeatId { get; set; }
}
