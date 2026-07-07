using API.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Serilog;
using Xunit;

namespace API.Tests.Services;

public class SerilogFileLoggingConfiguratorTests {

    [Fact]
    public void TryConfigureFileSink_WhenDisabled_ReturnsFalse() {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["Serilog:File:Enabled"] = "false"
            })
            .Build();

        var loggerConfiguration = new LoggerConfiguration();

        // Act
        bool configured = SerilogFileLoggingConfigurator.TryConfigureFileSink(loggerConfiguration, configuration);

        // Assert
        configured.Should().BeFalse();
    }

    [Fact]
    public void TryConfigureFileSink_WhenEnabled_WritesToConfiguredFile() {
        // Arrange
        string tempDir = Path.Combine(Path.GetTempPath(), $"landalf-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        string filePath = Path.Combine(tempDir, "landalf-test.log");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["Serilog:File:Enabled"] = "true",
                ["Serilog:File:Path"] = filePath,
                ["Serilog:File:RollingInterval"] = "Infinite",
                ["Serilog:File:RetainedFileCountLimit"] = "5"
            })
            .Build();

        var loggerConfiguration = new LoggerConfiguration().MinimumLevel.Debug();

        try {
            // Act
            bool configured = SerilogFileLoggingConfigurator.TryConfigureFileSink(loggerConfiguration, configuration);

            using (var logger = loggerConfiguration.CreateLogger()) {
                logger.Information("file logger test message");
            }

            // Assert
            configured.Should().BeTrue();
            File.Exists(filePath).Should().BeTrue();
            string content = File.ReadAllText(filePath);
            content.Should().Contain("file logger test message");
        } finally {
            if (Directory.Exists(tempDir)) {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void TryConfigureFileSink_WithInvalidRollingInterval_UsesFallbackAndWritesFile() {
        // Arrange
        string tempDir = Path.Combine(Path.GetTempPath(), $"landalf-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        string filePath = Path.Combine(tempDir, "landalf-invalid-rolling-.log");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["Serilog:File:Enabled"] = "true",
                ["Serilog:File:Path"] = filePath,
                ["Serilog:File:RollingInterval"] = "NotARealInterval"
            })
            .Build();

        var loggerConfiguration = new LoggerConfiguration().MinimumLevel.Debug();

        try {
            // Act
            bool configured = SerilogFileLoggingConfigurator.TryConfigureFileSink(loggerConfiguration, configuration);

            using (var logger = loggerConfiguration.CreateLogger()) {
                logger.Information("fallback rolling interval message");
            }

            // Assert
            configured.Should().BeTrue();
            string[] createdFiles = Directory.GetFiles(tempDir, "landalf-invalid-rolling-*.log");
            createdFiles.Should().NotBeEmpty();
        } finally {
            if (Directory.Exists(tempDir)) {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
