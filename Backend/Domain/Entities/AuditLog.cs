using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } // UUID
        public int? UserId { get; set; } // FK 
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty; // JSON
        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }
    }
}
