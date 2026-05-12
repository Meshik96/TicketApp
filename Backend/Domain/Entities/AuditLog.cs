using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } // UUID
        public int? UserId { get; set; } // FK to User
        public Guid? SeatId { get; set; } // FK to Seat
        public Guid? ReservationId { get; set; } // FK to Reservation
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty; // JSON
        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }
        public Seat? Seat { get; set; }
        public Reservation? Reservation { get; set; }
    }
}
