using Catalyst.Common.Errors;
using Catalyst.Common.Errors.Definitions;
using FluentAssertions;
using FluentResults;

namespace Catalyst.Tests.Unit.Common.Extensions;

public class ResultExtensionsTests : BaseTest
{
    private class CustomTestError : BaseError
    {
        public string SpecificProperty { get; }
        public CustomTestError(string message, string specificProperty) : base(message, CommonErrorTypes.UnknownError)
        {
            SpecificProperty = specificProperty;
        }
    }

    [Fact]
    public void HasError_TError_ShouldReturnTrue_WhenResultContainsErrorOfType()
    {
        // Arrange
        var result = Result.Fail(new NotFoundError("Resource not found"));

        // Act & Assert
        result.HasError<NotFoundError>().Should().BeTrue();
    }

    [Fact]
    public void HasError_TError_ShouldReturnFalse_WhenResultDoesNotContainErrorOfType()
    {
        // Arrange
        var result = Result.Fail(new ValidationError("Invalid input"));

        // Act & Assert
        result.HasError<NotFoundError>().Should().BeFalse();
    }

    [Fact]
    public void HasError_TError_ShouldReturnFalse_WhenResultIsSuccess()
    {
        // Arrange
        var result = Result.Ok();

        // Act & Assert
        result.HasError<NotFoundError>().Should().BeFalse();
    }
    
    [Fact]
    public void HasError_TError_Predicate_ShouldReturnTrue_WhenErrorMatchesPredicate()
    {
        // Arrange
        var specificValue = Faker.Lorem.Word();
        var result = Result.Fail(new CustomTestError("Test error", specificValue));

        // Act & Assert
        result.HasError<CustomTestError>(e => e.SpecificProperty == specificValue).Should().BeTrue();
    }

    [Fact]
    public void HasError_TError_Predicate_ShouldReturnFalse_WhenErrorDoesNotMatchPredicate()
    {
        // Arrange
        var result = Result.Fail(new CustomTestError("Test error", "ActualValue"));

        // Act & Assert
        result.HasError<CustomTestError>(e => e.SpecificProperty == "ExpectedValue").Should().BeFalse();
    }
    
    [Fact]
    public void HasError_TError_Predicate_ShouldReturnFalse_WhenResultDoesNotContainErrorOfType()
    {
        // Arrange
        var result = Result.Fail(new ValidationError("Input error"));

        // Act & Assert
        result.HasError<CustomTestError>(e => e.SpecificProperty == "AnyValue").Should().BeFalse();
    }

    [Fact]
    public void HasMetadataKey_ShouldReturnTrue_WhenErrorHasMetadataKey()
    {
        // Arrange
        var error = new DatabaseError("Test", CommonErrorTypes.UnknownError);
        error.Metadata.Add("CustomKey", "CustomValue");

        // Act & Assert
        error.HasMetadataKey("CustomKey").Should().BeTrue();
        error.HasMetadataKey("errorType").Should().BeTrue();
    }

    [Fact]
    public void HasMetadataKey_ShouldReturnFalse_WhenErrorDoesNotHaveMetadataKey()
    {
        // Arrange
        var error = new DatabaseError("Test", CommonErrorTypes.UnknownError);

        // Act & Assert
        error.HasMetadataKey("NonExistentKey").Should().BeFalse();
    }
}