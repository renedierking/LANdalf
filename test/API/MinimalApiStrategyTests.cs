using API.MinimalApi;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.Tests;

public class MinimalApiStrategyTests {
    [Fact]
    public void AddMinimalApiStrategies_RegistersMinimalApiStrategies() {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMinimalApiStrategies(typeof(PcDeviceMinimalApiStrategy).Assembly);
        using var serviceProvider = services.BuildServiceProvider();

        // Assert
        var strategies = serviceProvider.GetServices<IMinimalApiStrategy>();
        strategies.Should().ContainSingle(strategy => strategy.GetType() == typeof(PcDeviceMinimalApiStrategy));
    }

    [Fact]
    public void MapMinimalApiStrategies_MapsAllProvidedStrategies() {
        // Arrange
        var strategy = new FakeMinimalApiStrategy();
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();
        RouteGroupBuilder group = app.MapGroup("/api");

        // Act
        group.MapMinimalApiStrategies([strategy]);

        // Assert
        strategy.WasCalled.Should().BeTrue();
    }

    private sealed class FakeMinimalApiStrategy : IMinimalApiStrategy {
        public bool WasCalled { get; private set; }

        public void MapEndpoints(RouteGroupBuilder group) {
            WasCalled = true;
        }
    }
}
