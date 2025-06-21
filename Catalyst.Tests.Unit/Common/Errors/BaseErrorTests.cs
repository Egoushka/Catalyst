using Catalyst.Common.Errors;
using FluentAssertions;

namespace Catalyst.Tests.Unit.Common.Errors;

public abstract class BaseErrorTest<TError> : BaseTest
    where TError : BaseError
{
    protected abstract TError CreateError(string message, ErrorType errorType);
    protected abstract TError CreateErrorWithDefaultType(string message);
    protected abstract ErrorType GetDefaultErrorType();

    [Fact]
    public void Constructor_WithMessageAndErrorType_ShouldInitializePropertiesAndMetadata()
    {
        // Arrange
        var message = Faker.Lorem.Sentence();
        var errorType = new ErrorType(
            Faker.Random.AlphaNumeric(5),
            Faker.Lorem.Sentence(),
            Faker.Random.Int(400, 599));

        // Act
        var error = CreateError(message, errorType);

        // Assert
        Assert.Equal(error.Message, message);
        Assert.Equal(error.ErrorType, errorType);

        var isErrorTypeMetadataPresent = error.Metadata.TryGetValue("errorType", out var errorTypeValue);
        var isErrorDescriptionMetadataPresent =
            error.Metadata.TryGetValue("errorDescription", out var errorDescriptionValue);
        var isStatusCodeMetadataPresent = error.Metadata.TryGetValue("statusCode", out var statusCodeValue);

        isErrorDescriptionMetadataPresent.Should()
            .BeTrue(
                $"Expected 'errorDescription' metadata to be present, but it was not. Value: {errorDescriptionValue}");
        isErrorTypeMetadataPresent.Should()
            .BeTrue($"Expected 'errorType' metadata to be present, but it was not. Value: {errorTypeValue}");
        isStatusCodeMetadataPresent.Should()
            .BeTrue($"Expected 'statusCode' metadata to be present, but it was not. Value: {statusCodeValue}");

        errorTypeValue.Should().Be(errorType.CustomCode);

        message.Should().Be(error.Message);
        errorDescriptionValue.Should().Be(errorType.Description);
        statusCodeValue.Should().Be(errorType.StatusCode);
    }

    [Fact]
    public void Constructor_WithOnlyMessage_ShouldUseDefaultErrorType()
    {
        // Arrange
        var message = Faker.Lorem.Sentence();
        var defaultErrorType = GetDefaultErrorType();

        // Act
        var error = CreateErrorWithDefaultType(message);

        // Assert
        message.Should().Be(error.Message);
        defaultErrorType.Should().Be(error.ErrorType);

        var isErrorTypeMetadataPresent = error.Metadata.TryGetValue("errorType", out var errorTypeValue);
        var isErrorDescriptionMetadataPresent =
            error.Metadata.TryGetValue("errorDescription", out var errorDescriptionValue);
        var isStatusCodeMetadataPresent = error.Metadata.TryGetValue("statusCode", out var statusCodeValue);

        isErrorTypeMetadataPresent.Should()
            .BeTrue($"Expected 'errorType' metadata to be present, but it was not. Value: {errorTypeValue}");
        isErrorDescriptionMetadataPresent.Should()
            .BeTrue(
                $"Expected 'errorDescription' metadata to be present, but it was not. Value: {errorDescriptionValue}");
        isStatusCodeMetadataPresent.Should()
            .BeTrue($"Expected 'statusCode' metadata to be present, but it was not. Value: {statusCodeValue}");

        errorTypeValue.Should().Be(defaultErrorType.CustomCode);

        message.Should().Be(error.Message);
        errorDescriptionValue.Should().Be(defaultErrorType.Description);
        statusCodeValue.Should().Be(defaultErrorType.StatusCode);
    }
}