using FluentAssertions;
using LANdalf.UI.Components;
using Xunit;

namespace UI.Tests.Components;

public class CancellationTokenComponentBaseTests {

    #region CancellationToken Property Tests

    [Fact]
    public void CancellationToken_ReturnsValidToken_WhenFirstAccessed() {
        // Arrange
        var component = new TestCancellationTokenComponent();

        // Act
        var token = component.CancellationToken;

        // Assert
        token.Should().NotBe(default);
        token.CanBeCanceled.Should().BeTrue();
        token.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public void CancellationToken_ReturnsSameToken_OnMultipleAccesses() {
        // Arrange
        var component = new TestCancellationTokenComponent();

        // Act
        var token1 = component.CancellationToken;
        var token2 = component.CancellationToken;

        // Assert
        token1.Should().Be(token2);
    }

    [Fact]
    public void CancellationToken_LazyInitializesCancellationTokenSource() {
        // Arrange
        var component = new TestCancellationTokenComponent();

        // Act
        var token = component.CancellationToken;
        var token2 = component.CancellationToken;

        // Assert
        // Both should be the same, indicating single CancellationTokenSource
        token.Should().Be(token2);
    }

    #endregion

    #region Dispose Method Tests

    [Fact]
    public void Dispose_CancelsCancellationTokenSource() {
        // Arrange
        var component = new TestCancellationTokenComponent();
        var token = component.CancellationToken;

        // Act
        component.Dispose();

        // Assert
        token.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public void Dispose_DisposesResources_Properly() {
        // Arrange
        var component = new TestCancellationTokenComponent();
        var token = component.CancellationToken;

        // Act
        component.Dispose();

        // Assert - Should complete without throwing
        token.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes_WithoutThrow() {
        // Arrange
        var component = new TestCancellationTokenComponent();
        _ = component.CancellationToken;

        // Act & Assert - Should not throw
        component.Dispose();
        component.Dispose();
        component.Dispose();
    }

    [Fact]
    public void Dispose_DoesNotThrow_WhenCancellationTokenNeverAccessed() {
        // Arrange
        var component = new TestCancellationTokenComponent();
        // Don't access CancellationToken

        // Act & Assert - Should not throw
        component.Dispose();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CancellationToken_CanBeCancelledDuringAsyncOperation() {
        // Arrange
        var component = new TestCancellationTokenComponent();
        var token = component.CancellationToken;

        // Act
        var task = Task.Delay(5000, token);
        component.Dispose(); // This cancels the token

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
    }

    [Fact]
    public void CancellationToken_StatesAreConsistent_BeforeAndAfterDispose() {
        // Arrange
        var component = new TestCancellationTokenComponent();
        var token = component.CancellationToken;

        var isRequestedBefore = token.IsCancellationRequested;

        // Act
        component.Dispose();

        var isRequestedAfter = token.IsCancellationRequested;

        // Assert
        isRequestedBefore.Should().BeFalse();
        isRequestedAfter.Should().BeTrue();
    }

    [Fact]
    public void CancellationToken_CanBePassedToAsyncMethod() {
        // Arrange
        var component = new TestCancellationTokenComponent();
        var token = component.CancellationToken;

        // Act
        var delayTask = Task.Delay(100, token);

        // Assert
        delayTask.Should().NotBeNull();
        delayTask.IsCompletedSuccessfully.Should().BeFalse();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Dispose_BeforeTokenAccess_Works() {
        // Arrange
        var component = new TestCancellationTokenComponent();

        // Act & Assert
        component.Dispose(); // Dispose before accessing token
    }

    [Fact]
    public void CancellationToken_IsNotCancelled_InitialState() {
        // Arrange
        var component = new TestCancellationTokenComponent();

        // Act
        var token = component.CancellationToken;

        // Assert
        token.IsCancellationRequested.Should().BeFalse();
        token.CanBeCanceled.Should().BeTrue();
    }

    [Fact]
    public void MultipleComponents_HaveIndependentTokens() {
        // Arrange
        var component1 = new TestCancellationTokenComponent();
        var component2 = new TestCancellationTokenComponent();

        var token1 = component1.CancellationToken;
        var token2 = component2.CancellationToken;

        // Act
        component1.Dispose();

        // Assert
        token1.IsCancellationRequested.Should().BeTrue();
        token2.IsCancellationRequested.Should().BeFalse();
    }

    #endregion
}

/// <summary>
/// Concrete implementation of CancellationTokenComponentBase for testing
/// </summary>
public class TestCancellationTokenComponent : CancellationTokenComponentBase {
}
