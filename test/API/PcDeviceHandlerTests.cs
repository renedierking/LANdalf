using API.DTOs;
using API.Handler;
using API.Models;
using API.Services;
using FluentAssertions;
using LANdalf.API.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;
using System.Net.NetworkInformation;
using Xunit;

namespace API.Tests.Handler;

public class PcDeviceHandlerTests {
    private readonly Mock<IAppDbService> _mockAppDbService;
    private readonly Mock<WakeOnLanService> _mockWolService;
    private readonly PcDeviceHandler _handler;

    public PcDeviceHandlerTests() {
        _mockAppDbService = new Mock<IAppDbService>();
        _mockWolService = new Mock<WakeOnLanService>();
        _handler = new PcDeviceHandler(_mockAppDbService.Object, _mockWolService.Object);
    }

    #region GetAllDevices Tests

    [Fact]
    public async Task GetAllDevices_ReturnsEmptyList_WhenNoPcDevicesExist() {
        // Arrange
        _mockAppDbService.Setup(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PcDevice>());

        // Act
        var result = await _handler.GetAllDevices(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var okResult = result as Ok<List<PcDeviceDTO>>;
        okResult?.Value.Should().BeOfType<List<PcDeviceDTO>>();
        okResult?.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllDevices_ReturnsAllDevices_WhenDevicesExist() {
        // Arrange
        var devices = new List<PcDevice> {
            new() { Id = 1, Name = "PC1", MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"), IsOnline = true },
            new() { Id = 2, Name = "PC2", MacAddress = PhysicalAddress.Parse("AA-BB-CC-DD-EE-FF"), IsOnline = false }
        };

        _mockAppDbService.Setup(s => s.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(devices);

        // Act
        var result = await _handler.GetAllDevices(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var okResult = result as Ok<List<PcDeviceDTO>>;
        okResult?.Value.Should().NotBeNull();
        okResult?.Value.Should().HaveCount(2);
    }

    #endregion

    #region GetDeviceById Tests

    [Fact]
    public async Task GetDeviceById_ReturnsDevice_WhenDeviceExists() {
        // Arrange
        var device = new PcDevice {
            Id = 1,
            Name = "TestPC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IsOnline = true
        };

        _mockAppDbService.Setup(s => s.GetPcDeviceByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var result = await _handler.GetDeviceById(1, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var okResult = result as Ok<PcDeviceDTO>;
        okResult?.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetDeviceById_ReturnsNotFound_WhenDeviceDoesNotExist() {
        // Arrange
        _mockAppDbService.Setup(s => s.GetPcDeviceByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PcDevice?)null);

        // Act
        var result = await _handler.GetDeviceById(999, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result as NotFound<ProblemDetails>;
        notFoundResult?.StatusCode.Should().Be(404);
    }

    #endregion

    #region AddDevice Tests

    [Fact]
    public async Task AddDevice_CreatesDevice_WithValidInput() {
        // Arrange
        var dto = new PcCreateDto("TestPC", "00-11-22-33-44-55", "192.168.1.100", "192.168.1.255");
        var device = new PcDevice {
            Id = 1,
            Name = "TestPC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IpAddress = IPAddress.Parse("192.168.1.100"),
            BroadcastAddress = IPAddress.Parse("192.168.1.255")
        };

        _mockAppDbService.Setup(s => s.CreatePcDeviceAsync(It.IsAny<PcDevice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var result = await _handler.AddDevice(dto, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var createdResult = result as Created;
        createdResult?.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task AddDevice_ReturnsBadRequest_WithInvalidMacAddress() {
        // Arrange
        var dto = new PcCreateDto("TestPC", "INVALID-MAC", "192.168.1.100", "192.168.1.255");

        // Act
        var result = await _handler.AddDevice(dto, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result as BadRequest<ProblemDetails>;
        badRequestResult?.StatusCode.Should().Be(400);
        var problemDetails = badRequestResult?.Value as ProblemDetails;
        problemDetails?.Detail.Should().Contain("MAC-Address invalid");
    }

    [Fact]
    public async Task AddDevice_ReturnsBadRequest_WithInvalidIpAddress() {
        // Arrange
        var dto = new PcCreateDto("TestPC", "00-11-22-33-44-55", "INVALID-IP", null);

        // Act
        var result = await _handler.AddDevice(dto, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result as BadRequest<ProblemDetails>;
        badRequestResult?.StatusCode.Should().Be(400);
        var problemDetails = badRequestResult?.Value as ProblemDetails;
        problemDetails?.Detail.Should().Contain("IP-Address invalid");
    }

    [Fact]
    public async Task AddDevice_ReturnsBadRequest_WithInvalidBroadcastAddress() {
        // Arrange
        var dto = new PcCreateDto("TestPC", "00-11-22-33-44-55", "192.168.1.100", "INVALID-BROADCAST");

        // Act
        var result = await _handler.AddDevice(dto, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result as BadRequest<ProblemDetails>;
        badRequestResult?.StatusCode.Should().Be(400);
        var problemDetails = badRequestResult?.Value as ProblemDetails;
        problemDetails?.Detail.Should().Contain("Broadcast-Address invalid");
    }

    [Fact]
    public async Task AddDevice_CreatesDevice_WithNullOptionalAddresses() {
        // Arrange
        var dto = new PcCreateDto("TestPC", "00-11-22-33-44-55", null, null);
        var device = new PcDevice {
            Id = 1,
            Name = "TestPC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IpAddress = null,
            BroadcastAddress = null
        };

        _mockAppDbService.Setup(s => s.CreatePcDeviceAsync(It.IsAny<PcDevice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var result = await _handler.AddDevice(dto, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var createdResult = result as Created;
        createdResult?.StatusCode.Should().Be(201);
    }

    #endregion

    #region SetDevice Tests

    [Fact]
    public async Task SetDevice_UpdatesDevice_WithValidInput() {
        // Arrange
        var device = new PcDevice {
            Id = 1,
            Name = "OldName",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IsOnline = true
        };

        var updatedDevice = new PcDevice {
            Id = 1,
            Name = "NewName",
            MacAddress = PhysicalAddress.Parse("AA-BB-CC-DD-EE-FF"),
            IpAddress = IPAddress.Parse("192.168.1.100"),
            BroadcastAddress = IPAddress.Parse("192.168.1.255"),
            IsOnline = false
        };

        _mockAppDbService.Setup(s => s.GetPcDeviceByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockAppDbService.Setup(s => s.UpdatePcDeviceAsync(It.IsAny<PcDevice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDevice);

        var dto = new PcDeviceDTO(1, "NewName", "AA-BB-CC-DD-EE-FF", "192.168.1.100", "192.168.1.255", false);

        // Act
        var result = await _handler.SetDevice(1, dto, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var noContentResult = result as NoContent;
        noContentResult?.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task SetDevice_ReturnsNotFound_WhenDeviceDoesNotExist() {
        // Arrange
        _mockAppDbService.Setup(s => s.GetPcDeviceByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PcDevice?)null);

        var dto = new PcDeviceDTO(999, "NewName", "00-11-22-33-44-55", null, null, false);

        // Act
        var result = await _handler.SetDevice(999, dto, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result as NotFound<ProblemDetails>;
        notFoundResult?.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task SetDevice_ReturnsBadRequest_WithInvalidMacAddress() {
        // Arrange
        var device = new PcDevice {
            Id = 1,
            Name = "TestPC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IsOnline = true
        };

        _mockAppDbService.Setup(s => s.GetPcDeviceByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        var dto = new PcDeviceDTO(1, "TestPC", "INVALID-MAC", null, null, false);

        // Act
        var result = await _handler.SetDevice(1, dto, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var badRequestResult = result as BadRequest<ProblemDetails>;
        badRequestResult?.StatusCode.Should().Be(400);
        var problemDetails = badRequestResult?.Value as ProblemDetails;
        problemDetails?.Detail.Should().Contain("MAC-Address invalid");
    }

    #endregion

    #region DeleteDevice Tests

    [Fact]
    public async Task DeleteDevice_DeletesDevice_WhenDeviceExists() {
        // Arrange
        _mockAppDbService.Setup(s => s.DeletePcDeviceAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.DeleteDevice(1, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var noContentResult = result as NoContent;
        noContentResult?.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task DeleteDevice_ReturnsNotFound_WhenDeviceDoesNotExist() {
        // Arrange
        _mockAppDbService.Setup(s => s.DeletePcDeviceAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.DeleteDevice(999, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result as NotFound<ProblemDetails>;
        notFoundResult?.StatusCode.Should().Be(404);
    }

    #endregion

    #region WakeDevice Tests

    [Fact]
    public async Task WakeDevice_SendsWakePacket_WhenDeviceExists() {
        // Arrange
        var device = new PcDevice {
            Id = 1,
            Name = "TestPC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            BroadcastAddress = IPAddress.Parse("192.168.1.255"),
            IsOnline = true
        };

        _mockAppDbService.Setup(s => s.GetPcDeviceByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var result = await _handler.WakeDevice(1, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var okResult = result as Ok;
        okResult?.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task WakeDevice_ReturnsNotFound_WhenDeviceDoesNotExist() {
        // Arrange
        _mockAppDbService.Setup(s => s.GetPcDeviceByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PcDevice?)null);

        // Act
        var result = await _handler.WakeDevice(999, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        var notFoundResult = result as NotFound<ProblemDetails>;
        notFoundResult?.StatusCode.Should().Be(404);
    }

    #endregion
}
