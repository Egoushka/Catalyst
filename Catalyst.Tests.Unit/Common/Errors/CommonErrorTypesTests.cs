using Catalyst.Common.Errors;
using Microsoft.AspNetCore.Http;

namespace Catalyst.Tests.Unit.Common.Errors;

public class CommonErrorTypesTests
{
    [Fact]
    public void InvalidInput_ShouldHaveCorrectProperties()
    {
        // Act
        var errorType = CommonErrorTypes.InvalidInput;

        // Assert
        Assert.Equal("3.1", errorType.CustomCode);
        Assert.Equal("Invalid input.", errorType.Description);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, errorType.StatusCode);
    }

    [Fact]
    public void NotFound_ShouldHaveCorrectProperties()
    {
        // Act
        var errorType = CommonErrorTypes.NotFound;

        // Assert
        Assert.Equal("4.1", errorType.CustomCode);
        Assert.Equal("Resource not found.", errorType.Description);
        Assert.Equal(StatusCodes.Status404NotFound, errorType.StatusCode);
    }

    [Fact]
    public void Internal_DatabaseQueryFailed_ShouldHaveCorrectProperties()
    {
        // Act
        var errorType = CommonErrorTypes.Internal.DatabaseQueryFailed;

        // Assert
        Assert.Equal("8.2", errorType.CustomCode);
        Assert.Equal("Database query failed.", errorType.Description);
        Assert.Equal(StatusCodes.Status502BadGateway, errorType.StatusCode);
    }
}