using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Seats
{
    public class BuyRequest
    {
        public int UserId { get; set; }
        public List<Guid> SeatIds { get; set; } = new List<Guid>();
     }
}
