using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Event
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Venue { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public ICollection<Sector> Sectors { get; set; } = new List<Sector>();
    }
}
