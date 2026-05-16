using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Reservations
{
    public class UserReservationResponse
    {
        public Guid BookingId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string VenueName { get; set; }
        public string SectorName { get; set; }
        public string RowIdentifier { get; set; }
        public int SeatNumber { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
    }
}
