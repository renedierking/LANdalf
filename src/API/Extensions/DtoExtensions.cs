using API.DTOs;
using API.Models;
using LANdalf.API.DTOs;

namespace LANdalf.API.Extensions {
    public static class DtoExtensions {
        public static PcDeviceDTO ToDto(this PcDevice pcDevice) =>
            new PcDeviceDTO(
                Id: pcDevice.Id,
                Name: pcDevice.Name,
                MacAddress: string.Join("-", pcDevice.MacAddress.GetAddressBytes().Select(b => b.ToString("X2"))),
                IpAddress: pcDevice.IpAddress?.ToString(),
                BroadcastAddress: pcDevice.BroadcastAddress?.ToString(),
                IsOnline: pcDevice.IsOnline,
                OnlineSince: pcDevice.OnlineSince,
                GroupName: pcDevice.GroupName
            );

        public static WakeScheduleDTO ToDto(this WakeSchedule schedule) =>
            new WakeScheduleDTO(
                Id: schedule.Id,
                PcDeviceId: schedule.PcDeviceId,
                PcDeviceName: schedule.PcDevice?.Name ?? "",
                ScheduledTime: schedule.ScheduledTime.ToString("HH:mm"),
                DaysOfWeek: schedule.DaysOfWeek,
                Enabled: schedule.Enabled,
                LastExecuted: schedule.LastExecuted,
                NextExecution: schedule.NextExecution,
                CronExpression: schedule.CronExpression
            );

        public static DeviceEventDTO ToDto(this DeviceEvent deviceEvent) =>
            new DeviceEventDTO(
                Id: deviceEvent.Id,
                PcDeviceId: deviceEvent.PcDeviceId,
                PcDeviceName: deviceEvent.PcDevice?.Name ?? "",
                EventType: deviceEvent.EventType,
                Timestamp: deviceEvent.Timestamp,
                Details: deviceEvent.Details
            );
    }
}
