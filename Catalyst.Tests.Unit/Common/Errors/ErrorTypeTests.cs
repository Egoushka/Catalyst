using Catalyst.Common.Errors;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace Catalyst.Tests.Unit.Common.Errors;

public class ErrorTypeTests : BaseTest
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var customCode = Faker.Random.AlphaNumeric(5);
        var description = Faker.Lorem.Sentence();
        var statusCode = Faker.PickRandom(StatusCodes.Status400BadRequest, StatusCodes.Status500InternalServerError,
            StatusCodes.Status404NotFound);

        // Act
        var errorType = new ErrorType(customCode, description, statusCode);

        // Assert
        errorType.CustomCode.Should().Be(customCode);
        errorType.Description.Should().Be(description);
        errorType.StatusCode.Should().Be(statusCode);
    }
}