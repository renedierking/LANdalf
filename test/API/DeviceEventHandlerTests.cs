using API.Handler;
using API.Models;
using API.Services;
using FluentAssertions;
using LANdalf.API.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace API.Tests.Handler;

public class DeviceEventHandlerTests {
    private readonly Mock<IAppDbService> _mockAppDbService;
    private readonly DeviceEventHandler _handler;

    public DeviceEventHandlerTests() {
        _mockAppDbService = new Mock<IAppDbService>();
        _handler = new DeviceEventHandler(_mockAppDbService.Object, NullLogger<DeviceEventHandler>.Instance);
    }

    #region GetAllEvents Tests

    [Fact]
    public async Task GetAllEvents_ReturnsEmptyList_WhenNoEventsExist() {
        // Arrange
        _mockAppDbService.Setup(s => s.GetAllDeviceEventsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DeviceEvent>());

        // Act
        var result = await _handler.GetAllEvents(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var okResult = result as Ok<List<DeviceEventDTO>>;
        okResult?.Value.Should().BeOfType<List<DeviceEventDTO>>();
        okResult?.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllEvents_ReturnsAllEvents_WhenEventsExist() {
        // Arrange
        var events = new List<DeviceEvent> {
            new() { Id = 1, PcDeviceId = 1, EventType = "WakeCommandSent", Timestamp = DateTime.UtcNow, PcDevice = new PcDevice { Id = 1, Name = "PC1" } },
            new() { Id = 2, PcDeviceId = 1, EventType = "CameOnline", Timestamp = DateTime.UtcNow, PcDevice = new PcDevice { Id = 1, Name = "PC1" } }
        };

        _mockAppDbService.Setup(s => s.GetAllDeviceEventsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        // Act
        var result = await _handler.GetAllEvents(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var okResult = result as Ok<List<DeviceEventDTO>>;
        okResult?.Value.Should().NotBeNull();
        okResult?.Value.Should().HaveCount(2);
    }

    #endregion

    #region GetEventsByDeviceId Tests

    [Fact]
    public async Task GetEventsByDeviceId_ReturnsEvents_WhenEventsExist() {
        // Arrange
        var events = new List<DeviceEvent> {
            new() { Id = 1, PcDeviceId = 1, EventType = "WakeCommandSent", Timestamp = DateTime.UtcNow, PcDevice = new PcDevice { Id = 1, Name = "PC1" } },
            new() { Id = 2, PcDeviceId = 1, EventType = "CameOnline", Timestamp = DateTime.UtcNow, PcDevice = new PcDevice { Id = 1, Name = "PC1" } }
        };

        _mockAppDbService.Setup(s => s.GetDeviceEventsByDeviceIdAsync(1, 50, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        // Act
        var result = await _handler.GetEventsByDeviceId(1, 50, 0, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var okResult = result as Ok<List<DeviceEventDTO>>;
        okResult?.Value.Should().NotBeNull();
        okResult?.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEventsByDeviceId_ReturnsEmptyList_WhenNoEventsExist() {
        // Arrange
        _mockAppDbService.Setup(s => s.GetDeviceEventsByDeviceIdAsync(1, 50, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DeviceEvent>());

        // Act
        var result = await _handler.GetEventsByDeviceId(1, 50, 0, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var okResult = result as Ok<List<DeviceEventDTO>>;
        okResult?.Value.Should().BeOfType<List<DeviceEventDTO>>();
        okResult?.Value.Should().BeEmpty();
    }

    #endregion

    #region GetEventById Tests

    [Fact]
    public async Task GetEventById_ReturnsEvent_WhenEventExists() {
        // Arrange
        var deviceEvent = new DeviceEvent {
            Id = 1,
            PcDeviceId = 1,
            EventType = "WakeCommandSent",
            Timestamp = DateTime.UtcNow,
            PcDevice = new PcDevice { Id = 1, Name = "PC1" }
        };

        _mockAppDbService.Setup(s => s.GetDeviceEventByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deviceEvent);

        // Act
        var result = await _handler.GetEventById(1, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var okResult = result as Ok<DeviceEventDTO>;
        okResult?.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetEventById_ReturnsNotFound_WhenEventDoesNotExist() {
        // Arrange
        _mockAppDbService.Setup(s => s.GetDeviceEventByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DeviceEvent?)null);

        // Act
        var result = await _handler.GetEventById(999, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result as NotFound<ProblemDetails>;
        notFoundResult?.StatusCode.Should().Be(404);
    }

    #endregion

    #region DeleteEvent Tests

    [Fact]
    public async Task DeleteEvent_ReturnsNoContent_WhenEventExists() {
        // Arrange
        _mockAppDbService.Setup(s => s.DeleteDeviceEventAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.DeleteEvent(1, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var noContentResult = result as NoContent;
        noContentResult?.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task DeleteEvent_ReturnsNotFound_WhenEventDoesNotExist() {
        // Arrange
        _mockAppDbService.Setup(s => s.DeleteDeviceEventAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.DeleteEvent(999, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result as NotFound<ProblemDetails>;
        notFoundResult?.StatusCode.Should().Be(404);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenAppDbServiceIsNull() {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DeviceEventHandler(null!, NullLogger<DeviceEventHandler>.Instance));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull() {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DeviceEventHandler(_mockAppDbService.Object, null!));
    }

    #endregion
}
