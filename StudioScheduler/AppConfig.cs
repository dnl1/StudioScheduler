using StudioScheduler.Interfaces;

namespace StudioScheduler
{
    public class AppConfig : IAppConfig
    {
        public int FirstHour { get; set; }
        public int LastHour { get; set; }
    }
}
