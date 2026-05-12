using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities
{
    public class Seat
    {
        public Guid Id { get; set; } // UUID 
        public int SectorId { get; set; } // FK
        public string RowIdentifier { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public string Status { get; set; } = "Available"; // Available, Reserved, Sold

        // Concurrency control
        [ConcurrencyCheck]
        public int Version { get; set; }

        // Navigation properties
        public Sector Sector { get; set; } = null!;
        public Reservation? Reservation { get; set; }
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
