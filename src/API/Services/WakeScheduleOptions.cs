namespace API.Services {
    public class WakeScheduleOptions {
        public const string SectionName = "WakeSchedule";

        /// <summary>
        /// Whether the wake schedule service is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// How often to check for schedules (in seconds)
        /// </summary>
        public int CheckIntervalSeconds { get; set; } = 30;
    }
}
