using FluentAssertions;
using LANdalf.UI;
using LANdalf.UI.ApiClient;
using LANdalf.UI.Services;
using Moq;
using Xunit;

namespace UI.Tests.Services;

public class LANdalfApiServiceTests {
    private readonly Mock<LANdalfApiClient> _mockApiClient;
    private readonly LANdalfApiService _service;

    public LANdalfApiServiceTests() {
        _mockApiClient = new Mock<LANdalfApiClient>(new System.Net.Http.HttpClient()) { CallBase = false };
        _service = new LANdalfApiService(_mockApiClient.Object);
    }

    #region GetAllPcDevicesAsync Tests

    [Fact]
    public async Task GetAllPcDevicesAsync_ReturnsDeviceList_OnSuccess() {
        // Arrange
        var devices = new List<PcDeviceDTO> {
            new PcDeviceDTO { Id = 1,
                Name = "PC1",
                MacAddress = "00-11-22-33-44-55",
                BroadcastAddress = null,
                IpAddress = null,
                IsOnline = true
            },
            new PcDeviceDTO { Id = 2,
                Name = "PC2",
                MacAddress = "AA-BB-CC-DD-EE-FF",
                BroadcastAddress = null,
                IpAddress = null,
                IsOnline = false
            }
        };

        _mockApiClient.Setup(x => x.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(devices);

        // Act
        var result = await _service.GetAllPcDevicesAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(d => d.Id == 1);
        result.Value.Should().Contain(d => d.Id == 2);
    }

    [Fact]
    public async Task GetAllPcDevicesAsync_ReturnsEmptyList_OnSuccess() {
        // Arrange
        _mockApiClient.Setup(x => x.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PcDeviceDTO>());

        // Act
        var result = await _service.GetAllPcDevicesAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllPcDevicesAsync_ReturnsError_OnException() {
        // Arrange
        var exception = new HttpRequestException("Network error");
        _mockApiClient.Setup(x => x.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.GetAllPcDevicesAsync();

        // Assert
        result.IsError.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<HttpRequestException>();
    }

    [Fact]
    public async Task GetAllPcDevicesAsync_RespectsCancellationToken() {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockApiClient.Setup(x => x.GetAllPcDevicesAsync(cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _service.GetAllPcDevicesAsync(cts.Token);

        // Assert
        result.IsError.Should().BeTrue();
        result.Error.Should().BeOfType<OperationCanceledException>();
    }

    #endregion

    #region GetPcDeviceByIdAsync Tests

    [Fact]
    public async Task GetPcDeviceByIdAsync_ReturnsDevice_OnSuccess() {
        // Arrange
        var device = new PcDeviceDTO {
            Id = 1,
            Name = "TestPC",
            MacAddress = "00-11-22-33-44-55",
            BroadcastAddress = null,
            IpAddress = null,
            IsOnline = true
        };

        _mockApiClient.Setup(x => x.GetPcDeviceByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var result = await _service.GetPcDeviceByIdAsync(1, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(device);
        result.Value!.Name.Should().Be("TestPC");
    }

    [Fact]
    public async Task GetPcDeviceByIdAsync_ReturnsError_OnNotFound() {
        // Arrange
        var exception = new ApiException("Not found", 404, null, null, null);

        _mockApiClient.Setup(x => x.GetPcDeviceByIdAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.GetPcDeviceByIdAsync(999, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.Error.Should().BeOfType<ApiException>();
    }

    [Fact]
    public async Task GetPcDeviceByIdAsync_ReturnsError_OnGeneralException() {
        // Arrange
        var exception = new InvalidOperationException("Unexpected error");

        _mockApiClient.Setup(x => x.GetPcDeviceByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.GetPcDeviceByIdAsync(1, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.Error.Should().BeOfType<InvalidOperationException>();
    }

    #endregion

    #region AddPcDeviceAsync Tests

    [Fact]
    public async Task AddPcDeviceAsync_ReturnsTrue_OnSuccess() {
        // Arrange
        var dto = new PcCreateDto {
            Name = "NewPC",
            MacAddress = "00-11-22-33-44-55",
            IpAddress = "192.168.1.102",
            BroadcastAddress = "192.168.1.255"
        };

        _mockApiClient.Setup(x => x.AddPcDeviceAsync(dto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddPcDeviceAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _mockApiClient.Verify(x => x.AddPcDeviceAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddPcDeviceAsync_ReturnsError_OnBadRequest() {
        // Arrange
        var dto = new PcCreateDto {
            Name = "NewPC",
            MacAddress = "InvalidMacAddress",
            IpAddress = "192.168.1.102",
            BroadcastAddress = "192.168.1.255"
        };
        var exception = new ApiException("Bad request", 400, null, null, null);

        _mockApiClient.Setup(x => x.AddPcDeviceAsync(It.IsAny<PcCreateDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.AddPcDeviceAsync(dto, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.Error.Should().BeOfType<ApiException>();
    }

    #endregion

    #region UpdatePcDevice Tests

    [Fact]
    public async Task UpdatePcDevice_ReturnsTrue_OnSuccess() {
        // Arrange
        var dto = new PcDeviceDTO {
            Id = 1,
            Name = "UpdatedPC",
            MacAddress = "00-11-22-33-44-55",
            IpAddress = "192.168.1.100",
            BroadcastAddress = null,
            IsOnline = true
        };

        _mockApiClient.Setup(x => x.SetPcDeviceAsync(1, dto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdatePcDevice(dto, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _mockApiClient.Verify(x => x.SetPcDeviceAsync(1, dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePcDevice_ReturnsError_OnDeviceNotFound() {
        // Arrange
        var dto = new PcDeviceDTO {
            Id = 999,
            Name = "NonExistentPC",
            MacAddress = "00-11-22-33-44-55",
            IpAddress = null,
            BroadcastAddress = null,
            IsOnline = false
        };
        var exception = new ApiException("Not found", 404, null, null, null);

        _mockApiClient.Setup(x => x.SetPcDeviceAsync(999, It.IsAny<PcDeviceDTO>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.UpdatePcDevice(dto, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.Error.Should().BeOfType<ApiException>();
    }

    [Fact]
    public async Task UpdatePcDevice_UsesCorrectDeviceId() {
        // Arrange
        var dto = new PcDeviceDTO {
            Id = 42,
            Name = "TestPC",
            MacAddress = "00-11-22-33-44-55",
            IpAddress = null,
            BroadcastAddress = null,
            IsOnline = false
        };

        _mockApiClient.Setup(x => x.SetPcDeviceAsync(42, dto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdatePcDevice(dto, TestContext.Current.CancellationToken);

        // Assert
        _mockApiClient.Verify(x => x.SetPcDeviceAsync(42, dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region DeletePcDeviceAsync Tests

    [Fact]
    public async Task DeletePcDeviceAsync_ReturnsTrue_OnSuccess() {
        // Arrange
        _mockApiClient.Setup(x => x.DeletePcDeviceAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeletePcDeviceAsync(1, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _mockApiClient.Verify(x => x.DeletePcDeviceAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePcDeviceAsync_ReturnsError_OnDeviceNotFound() {
        // Arrange
        var exception = new ApiException("Not found", 404, null, null, null);

        _mockApiClient.Setup(x => x.DeletePcDeviceAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.DeletePcDeviceAsync(999, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.Error.Should().BeOfType<ApiException>();
    }

    #endregion

    #region WakePcDeviceAsync Tests

    [Fact]
    public async Task WakePcDeviceAsync_ReturnsTrue_OnSuccess() {
        // Arrange
        _mockApiClient.Setup(x => x.WakePcDeviceAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.WakePcDeviceAsync(1, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _mockApiClient.Verify(x => x.WakePcDeviceAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WakePcDeviceAsync_ReturnsError_OnDeviceNotFound() {
        // Arrange
        var exception = new ApiException("Not found", 404, null, null, null);

        _mockApiClient.Setup(x => x.WakePcDeviceAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.WakePcDeviceAsync(999, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.Error.Should().BeOfType<ApiException>();
    }

    #endregion

    #region Result Type Tests

    [Fact]
    public void Result_ImplicitConversion_FromValue() {
        // Arrange & Act
        Result<string, Exception> result = "Success";

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Success");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Result_ImplicitConversion_FromError() {
        // Arrange & Act
        var exception = new InvalidOperationException("Error occurred");
        Result<string, Exception> result = exception;

        // Assert
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(exception);
        result.Value.Should().BeNull();
    }

    [Fact]
    public void Result_Match_ExecutesSuccessFunction() {
        // Arrange
        Result<int, string> result = 42;

        // Act
        var outcome = result.Match(
            success: value => $"Success: {value}",
            failure: error => $"Error: {error}"
        );

        // Assert
        outcome.Should().Be("Success: 42");
    }

    [Fact]
    public void Result_Match_ExecutesFailureFunction() {
        // Arrange
        Result<int, string> result = "Something went wrong";

        // Act
        var outcome = result.Match(
            success: value => $"Success: {value}",
            failure: error => $"Error: {error}"
        );

        // Assert
        outcome.Should().Be("Error: Something went wrong");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Service_HandlesMultipleExceptionTypes() {
        // Arrange
        var exceptions = new List<Exception> {
            new HttpRequestException("Network error"),
            new TimeoutException("Request timeout"),
            new InvalidOperationException("Invalid state"),
            new OperationCanceledException("Operation cancelled")
        };

        int index = 0;
        _mockApiClient.Setup(x => x.GetAllPcDevicesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromException<ICollection<PcDeviceDTO>>(exceptions[index++]));

        // Act & Assert
        foreach (var exception in exceptions) {
            index = exceptions.IndexOf(exception);
            var result = await _service.GetAllPcDevicesAsync(TestContext.Current.CancellationToken);
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType(exception.GetType());
        }
    }

    #endregion
}
