namespace StudioScheduler.Models
{
    public class Studio
    {
        public int StudioID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }

        // Navigation property
        public ICollection<Room> Rooms { get; set; }
    }
}
