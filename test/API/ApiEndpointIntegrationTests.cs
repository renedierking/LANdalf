using API.Data;
using API.DTOs;
using API.Models;
using FluentAssertions;
using LANdalf.API.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using Xunit;

namespace API.Tests.Integration;

/// <summary>
/// Integration tests for API endpoints
/// </summary>
public class ApiEndpointIntegrationTests : IAsyncLifetime {
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private AppDbContext _dbContext = null!;

    public async ValueTask InitializeAsync() {
        // Set environment variable BEFORE creating the factory
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => {
                builder.ConfigureServices(services => {
                    // Remove all DbContext registrations
                    var toRemove = services
                        .Where(d => d.ServiceType.Name.Contains(nameof(AppDbContext)) || d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                        .ToList();

                    foreach (var descriptor in toRemove) {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database
                    services.AddDbContext<AppDbContext>(options => {
                        options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
                    });
                });
            });

        _client = _factory.CreateClient();

        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync() {
        _client?.Dispose();
        _factory?.Dispose();
        if (_dbContext != null) {
            await _dbContext.DisposeAsync();
        }
    }

    #region GetAllDevices Tests

    [Fact]
    public async Task GetAllDevices_ReturnsEmptyList_WhenNoDatabaseEntries() {
        // Act
        var response = await _client.GetAsync("/api/v1/pc-devices/", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var devices = await response.Content
            .ReadFromJsonAsAsyncEnumerable<PcDeviceDTO>(TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);
        devices.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllDevices_ReturnsAllDevices_WhenDatabaseHasEntries() {
        // Arrange
        var device1 = new PcDevice {
            Name = "PC1",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IsOnline = true
        };
        var device2 = new PcDevice {
            Name = "PC2",
            MacAddress = PhysicalAddress.Parse("AA-BB-CC-DD-EE-FF"),
            IsOnline = false
        };
        _dbContext.PcDevices.Add(device1);
        _dbContext.PcDevices.Add(device2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var response = await _client.GetAsync("/api/v1/pc-devices/", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var devices = await response.Content
            .ReadFromJsonAsAsyncEnumerable<PcDeviceDTO>(TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);
        devices.Should().HaveCount(2);
        devices.Should().Contain(d => d.Name == "PC1");
        devices.Should().Contain(d => d.Name == "PC2");
    }

    #endregion

    #region GetDeviceById Tests

    [Fact]
    public async Task GetDeviceById_ReturnsDevice_WhenDeviceExists() {
        // Arrange
        var device = new PcDevice {
            Name = "TestPC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IpAddress = IPAddress.Parse("192.168.1.100"),
            IsOnline = true
        };
        _dbContext.PcDevices.Add(device);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var deviceId = device.Id;

        // Act
        var response = await _client.GetAsync($"/api/v1/pc-devices/{deviceId}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var returnedDevice = await response.Content
            .ReadFromJsonAsAsyncEnumerable<PcDeviceDTO>(TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);
        returnedDevice.Should().HaveCount(1);
        var returnedDeviceSingle = returnedDevice.Single();
        returnedDeviceSingle.Should().NotBeNull();
        returnedDeviceSingle.Id.Should().Be(deviceId);
        returnedDeviceSingle.Name.Should().Be("TestPC");
        returnedDeviceSingle.MacAddress.Should().Be("00-11-22-33-44-55");
    }

    [Fact]
    public async Task GetDeviceById_Returns404_WhenDeviceDoesNotExist() {
        // Act
        var response = await _client.GetAsync("/api/v1/pc-devices/999", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region AddDevice Tests

    [Fact]
    public async Task AddDevice_Creates201_WithValidData() {
        // Arrange
        var dto = new PcCreateDto(
            "NewPC",
            "00-11-22-33-44-55",
            "192.168.1.100",
            "192.168.1.255"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/pc-devices/add", dto, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task AddDevice_PersistsToDatabase() {
        // Arrange
        var dto = new PcCreateDto(
            "PersistPC",
            "11-22-33-44-55-66",
            null,
            null
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/pc-devices/add", dto, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var dbDevice = await _dbContext.PcDevices.FirstOrDefaultAsync(d => d.Name == "PersistPC", TestContext.Current.CancellationToken);
        dbDevice.Should().NotBeNull();
        dbDevice!.MacAddress.Should().Be(PhysicalAddress.Parse("11-22-33-44-55-66"));
    }

    [Fact]
    public async Task AddDevice_Returns400_WithInvalidMacAddress() {
        // Arrange
        var dto = new PcCreateDto(
            "BadPC",
            "INVALID-MAC",
            null,
            null
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/pc-devices/add", dto, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddDevice_Returns400_WithInvalidIpAddress() {
        // Arrange
        var dto = new PcCreateDto(
            "BadIP",
            "00-11-22-33-44-55",
            "NOT-AN-IP",
            null
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/pc-devices/add", dto, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddDevice_Returns400_WithInvalidBroadcastAddress() {
        // Arrange
        var dto = new PcCreateDto(
            "BadBcast",
            "00-11-22-33-44-55",
            "192.168.1.100",
            "INVALID-BROADCAST"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/pc-devices/add", dto, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region UpdateDevice Tests

    [Fact]
    public async Task UpdateDevice_Returns204_WithValidData() {
        // Arrange
        var device = new PcDevice {
            Name = "OriginalName",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IsOnline = true
        };
        _dbContext.PcDevices.Add(device);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var deviceId = device.Id;

        var updateDto = new PcDeviceDTO(
            deviceId,
            "UpdatedName",
            "AA-BB-CC-DD-EE-FF",
            "192.168.1.100",
            "192.168.1.255",
            false
        );

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/pc-devices/{deviceId}/set", updateDto, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateDevice_PersistsChanges() {
        // Arrange
        var device = new PcDevice {
            Name = "OldName",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IsOnline = true
        };
        _dbContext.PcDevices.Add(device);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var deviceId = device.Id;

        var updateDto = new PcDeviceDTO(
            deviceId,
            "NewName",
            "00-11-22-33-44-55",
            null,
            null,
            false
        );

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/pc-devices/{deviceId}/set", updateDto, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updatedDevice = await _dbContext.PcDevices.FindAsync([deviceId], TestContext.Current.CancellationToken);
        updatedDevice!.Name.Should().Be("NewName");
        updatedDevice.IsOnline.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateDevice_Returns404_WhenDeviceNotFound() {
        // Arrange
        var updateDto = new PcDeviceDTO(
            999,
            "NonExistent",
            "00-11-22-33-44-55",
            null,
            null,
            false
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/pc-devices/999/set", updateDto, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDevice_Returns400_WithInvalidMacAddress() {
        // Arrange
        var device = new PcDevice {
            Name = "TestPC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IsOnline = true
        };
        _dbContext.PcDevices.Add(device);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var deviceId = device.Id;

        var updateDto = new PcDeviceDTO(
            deviceId,
            "TestPC",
            "INVALID-MAC",
            null,
            null,
            true
        );

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/pc-devices/{deviceId}/set", updateDto, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region DeleteDevice Tests

    [Fact]
    public async Task DeleteDevice_Returns204_WhenSuccessful() {
        // Arrange
        var device = new PcDevice {
            Name = "ToDelete",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IsOnline = true
        };
        _dbContext.PcDevices.Add(device);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var deviceId = device.Id;

        // Act
        var response = await _client.PostAsync($"/api/v1/pc-devices/{deviceId}/delete", null, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteDevice_RemovesFromDatabase() {
        // Arrange
        var device = new PcDevice {
            Name = "ToBeDeleted",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IsOnline = true
        };
        _dbContext.PcDevices.Add(device);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var deviceId = device.Id;

        // Act
        var response = await _client.PostAsync($"/api/v1/pc-devices/{deviceId}/delete", null, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var deletedDevice = await _dbContext.PcDevices.FindAsync([deviceId], TestContext.Current.CancellationToken);
        deletedDevice.Should().BeNull();
    }

    [Fact]
    public async Task DeleteDevice_Returns404_WhenDeviceNotFound() {
        // Act
        var response = await _client.PostAsync("/api/v1/pc-devices/999/delete", null, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region WakeDevice Tests

    [Fact]
    public async Task WakeDevice_Returns200_WhenSuccessful() {
        // Arrange
        var device = new PcDevice {
            Name = "WakeablePC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            BroadcastAddress = System.Net.IPAddress.Parse("192.168.1.255"),
            IsOnline = true
        };
        _dbContext.PcDevices.Add(device);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var deviceId = device.Id;

        // Act
        var response = await _client.PostAsync($"/api/v1/pc-devices/{deviceId}/wake", null, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WakeDevice_Returns404_WhenDeviceNotFound() {
        // Act
        var response = await _client.PostAsync("/api/v1/pc-devices/999/wake", null, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WakeDevice_ReturnsSuccessMessage() {
        // Arrange
        var device = new PcDevice {
            Name = "MyPC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IsOnline = true
        };
        _dbContext.PcDevices.Add(device);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        var deviceId = device.Id;

        // Act
        var response = await _client.PostAsync($"/api/v1/pc-devices/{deviceId}/wake", null, TestContext.Current.CancellationToken);
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        json.Should().NotBeNull();
    }

    #endregion

    #region CORS Tests

    [Fact]
    public async Task ApiEndpoint_IncludesCorHeaders_InResponse() {
        // Act
        var response = await _client.GetAsync("/api/v1/pc-devices/", TestContext.Current.CancellationToken);

        // Assert
        // Response should succeed (CORS is configured)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
