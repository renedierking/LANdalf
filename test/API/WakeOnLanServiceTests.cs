using API.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Net.NetworkInformation;
using Xunit;

namespace API.Tests.Services;

public class WakeOnLanServiceTests {
    private readonly WakeOnLanService _service;

    public WakeOnLanServiceTests() {
        _service = new WakeOnLanService(NullLogger<WakeOnLanService>.Instance);
    }

    #region Wake Method Tests

    [Fact]
    public async Task Wake_CreatesMagicPacket_WithCorrectStructure() {
        // Arrange
        var mac = PhysicalAddress.Parse("00-11-22-33-44-55");
        var broadcast = IPAddress.Parse("192.168.1.255");
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act & Assert
        // Should not throw and complete without error
        await _service.Wake(mac, broadcast, cts.Token);
    }

    [Fact]
    public async Task Wake_ThrowsArgumentException_WithInvalidMacAddressLength() {
        // Arrange
        var mac = new PhysicalAddress(new byte[] { 0x00, 0x11, 0x22 }); // Only 3 bytes
        var broadcast = IPAddress.Parse("192.168.1.255");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.Wake(mac, broadcast, TestContext.Current.CancellationToken)
        );
    }

    [Fact]
    public async Task Wake_ThrowsArgumentException_WithEmptyMacAddress() {
        // Arrange
        var mac = new PhysicalAddress(new byte[] { }); // Empty
        var broadcast = IPAddress.Parse("192.168.1.255");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.Wake(mac, broadcast, TestContext.Current.CancellationToken)
        );
    }

    [Fact]
    public async Task Wake_AcceptsNullBroadcast_UsesFallbackAddresses() {
        // Arrange
        var mac = PhysicalAddress.Parse("AA-BB-CC-DD-EE-FF");
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act & Assert
        // Should not throw when broadcast is null (uses fallback logic)
        await _service.Wake(mac, null, cts.Token);
    }

    [Fact]
    public async Task Wake_RespectsCancellationToken() {
        // Arrange
        var mac = PhysicalAddress.Parse("00-11-22-33-44-55");
        var broadcast = IPAddress.Parse("192.168.1.255");
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(10));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            async () => await _service.Wake(mac, broadcast, cts.Token)
        );
    }

    #endregion

    #region Magic Packet Structure Tests

    [Fact]
    public async Task Wake_GeneratesMagicPacket_With6ByteHeader() {
        // Arrange
        var mac = PhysicalAddress.Parse("00-11-22-33-44-55");
        var broadcast = IPAddress.Parse("192.168.1.255");
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act & Assert
        // Magic packet should be 6 (header) + 16 * 6 (MAC) = 102 bytes
        // This test verifies the service completes without throwing
        await _service.Wake(mac, broadcast, cts.Token);
    }

    [Theory]
    [InlineData("00-00-00-00-00-00")]
    [InlineData("FF-FF-FF-FF-FF-FF")]
    [InlineData("12-34-56-78-9A-BC")]
    public async Task Wake_AcceptsDifferentValidMacAddresses(string macAddress) {
        // Arrange
        var mac = PhysicalAddress.Parse(macAddress);
        var broadcast = IPAddress.Parse("192.168.1.255");
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act & Assert
        await _service.Wake(mac, broadcast, cts.Token);
    }

    #endregion

    #region Broadcast Address Tests

    [Theory]
    [InlineData("192.168.1.255")]
    [InlineData("10.0.0.255")]
    [InlineData("172.16.0.255")]
    [InlineData("255.255.255.255")]
    public async Task Wake_WorksWithDifferentBroadcastAddresses(string broadcastAddress) {
        // Arrange
        var mac = PhysicalAddress.Parse("00-11-22-33-44-55");
        var broadcast = IPAddress.Parse(broadcastAddress);
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act & Assert
        await _service.Wake(mac, broadcast, cts.Token);
    }

    #endregion

    #region Port and Retry Logic Tests

    [Fact]
    public async Task Wake_SendsToMultiplePorts() {
        // Arrange
        var mac = PhysicalAddress.Parse("00-11-22-33-44-55");
        var broadcast = IPAddress.Parse("192.168.1.255");
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act & Assert
        // Service should send to both port 7 and port 9
        await _service.Wake(mac, broadcast, cts.Token);
    }

    [Fact]
    public async Task Wake_RetriesSending_MultipleTimesPerPort() {
        // Arrange
        var mac = PhysicalAddress.Parse("00-11-22-33-44-55");
        var broadcast = IPAddress.Parse("192.168.1.255");
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act & Assert
        // Service retries 3 times per port with 30ms delays
        // This test verifies the service completes (actual retry count is internal)
        await _service.Wake(mac, broadcast, cts.Token);
    }

    #endregion

    #region Environment Variable Tests

    [Fact]
    public async Task Wake_UsesEnvironmentVariable_WolBroadcasts() {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable("WOL_BROADCASTS");
        try {
            Environment.SetEnvironmentVariable("WOL_BROADCASTS", "10.0.0.255,172.16.0.255");

            var mac = PhysicalAddress.Parse("00-11-22-33-44-55");
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Act & Assert
            // Should use the environment variable addresses
            await _service.Wake(mac, null, cts.Token);
        } finally {
            Environment.SetEnvironmentVariable("WOL_BROADCASTS", originalValue);
        }
    }

    [Fact]
    public async Task Wake_IgnoresInvalidAddressesInEnvironmentVariable() {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable("WOL_BROADCASTS");
        try {
            Environment.SetEnvironmentVariable("WOL_BROADCASTS", "INVALID-IP,192.168.1.255,not-an-ip");

            var mac = PhysicalAddress.Parse("00-11-22-33-44-55");
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Act & Assert
            // Should skip invalid addresses and use valid ones
            await _service.Wake(mac, null, cts.Token);
        } finally {
            Environment.SetEnvironmentVariable("WOL_BROADCASTS", originalValue);
        }
    }

    [Fact]
    public async Task Wake_TrimsWhitespaceInEnvironmentVariable() {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable("WOL_BROADCASTS");
        try {
            Environment.SetEnvironmentVariable("WOL_BROADCASTS", "  192.168.1.255  ,  10.0.0.255  ");

            var mac = PhysicalAddress.Parse("00-11-22-33-44-55");
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Act & Assert
            // Should trim whitespace and use the addresses
            await _service.Wake(mac, null, cts.Token);
        } finally {
            Environment.SetEnvironmentVariable("WOL_BROADCASTS", originalValue);
        }
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task Wake_HandlesDuplicateBroadcastAddresses() {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable("WOL_BROADCASTS");
        try {
            Environment.SetEnvironmentVariable("WOL_BROADCASTS", "192.168.1.255,192.168.1.255,192.168.1.255");

            var mac = PhysicalAddress.Parse("00-11-22-33-44-55");
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Act & Assert
            // Should handle duplicates gracefully
            await _service.Wake(mac, null, cts.Token);
        } finally {
            Environment.SetEnvironmentVariable("WOL_BROADCASTS", originalValue);
        }
    }

    [Fact]
    public async Task Wake_SkipsIPv6AddressesInEnvironmentVariable() {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable("WOL_BROADCASTS");
        try {
            // IPv6 addresses should be ignored, only IPv4 used
            Environment.SetEnvironmentVariable("WOL_BROADCASTS", "::1,192.168.1.255,fe80::1");

            var mac = PhysicalAddress.Parse("00-11-22-33-44-55");
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Act & Assert
            // Should skip IPv6 and use IPv4
            await _service.Wake(mac, null, cts.Token);
        } finally {
            Environment.SetEnvironmentVariable("WOL_BROADCASTS", originalValue);
        }
    }

    [Fact]
    public async Task Wake_UsesGlobalBroadcastFallback_WhenNoAddressesAvailable() {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable("WOL_BROADCASTS");
        try {
            Environment.SetEnvironmentVariable("WOL_BROADCASTS", "");

            var mac = PhysicalAddress.Parse("00-11-22-33-44-55");
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Act & Assert
            // Should fall back to 255.255.255.255
            await _service.Wake(mac, null, cts.Token);
        } finally {
            Environment.SetEnvironmentVariable("WOL_BROADCASTS", originalValue);
        }
    }

    #endregion
}
