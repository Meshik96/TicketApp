namespace Application.DTOs.Reservations;

public class ReserveRequest
{
    public int UserId { get; set; }
    public Guid SeatId { get; set; }
}
