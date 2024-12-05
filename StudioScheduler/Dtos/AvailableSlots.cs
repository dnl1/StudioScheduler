namespace StudioScheduler.Dtos
{
    public class Slot
    {
        public string Time { get; set; }
        public string Status { get; set; }
    }

    public class AvailableSlots
    {
        public string Date { get; set; }
        public IList<Slot> Slots { get; set; }
    }
}