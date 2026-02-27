using FluentAssertions;
using LANdalf.UI.Services;
using Xunit;

namespace UI.Tests.Services;

public class DeviceValidationServiceTests {
    private readonly DeviceValidationService _service;

    public DeviceValidationServiceTests() {
        _service = new DeviceValidationService();
    }

    #region ValidateName Tests

    [Fact]
    public void ValidateName_WithValidName_ReturnsNull() {
        // Arrange
        var name = "My PC";

        // Act
        var result = _service.ValidateName(name);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateName_WithNull_ReturnsError() {
        // Arrange
        string? name = null;

        // Act
        var result = _service.ValidateName(name);

        // Assert
        result.Should().Be("Name is required");
    }

    [Fact]
    public void ValidateName_WithEmptyString_ReturnsError() {
        // Arrange
        var name = "";

        // Act
        var result = _service.ValidateName(name);

        // Assert
        result.Should().Be("Name is required");
    }

    [Fact]
    public void ValidateName_WithWhitespaceOnly_ReturnsError() {
        // Arrange
        var name = "   ";

        // Act
        var result = _service.ValidateName(name);

        // Assert
        result.Should().Be("Name is required");
    }

    [Fact]
    public void ValidateName_WithMaxLength_ReturnsNull() {
        // Arrange
        var name = new string('A', 64);

        // Act
        var result = _service.ValidateName(name);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateName_ExceedsMaxLength_ReturnsError() {
        // Arrange
        var name = new string('A', 65);

        // Act
        var result = _service.ValidateName(name);

        // Assert
        result.Should().Be("Name must be 64 characters or less");
    }

    #endregion

    #region ValidateMacAddress Tests

    [Theory]
    [InlineData("00:11:22:33:44:55")]
    [InlineData("AA:BB:CC:DD:EE:FF")]
    [InlineData("aa:bb:cc:dd:ee:ff")]
    [InlineData("aA:bB:cC:dD:eE:fF")]
    public void ValidateMacAddress_WithColonFormat_ReturnsNull(string macAddress) {
        // Act
        var result = _service.ValidateMacAddress(macAddress);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("00-11-22-33-44-55")]
    [InlineData("AA-BB-CC-DD-EE-FF")]
    [InlineData("aa-bb-cc-dd-ee-ff")]
    [InlineData("aA-bB-cC-dD-eE-fF")]
    public void ValidateMacAddress_WithHyphenFormat_ReturnsNull(string macAddress) {
        // Act
        var result = _service.ValidateMacAddress(macAddress);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("001122334455")]
    [InlineData("AABBCCDDEEFF")]
    [InlineData("aabbccddeeff")]
    [InlineData("aAbBcCdDeEfF")]
    public void ValidateMacAddress_WithNoSeparatorFormat_ReturnsNull(string macAddress) {
        // Act
        var result = _service.ValidateMacAddress(macAddress);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateMacAddress_WithNull_ReturnsError() {
        // Arrange
        string? macAddress = null;

        // Act
        var result = _service.ValidateMacAddress(macAddress);

        // Assert
        result.Should().Be("MAC Address is required");
    }

    [Fact]
    public void ValidateMacAddress_WithEmptyString_ReturnsError() {
        // Arrange
        var macAddress = "";

        // Act
        var result = _service.ValidateMacAddress(macAddress);

        // Assert
        result.Should().Be("MAC Address is required");
    }

    [Theory]
    [InlineData("00:11:22:33:44")]  // Too short
    [InlineData("00:11:22:33:44:55:66")]  // Too long
    [InlineData("GG:HH:II:JJ:KK:LL")]  // Invalid hex characters
    [InlineData("00.11.22.33.44.55")]  // Wrong separator
    [InlineData("00:11:22:33:44:5")]  // Last octet too short
    [InlineData("0011223344")]  // Too short (no separator format)
    [InlineData("00112233445566")]  // Too long (no separator format)
    [InlineData("AA:BB-CC:DD-EE:FF")]  // Mixed separators
    [InlineData("AA-BB:CC-DD:EE-FF")]  // Mixed separators
    public void ValidateMacAddress_WithInvalidFormat_ReturnsError(string macAddress) {
        // Act
        var result = _service.ValidateMacAddress(macAddress);

        // Assert
        result.Should().Contain("Invalid MAC Address format");
    }

    #endregion

    #region ValidateIpAddress Tests

    [Fact]
    public void ValidateIpAddress_WithNull_ReturnsNull() {
        // Arrange
        string? ipAddress = null;

        // Act
        var result = _service.ValidateIpAddress(ipAddress);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateIpAddress_WithEmptyString_ReturnsNull() {
        // Arrange
        var ipAddress = "";

        // Act
        var result = _service.ValidateIpAddress(ipAddress);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateIpAddress_WithWhitespaceOnly_ReturnsNull() {
        // Arrange
        var ipAddress = "   ";

        // Act
        var result = _service.ValidateIpAddress(ipAddress);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    [InlineData("255.255.255.255")]
    [InlineData("0.0.0.0")]
    public void ValidateIpAddress_WithValidIPv4_ReturnsNull(string ipAddress) {
        // Act
        var result = _service.ValidateIpAddress(ipAddress);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("::1")]
    [InlineData("fe80::1")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    public void ValidateIpAddress_WithValidIPv6_ReturnsNull(string ipAddress) {
        // Act
        var result = _service.ValidateIpAddress(ipAddress);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("256.1.1.1")]  // Octet out of range
    [InlineData("192.168.1.1.1")]  // Too many octets
    [InlineData("not-an-ip")]
    [InlineData("192.168.1.abc")]
    public void ValidateIpAddress_WithInvalidFormat_ReturnsError(string ipAddress) {
        // Act
        var result = _service.ValidateIpAddress(ipAddress);

        // Assert
        result.Should().Be("Invalid IP Address format");
    }

    #endregion
}
