namespace StudioScheduler.Models
{
    public class Room
    {
        public int RoomID { get; set; }
        public int StudioID { get; set; }
        public string RoomName { get; set; }

        // Navigation property
        public Studio Studio { get; set; }
        public ICollection<Scheduler> Schedules { get; set; }
    }
}
