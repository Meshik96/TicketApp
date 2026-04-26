using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Persistence.Seats
{
    public interface ISeatCommands
    {
        Task ConfirmSeatsPurchaseAsync(int userId, List<Guid> seatIds);
        Task DeleteExpiredReservationsAsync();
    }
}
