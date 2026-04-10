using Microsoft.AspNetCore.SignalR;

namespace API.Hubs {
    public class DeviceStatusHub : Hub {
        // Hub for broadcasting real-time device status updates
        // Clients can subscribe to receive notifications when device online status changes
    }
}
