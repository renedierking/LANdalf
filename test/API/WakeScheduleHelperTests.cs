using API.Models;
using API.Services;
using FluentAssertions;
using Xunit;

namespace API.Tests.Services;

public class WakeScheduleHelperTests {

    #region GenerateCronExpression Tests

    [Fact]
    public void GenerateCronExpression_OneTimeSchedule_ReturnsCorrectCron() {
        // Arrange
        var scheduledTime = new TimeOnly(7, 30);
        string? daysOfWeek = null;

        // Act
        var cronExpression = WakeScheduleHelper.GenerateCronExpression(scheduledTime, daysOfWeek);

        // Assert
        cronExpression.Should().Be("30 7 * * *");
    }

    [Fact]
    public void GenerateCronExpression_RecurringSchedule_ReturnsCorrectCron() {
        // Arrange
        var scheduledTime = new TimeOnly(8, 0);
        string daysOfWeek = "1,2,3,4,5"; // Monday-Friday

        // Act
        var cronExpression = WakeScheduleHelper.GenerateCronExpression(scheduledTime, daysOfWeek);

        // Assert
        cronExpression.Should().Be("0 8 * * 1,2,3,4,5");
    }

    [Fact]
    public void GenerateCronExpression_MidnightTime_HandlesCorrectly() {
        // Arrange
        var scheduledTime = new TimeOnly(0, 0);
        string daysOfWeek = "0,6"; // Sunday and Saturday

        // Act
        var cronExpression = WakeScheduleHelper.GenerateCronExpression(scheduledTime, daysOfWeek);

        // Assert
        cronExpression.Should().Be("0 0 * * 0,6");
    }

    [Fact]
    public void GenerateCronExpression_LateDayTime_HandlesCorrectly() {
        // Arrange
        var scheduledTime = new TimeOnly(23, 59);
        string daysOfWeek = "1";

        // Act
        var cronExpression = WakeScheduleHelper.GenerateCronExpression(scheduledTime, daysOfWeek);

        // Assert
        cronExpression.Should().Be("59 23 * * 1");
    }

    [Theory]
    [InlineData("", "30 7 * * *")]
    [InlineData("   ", "30 7 * * *")]
    public void GenerateCronExpression_EmptyOrWhitespaceDays_TreatsAsOneTime(string daysOfWeek, string expected) {
        // Arrange
        var scheduledTime = new TimeOnly(7, 30);

        // Act
        var cronExpression = WakeScheduleHelper.GenerateCronExpression(scheduledTime, daysOfWeek);

        // Assert
        cronExpression.Should().Be(expected);
    }

    [Theory]
    [InlineData("0", "0 9 * * 0")]       // Sunday only
    [InlineData("6", "0 9 * * 6")]       // Saturday only
    [InlineData("1,3,5", "0 9 * * 1,3,5")] // Mon, Wed, Fri
    public void GenerateCronExpression_DifferentDayCombinations_ReturnsCorrectCron(string daysOfWeek, string expectedCron) {
        // Arrange
        var scheduledTime = new TimeOnly(9, 0);

        // Act
        var cronExpression = WakeScheduleHelper.GenerateCronExpression(scheduledTime, daysOfWeek);

        // Assert
        cronExpression.Should().Be(expectedCron);
    }

    #endregion

    #region ValidateDaysOfWeek Tests

    [Fact]
    public void ValidateDaysOfWeek_NullValue_ReturnsTrue() {
        // Arrange
        string? daysOfWeek = null;

        // Act
        var result = WakeScheduleHelper.ValidateDaysOfWeek(daysOfWeek);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateDaysOfWeek_EmptyString_ReturnsTrue() {
        // Arrange
        string? daysOfWeek = "";

        // Act
        var result = WakeScheduleHelper.ValidateDaysOfWeek(daysOfWeek);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateDaysOfWeek_WhitespaceString_ReturnsTrue() {
        // Arrange
        string? daysOfWeek = "   ";

        // Act
        var result = WakeScheduleHelper.ValidateDaysOfWeek(daysOfWeek);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("0")]
    [InlineData("6")]
    [InlineData("1,2,3,4,5")]
    [InlineData("0,6")]
    [InlineData("0,1,2,3,4,5,6")]
    public void ValidateDaysOfWeek_ValidFormats_ReturnsTrue(string daysOfWeek) {
        // Act
        var result = WakeScheduleHelper.ValidateDaysOfWeek(daysOfWeek);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("7")]
    [InlineData("-1")]
    [InlineData("1,7")]
    [InlineData("abc")]
    [InlineData("1,2,abc")]
    [InlineData("1.5")]
    public void ValidateDaysOfWeek_InvalidFormats_ReturnsFalse(string daysOfWeek) {
        // Act
        var result = WakeScheduleHelper.ValidateDaysOfWeek(daysOfWeek);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateCronExpression_InvalidDaysOfWeek_ThrowsArgumentException() {
        // Arrange
        var scheduledTime = new TimeOnly(8, 0);
        string daysOfWeek = "7,8,9"; // Invalid days

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            WakeScheduleHelper.GenerateCronExpression(scheduledTime, daysOfWeek));

        exception.ParamName.Should().Be("daysOfWeek");
        exception.Message.Should().Contain("Invalid daysOfWeek format");
    }

    [Fact]
    public void GenerateCronExpression_InvalidDaysOfWeekWithLetters_ThrowsArgumentException() {
        // Arrange
        var scheduledTime = new TimeOnly(8, 0);
        string daysOfWeek = "1,2,abc"; // Invalid format

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            WakeScheduleHelper.GenerateCronExpression(scheduledTime, daysOfWeek));

        exception.ParamName.Should().Be("daysOfWeek");
    }

    #endregion

    #region UpdateScheduleExecution Tests

    [Fact]
    public void UpdateScheduleExecution_EnabledSchedule_CalculatesNextExecution() {
        // Arrange
        var schedule = new WakeSchedule {
            ScheduledTime = new TimeOnly(10, 0),
            DaysOfWeek = "1,2,3,4,5", // Weekdays
            Enabled = true
        };
        var utcNow = new DateTime(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc); // Thursday morning

        // Act
        WakeScheduleHelper.UpdateScheduleExecution(schedule, utcNow);

        // Assert
        schedule.CronExpression.Should().Be("0 10 * * 1,2,3,4,5");
        schedule.NextExecution.Should().NotBeNull();
        schedule.NextExecution.Should().BeAfter(utcNow);
    }

    [Fact]
    public void UpdateScheduleExecution_DisabledSchedule_SetsNextExecutionToNull() {
        // Arrange
        var schedule = new WakeSchedule {
            ScheduledTime = new TimeOnly(10, 0),
            DaysOfWeek = "1,2,3,4,5",
            Enabled = false
        };
        var utcNow = DateTime.UtcNow;

        // Act
        WakeScheduleHelper.UpdateScheduleExecution(schedule, utcNow);

        // Assert
        schedule.CronExpression.Should().Be("0 10 * * 1,2,3,4,5");
        schedule.NextExecution.Should().BeNull();
    }

    [Fact]
    public void UpdateScheduleExecution_OneTimeSchedule_CalculatesNextExecution() {
        // Arrange
        var schedule = new WakeSchedule {
            ScheduledTime = new TimeOnly(15, 30),
            DaysOfWeek = null,
            Enabled = true
        };
        var utcNow = new DateTime(2026, 4, 10, 12, 0, 0, DateTimeKind.Utc);

        // Act
        WakeScheduleHelper.UpdateScheduleExecution(schedule, utcNow);

        // Assert
        schedule.CronExpression.Should().Be("30 15 * * *");
        schedule.NextExecution.Should().NotBeNull();
    }

    #endregion

    #region ShouldExecute Tests

    [Fact]
    public void ShouldExecute_EnabledWithPastTime_ReturnsTrue() {
        // Arrange
        var utcNow = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc);
        var schedule = new WakeSchedule {
            Enabled = true,
            NextExecution = new DateTime(2026, 4, 10, 9, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = WakeScheduleHelper.ShouldExecute(schedule, utcNow);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldExecute_EnabledWithFutureTime_ReturnsFalse() {
        // Arrange
        var utcNow = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc);
        var schedule = new WakeSchedule {
            Enabled = true,
            NextExecution = new DateTime(2026, 4, 10, 11, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = WakeScheduleHelper.ShouldExecute(schedule, utcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldExecute_DisabledSchedule_ReturnsFalse() {
        // Arrange
        var utcNow = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc);
        var schedule = new WakeSchedule {
            Enabled = false,
            NextExecution = new DateTime(2026, 4, 10, 9, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = WakeScheduleHelper.ShouldExecute(schedule, utcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldExecute_NullNextExecution_ReturnsFalse() {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var schedule = new WakeSchedule {
            Enabled = true,
            NextExecution = null
        };

        // Act
        var result = WakeScheduleHelper.ShouldExecute(schedule, utcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldExecute_ExactTime_ReturnsTrue() {
        // Arrange
        var utcNow = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc);
        var schedule = new WakeSchedule {
            Enabled = true,
            NextExecution = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = WakeScheduleHelper.ShouldExecute(schedule, utcNow);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region MarkAsExecuted Tests

    [Fact]
    public void MarkAsExecuted_OneTimeSchedule_DisablesAndClearsNextExecution() {
        // Arrange
        var schedule = new WakeSchedule {
            ScheduledTime = new TimeOnly(10, 0),
            DaysOfWeek = null, // One-time
            Enabled = true,
            NextExecution = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc)
        };
        var utcNow = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc);

        // Act
        WakeScheduleHelper.MarkAsExecuted(schedule, utcNow);

        // Assert
        schedule.LastExecuted.Should().Be(utcNow);
        schedule.Enabled.Should().BeFalse();
        schedule.NextExecution.Should().BeNull();
    }

    [Fact]
    public void MarkAsExecuted_RecurringSchedule_UpdatesNextExecution() {
        // Arrange
        var schedule = new WakeSchedule {
            ScheduledTime = new TimeOnly(10, 0),
            DaysOfWeek = "1,2,3,4,5", // Weekdays
            Enabled = true,
            NextExecution = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc)
        };
        var utcNow = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc);

        // Act
        WakeScheduleHelper.MarkAsExecuted(schedule, utcNow);

        // Assert
        schedule.LastExecuted.Should().Be(utcNow);
        schedule.Enabled.Should().BeTrue(); // Should remain enabled
        schedule.NextExecution.Should().NotBeNull();
        schedule.NextExecution.Should().BeAfter(utcNow);
    }

    [Fact]
    public void MarkAsExecuted_RecurringSchedule_CalculatesCorrectNextOccurrence() {
        // Arrange
        var schedule = new WakeSchedule {
            ScheduledTime = new TimeOnly(10, 0),
            DaysOfWeek = "1,2,3,4,5", // Mon-Fri
            Enabled = true
        };
        var utcNow = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc); // Friday 10 AM

        // Act
        WakeScheduleHelper.MarkAsExecuted(schedule, utcNow);

        // Assert
        schedule.LastExecuted.Should().Be(utcNow);
        schedule.NextExecution.Should().NotBeNull();
        // Next occurrence should be Monday at 10:00 AM
        var expectedDate = new DateTime(2026, 4, 13, 10, 0, 0, DateTimeKind.Utc); // Monday
        schedule.NextExecution.Value.Date.Should().Be(expectedDate.Date);
        schedule.NextExecution.Value.Hour.Should().Be(10);
        schedule.NextExecution.Value.Minute.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void MarkAsExecuted_OneTimeScheduleWithEmptyDays_DisablesSchedule(string daysOfWeek) {
        // Arrange
        var schedule = new WakeSchedule {
            ScheduledTime = new TimeOnly(10, 0),
            DaysOfWeek = daysOfWeek,
            Enabled = true
        };
        var utcNow = DateTime.UtcNow;

        // Act
        WakeScheduleHelper.MarkAsExecuted(schedule, utcNow);

        // Assert
        schedule.Enabled.Should().BeFalse();
        schedule.NextExecution.Should().BeNull();
    }

    #endregion

    #region Edge Cases and DST Tests

    [Fact]
    public void GenerateCronExpression_AllDaysOfWeek_ReturnsCorrectPattern() {
        // Arrange
        var scheduledTime = new TimeOnly(12, 0);
        string daysOfWeek = "0,1,2,3,4,5,6"; // All days

        // Act
        var cronExpression = WakeScheduleHelper.GenerateCronExpression(scheduledTime, daysOfWeek);

        // Assert
        cronExpression.Should().Be("0 12 * * 0,1,2,3,4,5,6");
    }

    [Fact]
    public void UpdateScheduleExecution_SetsCorrectCronExpression() {
        // Arrange
        var schedule = new WakeSchedule {
            ScheduledTime = new TimeOnly(7, 15),
            DaysOfWeek = "0,6",
            Enabled = true
        };
        var utcNow = DateTime.UtcNow;

        // Act
        WakeScheduleHelper.UpdateScheduleExecution(schedule, utcNow);

        // Assert
        schedule.CronExpression.Should().Be("15 7 * * 0,6");
    }

    [Fact]
    public void ShouldExecute_OneSecondBeforeExecution_ReturnsFalse() {
        // Arrange
        var nextExecution = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc);
        var utcNow = nextExecution.AddSeconds(-1);
        var schedule = new WakeSchedule {
            Enabled = true,
            NextExecution = nextExecution
        };

        // Act
        var result = WakeScheduleHelper.ShouldExecute(schedule, utcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldExecute_OneSecondAfterExecution_ReturnsTrue() {
        // Arrange
        var nextExecution = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc);
        var utcNow = nextExecution.AddSeconds(1);
        var schedule = new WakeSchedule {
            Enabled = true,
            NextExecution = nextExecution
        };

        // Act
        var result = WakeScheduleHelper.ShouldExecute(schedule, utcNow);

        // Assert
        result.Should().BeTrue();
    }

    #endregion
}
