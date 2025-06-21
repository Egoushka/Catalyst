using System.Net;
using Catalyst.Common.Errors;
using Catalyst.Common.Errors.Definitions;
using FluentAssertions;

namespace Catalyst.Tests.Unit.Common.Errors;

public class HttpErrorTests : BaseErrorTest<HttpError>
{
    protected override HttpError CreateError(string message, ErrorType errorType) => new(message, errorType);
    protected override HttpError CreateErrorWithDefaultType(string message) => new(message);
    protected override ErrorType GetDefaultErrorType() => CommonErrorTypes.Internal.HttpRequestFailed;

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(null)] // Test case where statusCode is not provided
    public void Constructor_WithHttpStatusCode_ShouldSetStatusCodeInMetadata(HttpStatusCode? statusCode)
    {
        // Arrange
        var message = Faker.Lorem.Sentence();

        // Act
        var error = new HttpError(message, statusCode);

        // Assert
        error.Message.Should().Be(message);
        error.ErrorType.Should().Be(CommonErrorTypes.Internal.HttpRequestFailed);
        if (statusCode.HasValue)
        {
            error.Metadata.Should().ContainKey("statusCode").WhoseValue.Should().Be((int)statusCode.Value);
        }
        else
        {
            // Default statusCode from ErrorType
            error.Metadata.Should().ContainKey("statusCode").WhoseValue.Should()
                .Be(CommonErrorTypes.Internal.HttpRequestFailed.StatusCode);
        }
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString_WithStatusCode()
    {
        // Arrange
        var message = "HTTP request failure";
        var statusCode = HttpStatusCode.ServiceUnavailable;
        var error = new HttpError(message, statusCode);
        var expectedString =
            $"[HttpError] {message} (Type: {CommonErrorTypes.Internal.HttpRequestFailed.CustomCode}, HTTP Status: {(int)statusCode})";

        // Act
        var result = error.ToString();

        // Assert
        result.Should().Be(expectedString);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString_WithoutStatusCodeExplicitlySetInConstructor()
    {
        // Arrange
        var message = "Generic HTTP failure";
        // This constructor overload uses CommonErrorTypes.Internal.HttpRequestFailed, which has a default status code
        var error = new HttpError(message);
        var expectedErrorType = CommonErrorTypes.Internal.HttpRequestFailed;
        var expectedString =
            $"[HttpError] {message} (Type: {expectedErrorType.CustomCode}, HTTP Status: {expectedErrorType.StatusCode})";


        // Act
        var result = error.ToString();

        // Assert
        result.Should().Be(expectedString);
    }
}