using Catalyst.Common.Errors;
using Catalyst.Common.Errors.Definitions;
using Catalyst.Common.Models;
using Catalyst.Common.Services;
using Catalyst.Core;
using Catalyst.Core.MediatR;
using Catalyst.Tests.Unit.Common;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Catalyst.Tests.Unit.Core;

public class BaseControllerHandleResultTests : BaseTest
{
    private readonly Mock<IMediatrContext> _mockMediatrContext;
    private readonly Mock<ICorrelationIdProvider> _mockCorrelationIdProvider;
    private readonly TestableBaseController _controller;
    private readonly Guid _correlationId = Guid.NewGuid();

    public BaseControllerHandleResultTests()
    {
        _mockMediatrContext = new Mock<IMediatrContext>();
        _mockCorrelationIdProvider = new Mock<ICorrelationIdProvider>();
        _mockCorrelationIdProvider.Setup(c => c.CorrelationId).Returns(_correlationId);

        _controller = new TestableBaseController(_mockMediatrContext.Object, _mockCorrelationIdProvider.Object);
    }

    [Fact]
    public void HandleResult_WithSuccessfulResultAndValue_ShouldReturnOkObjectResult()
    {
        // Arrange
        var data = new { Message = "Success" };
        var successResult = Result.Ok(data);

        // Act
        var actionResult = _controller.PublicHandleResult(successResult);

        // Assert
        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
    
    public record TestPaginatedData { public int Id { get; set; } }
    public record TestPaginationResponse : PaginationResponse<TestPaginatedData> {}


    [Fact]
    public void HandleResult_WithSuccessfulPaginatedResult_ShouldReturnOkObjectResultWithPaginatedApiResponse()
    {
        // Arrange
        var paginatedData = new TestPaginationResponse
        {
            Data = [new TestPaginatedData { Id = 1 }],
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1
            // CorrelationId is set by BaseController
        };
        var successResult = Result.Ok<IPaginationResponse>(paginatedData); // Cast to IPaginationResponse

        // Act
        var actionResult = _controller.PublicHandleResult(successResult);

        // Assert
        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        var apiResponse = okResult.Value.Should().BeOfType<PaginatedApiResponse<object>>().Subject; // object or TestPaginatedData
        apiResponse.IsSuccess.Should().BeTrue();
        apiResponse.PageNumber.Should().Be(paginatedData.PageNumber);
        apiResponse.PageSize.Should().Be(paginatedData.PageSize);
        apiResponse.TotalCount.Should().Be(paginatedData.TotalCount);
        apiResponse.CorrelationId.Should().Be(_correlationId);
        // ((IEnumerable<object>)apiResponse.Value).Should().HaveCount(1); // Adjust assertion based on actual T
    }


    [Fact]
    public void HandleResult_WithSuccessfulResultAndNullValue_ShouldReturnNoContentResult()
    {
        // Arrange
        var successResult = Result.Ok<object?>(null); // Explicitly object? for null

        // Act
        var actionResult = _controller.PublicHandleResult(successResult);

        // Assert
        actionResult.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void HandleResult_WithFailedResultAndBaseError_ShouldReturnStatusCodeResultWithProblemDetails()
    {
        // Arrange
        var notFoundError = new NotFoundError("Resource not found");
        var failedResult = Result.Fail<object>(notFoundError);

        // Act
        var actionResult = _controller.PublicHandleResult(failedResult);

        // Assert
        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Resource not found.");
        problemDetails.Detail.Should().Be(notFoundError.Message);
        problemDetails.Extensions["traceId"].Should().Be(_correlationId.ToString());
    }
    
    [Fact]
    public void HandleResult_WithFailedResultAndGenericFluentErrorWithStatusCodeMetadata_ShouldReturnCorrectStatusCode()
    {
        // Arrange
        var genericError = new Error("A generic failure")
            .WithMetadata("statusCode", StatusCodes.Status400BadRequest);
        var failedResult = Result.Fail<object>(genericError);

        // Act
        var actionResult = _controller.PublicHandleResult(failedResult);

        // Assert
        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("An unknown error occurred.");
        problemDetails.Detail.Should().Be(genericError.Message);
    }

    [Fact]
    public void HandleResult_WithFailedResultAndMultipleErrors_ShouldUseFirstErrorForDetail()
    {
        // Arrange
        var error1 = new ValidationError("Field A is required.");
        var error2 = new ConflictError("Resource already exists.");
        var failedResult = Result.Fail<object>(new List<IError> { error1, error2 });

        // Act
        var actionResult = _controller.PublicHandleResult(failedResult);

        // Assert
        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(CommonErrorTypes.InvalidInput.StatusCode); // From first error (ValidationError)
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Detail.Should().Be(error1.Message); // Detail comes from the first error
        
        problemDetails.Extensions.TryGetValue("errors", out object? errorsValue);
        //TODO check array of errors
    }
}