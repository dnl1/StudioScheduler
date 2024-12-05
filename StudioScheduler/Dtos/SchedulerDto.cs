namespace StudioScheduler.Dtos
{
    public class SchedulerDto
    {
        public int ScheduleID { get; set; }
        public string BandName { get; set; }
        public string ContactName { get; set; }
        public string MobileNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public RoomDto Room { get; set; }

    }
}