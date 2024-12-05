namespace StudioScheduler.Models
{
    public class Scheduler
    {
        public int ScheduleID { get; set; }
        public int RoomID { get; set; }
        public string BandName { get; set; }
        public string ContactName { get; set; }
        public string MobileNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Navigation property
        public Room Room { get; set; }
    }
}
