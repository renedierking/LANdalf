using API.Models;
using Cronos;

namespace API.Services {
    /// <summary>
    /// Helper service for working with wake schedules
    /// </summary>
    public static class WakeScheduleHelper {
        /// <summary>
        /// Generates a cron expression from structured time and days of week
        /// Format: minute hour * * dayOfWeek
        /// </summary>
        public static string GenerateCronExpression(TimeOnly scheduledTime, string? daysOfWeek) {
            var minute = scheduledTime.Minute;
            var hour = scheduledTime.Hour;

            // If no days specified, it's a one-time schedule (not recurring)
            // Use * for day of week to match any day
            if (string.IsNullOrWhiteSpace(daysOfWeek)) {
                return $"{minute} {hour} * * *";
            }

            // Parse comma-separated days (0=Sunday, 6=Saturday)
            // Cronos uses 0=Sunday format which matches our storage
            var days = daysOfWeek.Trim();
            return $"{minute} {hour} * * {days}";
        }

        /// <summary>
        /// Updates the schedule with cron expression and next execution time
        /// </summary>
        public static void UpdateScheduleExecution(WakeSchedule schedule, DateTime utcNow) {
            schedule.CronExpression = GenerateCronExpression(schedule.ScheduledTime, schedule.DaysOfWeek);

            if (!schedule.Enabled) {
                schedule.NextExecution = null;
                return;
            }

            try {
                var cronExpression = CronExpression.Parse(schedule.CronExpression);
                var next = cronExpression.GetNextOccurrence(utcNow, TimeZoneInfo.Utc);
                schedule.NextExecution = next;
            } catch (Exception) {
                // Invalid cron expression
                schedule.NextExecution = null;
            }
        }

        /// <summary>
        /// Checks if a schedule should execute now
        /// </summary>
        public static bool ShouldExecute(WakeSchedule schedule, DateTime utcNow) {
            if (!schedule.Enabled || schedule.NextExecution == null) {
                return false;
            }

            // Check if the next execution time has passed
            return schedule.NextExecution <= utcNow;
        }

        /// <summary>
        /// Marks a schedule as executed and calculates the next execution time
        /// </summary>
        public static void MarkAsExecuted(WakeSchedule schedule, DateTime utcNow) {
            schedule.LastExecuted = utcNow;

            // For one-time schedules (no days of week), disable after execution
            if (string.IsNullOrWhiteSpace(schedule.DaysOfWeek)) {
                schedule.Enabled = false;
                schedule.NextExecution = null;
            } else {
                // For recurring schedules, calculate next execution
                UpdateScheduleExecution(schedule, utcNow);
            }
        }
    }
}
