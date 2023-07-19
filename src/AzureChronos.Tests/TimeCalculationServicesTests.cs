using AzureChronos.Functions.Services;
using NUnit.Framework;

namespace AzureChronos.Tests;

public class TimeCalculationServicesTests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void GetNextOccurrence_ValidCronExpression_ReturnsNextOccurrence()
    {
        // Arrange
        var cronExpression = "0 0 * * *"; // Every day at midnight
        var expectedNextOccurrence = DateTime.UtcNow.Date.AddDays(1);

        // Act
        var nextOccurrence = TimeCalculationService.GetNextOccurrence(cronExpression);

        // Assert
        Assert.AreEqual(expectedNextOccurrence, nextOccurrence?.Date);
    }

    [Test]
    public void GetNextOccurrence_InvalidCronExpression_ReturnsNull()
    {
        // Arrange
        var cronExpression = "invalid expression";

        // Act
        var nextOccurrence = TimeCalculationService.GetNextOccurrence(cronExpression);

        // Assert
        Assert.IsNull(nextOccurrence);
    }

    [Test]
    public void IsWithinNext30Minutes_DateTimeWithin30Minutes_ReturnsTrue()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddMinutes(15);
        var validationMinutes = 30;

        // Act
        var result = TimeCalculationService.CheckDateTimeInRange(dateTime, validationMinutes);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void IsWithinNext30Minutes_DateTimeOutside30Minutes_ReturnsFalse()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddMinutes(45);
        var validationMinutes = 30;

        // Act
        var result = TimeCalculationService.CheckDateTimeInRange(dateTime, validationMinutes);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void IsWithinNext30Minutes_NullDateTime_ReturnsFalse()
    {
        // Arrange
        DateTime? dateTime = null;
        var validationMinutes = 30;

        // Act
        var result = TimeCalculationService.CheckDateTimeInRange(dateTime, validationMinutes);

        // Assert
        Assert.IsFalse(result);
    }
}