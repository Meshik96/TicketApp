using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Sector
    {
        public int Id { get; set; }
        public int EventId { get; set; } // FK
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public int GridX { get; set; }
        public int GridY { get; set; }
        public string Orientation { get; set; }
        public Event Event { get; set; } = null!;
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}
