namespace StudioScheduler.Extensions
{
    public static class DateTimeExtensions
    {
        public static (DateTime StartOfWeek, DateTime EndOfWeek) GetWeekRange(this DateTime date)
        {
            // Get the current culture to determine the first day of the week
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek; // Typically Monday

            // Calculate the start of the week
            int diff = (7 + (date.DayOfWeek - firstDayOfWeek)) % 7;
            DateTime startOfWeek = date.AddDays(-diff).Date;

            // End of the week
            DateTime endOfWeek = startOfWeek.AddDays(6);

            return (startOfWeek, endOfWeek);
        }

        public static List<DateTime> GenerateHourlySlots(this DateTime date, int firstHour, int lastHour)
        {
            var slots = new List<DateTime>();

            for (int hour = firstHour; hour < lastHour; hour++)
            {
                slots.Add(date.Date.AddHours(hour));
            }

            return slots;
        }
    }
}
