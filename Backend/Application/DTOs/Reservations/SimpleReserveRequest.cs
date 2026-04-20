namespace Application.DTOs.Reservations;

public class SimpleReserveRequest
{
    public int UserId { get; set; }
    public Guid SeatId { get; set; }
}
