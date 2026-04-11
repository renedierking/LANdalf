using System.ComponentModel.DataAnnotations;

namespace API.Models {
    public class WakeSchedule {
        public int Id { get; set; }

        [Required]
        public int PcDeviceId { get; set; }

        public PcDevice? PcDevice { get; set; }

        /// <summary>
        /// Time of day in UTC when the device should wake (HH:mm format)
        /// </summary>
        [Required]
        public TimeOnly ScheduledTime { get; set; }

        /// <summary>
        /// Days of week when schedule is active (0=Sunday, 6=Saturday).
        /// Null or empty = one-time schedule, non-empty = recurring
        /// </summary>
        public string? DaysOfWeek { get; set; }

        /// <summary>
        /// Whether the schedule is currently enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Last time this schedule was executed (UTC)
        /// </summary>
        public DateTime? LastExecuted { get; set; }

        /// <summary>
        /// Next scheduled execution time (UTC), calculated on schedule evaluation
        /// </summary>
        public DateTime? NextExecution { get; set; }

        /// <summary>
        /// Cron expression generated from ScheduledTime and DaysOfWeek
        /// </summary>
        public string CronExpression { get; set; } = "";
    }
}
