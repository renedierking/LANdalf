namespace API.DTOs {
    /// <summary>
    /// DTO for WakeSchedule responses
    /// </summary>
    public record WakeScheduleDTO(
        int Id,
        int PcDeviceId,
        string PcDeviceName,
        string ScheduledTime,
        string? DaysOfWeek,
        bool Enabled,
        DateTime? LastExecuted,
        DateTime? NextExecution,
        string CronExpression
    );

    /// <summary>
    /// DTO for creating or updating a WakeSchedule
    /// </summary>
    public record WakeScheduleCreateDto(
        int PcDeviceId,
        string ScheduledTime,
        string? DaysOfWeek,
        bool Enabled
    );
}
