using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Reservation
    {
        public Guid Id { get; set; } // UUID
        public int UserId { get; set; } // FK
        public Guid SeatId { get; set; } // FK
        public string Status { get; set; } = "Pending"; // Pending, Paid, Expired
        public DateTime ReservedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Seat Seat { get; set; } = null!;
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
