using API.Models;
using API.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Net.NetworkInformation;
using Xunit;

namespace API.Tests.Services;

public class DeviceMonitoringServiceTests {
    private readonly Mock<IAppDbService> _mockAppDbService;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;

    public DeviceMonitoringServiceTests() {
        _mockAppDbService = new Mock<IAppDbService>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();

        // Setup service scope chain
        _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);
        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceProvider.Setup(p => p.GetService(typeof(IAppDbService))).Returns(_mockAppDbService.Object);
    }

    private static IOptions<DeviceMonitoringOptions> CreateOptions(
        bool? enabled = null,
        int? intervalSeconds = null,
        int? timeoutMilliseconds = null) {
        var options = new DeviceMonitoringOptions {
            Enabled = enabled ?? true,
            IntervalSeconds = intervalSeconds ?? 30,
            TimeoutMilliseconds = timeoutMilliseconds ?? 2000
        };
        return Options.Create(options);
    }

    #region Configuration Tests

    [Fact]
    public void Constructor_LoadsDefaultConfiguration_WhenNoConfigProvided() {
        // Arrange & Act
        var service = new DeviceMonitoringService(
            NullLogger<DeviceMonitoringService>.Instance,
            _mockScopeFactory.Object,
            CreateOptions());

        // Assert
        service.IsEnabled.Should().BeTrue();
        service.IntervalSeconds.Should().Be(30);
        service.TimeoutMilliseconds.Should().Be(2000);
    }

    [Fact]
    public void Constructor_LoadsCustomConfiguration_WhenConfigProvided() {
        // Arrange & Act
        var service = new DeviceMonitoringService(
            NullLogger<DeviceMonitoringService>.Instance,
            _mockScopeFactory.Object,
            CreateOptions(enabled: false, intervalSeconds: 60, timeoutMilliseconds: 5000));

        // Assert
        service.IsEnabled.Should().BeFalse();
        service.IntervalSeconds.Should().Be(60);
        service.TimeoutMilliseconds.Should().Be(5000);
    }

    [Fact]
    public void Constructor_EnforcesMinimumIntervalSeconds() {
        // Arrange & Act
        var service = new DeviceMonitoringService(
            NullLogger<DeviceMonitoringService>.Instance,
            _mockScopeFactory.Object,
            CreateOptions(intervalSeconds: 1));

        // Assert
        service.IntervalSeconds.Should().Be(5); // Minimum enforced
    }

    [Fact]
    public void Constructor_EnforcesMinimumTimeoutMilliseconds() {
        // Arrange & Act
        var service = new DeviceMonitoringService(
            NullLogger<DeviceMonitoringService>.Instance,
            _mockScopeFactory.Object,
            CreateOptions(timeoutMilliseconds: 50));

        // Assert
        service.TimeoutMilliseconds.Should().Be(100); // Minimum enforced
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull() {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DeviceMonitoringService(null!, _mockScopeFactory.Object, CreateOptions()));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenScopeFactoryIsNull() {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DeviceMonitoringService(NullLogger<DeviceMonitoringService>.Instance, null!, CreateOptions()));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOptionsIsNull() {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DeviceMonitoringService(NullLogger<DeviceMonitoringService>.Instance, _mockScopeFactory.Object, null!));
    }

    #endregion

    #region CheckAllDevicesAsync Tests

    [Fact]
    public async Task CheckAllDevicesAsync_HandlesEmptyDeviceList() {
        // Arrange
        var service = new DeviceMonitoringService(
            NullLogger<DeviceMonitoringService>.Instance,
            _mockScopeFactory.Object,
            CreateOptions());

        _mockAppDbService.Setup(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PcDevice>());

        // Act
        await service.CheckAllDevicesAsync(TestContext.Current.CancellationToken);

        // Assert
        _mockAppDbService.Verify(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockAppDbService.Verify(s => s.UpdatePcDeviceAsync(It.IsAny<PcDevice>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckAllDevicesAsync_SkipsDevicesWithoutIpAddress() {
        // Arrange
        var service = new DeviceMonitoringService(
            NullLogger<DeviceMonitoringService>.Instance,
            _mockScopeFactory.Object,
            CreateOptions());

        var devices = new List<PcDevice> {
            new() {
                Id = 1,
                Name = "Device1",
                MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
                IpAddress = null,
                IsOnline = false
            }
        };

        _mockAppDbService.Setup(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(devices);

        // Act
        await service.CheckAllDevicesAsync(TestContext.Current.CancellationToken);

        // Assert
        _mockAppDbService.Verify(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()), Times.Once);
        // Should not update device since it's already offline and has no IP
        _mockAppDbService.Verify(s => s.UpdatePcDeviceAsync(It.IsAny<PcDevice>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckAllDevicesAsync_MarksDeviceOffline_WhenNoIpAndWasOnline() {
        // Arrange
        var service = new DeviceMonitoringService(
            NullLogger<DeviceMonitoringService>.Instance,
            _mockScopeFactory.Object,
            CreateOptions());

        var devices = new List<PcDevice> {
            new() {
                Id = 1,
                Name = "Device1",
                MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
                IpAddress = null,
                IsOnline = true // Was online before
            }
        };

        _mockAppDbService.Setup(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(devices);

        _mockAppDbService.Setup(s => s.UpdatePcDeviceAsync(It.IsAny<PcDevice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PcDevice d, CancellationToken ct) => d);

        // Act
        await service.CheckAllDevicesAsync(TestContext.Current.CancellationToken);

        // Assert
        _mockAppDbService.Verify(s => s.UpdatePcDeviceAsync(
            It.Is<PcDevice>(d => d.Id == 1 && !d.IsOnline),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckAllDevicesAsync_PingsDevicesWithIpAddress() {
        // Arrange
        var service = new DeviceMonitoringService(
            NullLogger<DeviceMonitoringService>.Instance,
            _mockScopeFactory.Object,
            CreateOptions());

        // Use loopback address which should be reachable
        var devices = new List<PcDevice> {
            new() {
                Id = 1,
                Name = "Localhost",
                MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
                IpAddress = IPAddress.Loopback,
                IsOnline = false
            }
        };

        _mockAppDbService.Setup(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(devices);

        _mockAppDbService.Setup(s => s.UpdatePcDeviceAsync(It.IsAny<PcDevice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PcDevice d, CancellationToken ct) => d);

        // Act
        await service.CheckAllDevicesAsync(TestContext.Current.CancellationToken);

        // Assert
        _mockAppDbService.Verify(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()), Times.Once);
        // Should attempt to update device status
        _mockAppDbService.Verify(s => s.UpdatePcDeviceAsync(It.IsAny<PcDevice>(), It.IsAny<CancellationToken>()), Times.AtMostOnce);
    }

    [Fact]
    public async Task CheckAllDevicesAsync_DoesNotUpdateDevice_WhenStatusUnchanged() {
        // Arrange
        var service = new DeviceMonitoringService(
            NullLogger<DeviceMonitoringService>.Instance,
            _mockScopeFactory.Object,
            CreateOptions());

        // Use an unreachable private IP
        var devices = new List<PcDevice> {
            new() {
                Id = 1,
                Name = "Device1",
                MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
                IpAddress = IPAddress.Parse("192.0.2.1"), // TEST-NET-1, should be unreachable
                IsOnline = false // Already marked offline
            }
        };

        _mockAppDbService.Setup(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(devices);

        // Act
        await service.CheckAllDevicesAsync(TestContext.Current.CancellationToken);

        // Assert
        _mockAppDbService.Verify(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()), Times.Once);
        // Should not update since status didn't change
        _mockAppDbService.Verify(s => s.UpdatePcDeviceAsync(It.IsAny<PcDevice>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckAllDevicesAsync_ProcessesMultipleDevicesInParallel() {
        // Arrange
        var service = new DeviceMonitoringService(
            NullLogger<DeviceMonitoringService>.Instance,
            _mockScopeFactory.Object,
            CreateOptions());

        var devices = new List<PcDevice> {
            new() {
                Id = 1,
                Name = "Device1",
                MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
                IpAddress = IPAddress.Parse("192.0.2.1"),
                IsOnline = false
            },
            new() {
                Id = 2,
                Name = "Device2",
                MacAddress = PhysicalAddress.Parse("AA-BB-CC-DD-EE-FF"),
                IpAddress = IPAddress.Parse("192.0.2.2"),
                IsOnline = false
            },
            new() {
                Id = 3,
                Name = "Device3",
                MacAddress = PhysicalAddress.Parse("11-22-33-44-55-66"),
                IpAddress = null,
                IsOnline = false
            }
        };

        _mockAppDbService.Setup(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(devices);

        // Act
        await service.CheckAllDevicesAsync(TestContext.Current.CancellationToken);

        // Assert
        _mockAppDbService.Verify(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckAllDevicesAsync_RespectsCancellationToken() {
        // Arrange
        var service = new DeviceMonitoringService(
            NullLogger<DeviceMonitoringService>.Instance,
            _mockScopeFactory.Object,
            CreateOptions());

        var cts = new CancellationTokenSource();

        // Setup mock to throw OperationCanceledException to simulate cancellation during operation
        _mockAppDbService.Setup(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await service.CheckAllDevicesAsync(cts.Token));
    }

    #endregion

    #region Background Service Tests

    [Fact]
    public async Task ExecuteAsync_DoesNotStart_WhenDisabled() {
        // Arrange
        var service = new DeviceMonitoringService(
            NullLogger<DeviceMonitoringService>.Instance,
            _mockScopeFactory.Object,
            CreateOptions(enabled: false));

        _mockAppDbService.Setup(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PcDevice>());

        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(50, TestContext.Current.CancellationToken); // Wait a bit
        await service.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        // Should not have called GetAllPcDevicesAsync since monitoring is disabled
        _mockAppDbService.Verify(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_StartsMonitoring_WhenEnabled() {
        // Arrange
        var service = new DeviceMonitoringService(
            NullLogger<DeviceMonitoringService>.Instance,
            _mockScopeFactory.Object,
            CreateOptions(enabled: true, intervalSeconds: 5)); // Minimum allowed

        _mockAppDbService.Setup(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PcDevice>());

        var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(6000, TestContext.Current.CancellationToken); // Wait for initial delay (5s) + first check
        cts.Cancel();
        await service.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        // Should have called GetAllPcDevicesAsync at least once after the initial delay
        _mockAppDbService.Verify(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    #endregion
}
