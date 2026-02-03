using API.Models;
using FluentAssertions;
using LANdalf.API.DTOs;
using LANdalf.API.Extensions;
using System.Net;
using System.Net.NetworkInformation;
using Xunit;

namespace API.Tests.Extensions;

public class DtoExtensionsTests {

    #region ToDto Method Tests

    [Fact]
    public void ToDto_ConvertsPcDeviceToDto_WithAllProperties() {
        // Arrange
        var pcDevice = new PcDevice {
            Id = 1,
            Name = "TestPC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IpAddress = IPAddress.Parse("192.168.1.100"),
            BroadcastAddress = IPAddress.Parse("192.168.1.255"),
            IsOnline = true
        };

        // Act
        var dto = pcDevice.ToDto();

        // Assert
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("TestPC");
        dto.MacAddress.Should().Be("00-11-22-33-44-55");
        dto.IpAddress.Should().Be("192.168.1.100");
        dto.BroadcastAddress.Should().Be("192.168.1.255");
        dto.IsOnline.Should().BeTrue();
    }

    [Fact]
    public void ToDto_FormatsMacAddress_AsHexWithDashes() {
        // Arrange
        var pcDevice = new PcDevice {
            Id = 1,
            Name = "TestPC",
            MacAddress = PhysicalAddress.Parse("AA-BB-CC-DD-EE-FF"),
            IsOnline = false
        };

        // Act
        var dto = pcDevice.ToDto();

        // Assert
        dto.MacAddress.Should().Be("AA-BB-CC-DD-EE-FF");
        dto.MacAddress.Should().MatchRegex(@"^[0-9A-F]{2}(-[0-9A-F]{2}){5}$");
    }

    [Fact]
    public void ToDto_HandlesNullIpAddress() {
        // Arrange
        var pcDevice = new PcDevice {
            Id = 1,
            Name = "TestPC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IpAddress = null,
            IsOnline = true
        };

        // Act
        var dto = pcDevice.ToDto();

        // Assert
        dto.IpAddress.Should().BeNull();
    }

    [Fact]
    public void ToDto_HandlesNullBroadcastAddress() {
        // Arrange
        var pcDevice = new PcDevice {
            Id = 1,
            Name = "TestPC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            BroadcastAddress = null,
            IsOnline = false
        };

        // Act
        var dto = pcDevice.ToDto();

        // Assert
        dto.BroadcastAddress.Should().BeNull();
    }

    [Fact]
    public void ToDto_HandlesAllNullOptionalAddresses() {
        // Arrange
        var pcDevice = new PcDevice {
            Id = 1,
            Name = "TestPC",
            MacAddress = PhysicalAddress.Parse("11-22-33-44-55-66"),
            IpAddress = null,
            BroadcastAddress = null,
            IsOnline = true
        };

        // Act
        var dto = pcDevice.ToDto();

        // Assert
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("TestPC");
        dto.MacAddress.Should().Be("11-22-33-44-55-66");
        dto.IpAddress.Should().BeNull();
        dto.BroadcastAddress.Should().BeNull();
        dto.IsOnline.Should().BeTrue();
    }

    [Theory]
    [InlineData("00-00-00-00-00-00")]
    [InlineData("FF-FF-FF-FF-FF-FF")]
    [InlineData("12-34-56-78-9A-BC")]
    [InlineData("DE-AD-BE-EF-CA-FE")]
    public void ToDto_CorrectlyFormatsDifferentMacAddresses(string macAddressString) {
        // Arrange
        var pcDevice = new PcDevice {
            Id = 1,
            Name = "TestPC",
            MacAddress = PhysicalAddress.Parse(macAddressString),
            IsOnline = false
        };

        // Act
        var dto = pcDevice.ToDto();

        // Assert
        dto.MacAddress.Should().Be(macAddressString);
    }

    [Fact]
    public void ToDto_PreservesIsOnlineStatus_True() {
        // Arrange
        var pcDevice = new PcDevice {
            Id = 1,
            Name = "OnlinePC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IsOnline = true
        };

        // Act
        var dto = pcDevice.ToDto();

        // Assert
        dto.IsOnline.Should().BeTrue();
    }

    [Fact]
    public void ToDto_PreservesIsOnlineStatus_False() {
        // Arrange
        var pcDevice = new PcDevice {
            Id = 1,
            Name = "OfflinePC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IsOnline = false
        };

        // Act
        var dto = pcDevice.ToDto();

        // Assert
        dto.IsOnline.Should().BeFalse();
    }

    [Fact]
    public void ToDto_HandlesDifferentIPAddressTypes() {
        // Arrange
        var pcDevice = new PcDevice {
            Id = 1,
            Name = "TestPC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IpAddress = IPAddress.Parse("10.0.0.1"),
            BroadcastAddress = IPAddress.Parse("10.0.0.255"),
            IsOnline = true
        };

        // Act
        var dto = pcDevice.ToDto();

        // Assert
        dto.IpAddress.Should().Be("10.0.0.1");
        dto.BroadcastAddress.Should().Be("10.0.0.255");
    }

    [Fact]
    public void ToDto_ProducesValidDtoRecord() {
        // Arrange
        var pcDevice = new PcDevice {
            Id = 5,
            Name = "MyPC",
            MacAddress = PhysicalAddress.Parse("AA-BB-CC-DD-EE-FF"),
            IpAddress = IPAddress.Parse("192.168.100.50"),
            BroadcastAddress = IPAddress.Parse("192.168.100.255"),
            IsOnline = true
        };

        // Act
        var dto = pcDevice.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Should().BeOfType<PcDeviceDTO>();
        dto.Id.Should().Be(5);
    }

    #endregion
}

public class PcDeviceModelTests {

    #region Property Initialization Tests

    [Fact]
    public void PcDevice_InitializesWithDefaultValues() {
        // Arrange & Act
        var device = new PcDevice();

        // Assert
        device.Id.Should().Be(0);
        device.Name.Should().Be("");
        device.MacAddress.Should().Be(PhysicalAddress.None);
        device.IpAddress.Should().BeNull();
        device.BroadcastAddress.Should().BeNull();
        device.IsOnline.Should().BeFalse();
    }

    [Fact]
    public void PcDevice_AllowsSettingAllProperties() {
        // Arrange
        var device = new PcDevice();
        var mac = PhysicalAddress.Parse("11-22-33-44-55-66");
        var ip = IPAddress.Parse("192.168.1.100");
        var broadcast = IPAddress.Parse("192.168.1.255");

        // Act
        device.Id = 1;
        device.Name = "TestPC";
        device.MacAddress = mac;
        device.IpAddress = ip;
        device.BroadcastAddress = broadcast;
        device.IsOnline = true;

        // Assert
        device.Id.Should().Be(1);
        device.Name.Should().Be("TestPC");
        device.MacAddress.Should().Be(mac);
        device.IpAddress.Should().Be(ip);
        device.BroadcastAddress.Should().Be(broadcast);
        device.IsOnline.Should().BeTrue();
    }

    [Fact]
    public void PcDevice_AllowsNullIPAddresses() {
        // Arrange & Act
        var device = new PcDevice {
            Id = 1,
            Name = "TestPC",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IpAddress = null,
            BroadcastAddress = null,
            IsOnline = false
        };

        // Assert
        device.IpAddress.Should().BeNull();
        device.BroadcastAddress.Should().BeNull();
    }

    [Fact]
    public void PcDevice_SupportsEmptyName() {
        // Arrange & Act
        var device = new PcDevice {
            Name = ""
        };

        // Assert
        device.Name.Should().Be("");
    }

    [Fact]
    public void PcDevice_SupportsLongNames() {
        // Arrange
        var longName = new string('A', 500);

        // Act
        var device = new PcDevice {
            Name = longName
        };

        // Assert
        device.Name.Should().Be(longName);
        device.Name.Length.Should().Be(500);
    }

    #endregion

    #region Object Behavior Tests

    [Fact]
    public void PcDevice_CanBeCreatedWithObjectInitializer() {
        // Arrange & Act
        var device = new PcDevice {
            Id = 10,
            Name = "MyPC",
            MacAddress = PhysicalAddress.Parse("FF-EE-DD-CC-BB-AA"),
            IsOnline = true
        };

        // Assert
        device.Should().NotBeNull();
        device.Id.Should().Be(10);
        device.Name.Should().Be("MyPC");
    }

    [Fact]
    public void PcDevice_MaintainsPropertyIndependence() {
        // Arrange
        var device1 = new PcDevice {
            Id = 1,
            Name = "PC1",
            MacAddress = PhysicalAddress.Parse("00-11-22-33-44-55"),
            IsOnline = true
        };

        var device2 = new PcDevice {
            Id = 2,
            Name = "PC2",
            MacAddress = PhysicalAddress.Parse("AA-BB-CC-DD-EE-FF"),
            IsOnline = false
        };

        // Act & Assert
        device1.Id.Should().NotBe(device2.Id);
        device1.Name.Should().NotBe(device2.Name);
        device1.MacAddress.Should().NotBe(device2.MacAddress);
        device1.IsOnline.Should().NotBe(device2.IsOnline);
    }

    #endregion
}
