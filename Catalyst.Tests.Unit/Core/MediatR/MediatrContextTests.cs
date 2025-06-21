using System.Data.Common;
using System.Net;
using System.Net.Sockets;
using Catalyst.Common.Errors;
using Catalyst.Common.Errors.Definitions;
using Catalyst.Core.MediatR;
using Catalyst.Tests.Unit.Common;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using ExceptionalError = Catalyst.Common.Errors.Definitions.ExceptionalError;

namespace Catalyst.Tests.Unit.Core.MediatR;

public record TestMediatrRequest : IRequest<Result<TestMediatrResponse>>;
public record TestMediatrResponse { public string Data { get; init; } = string.Empty; }

public class MediatrContextTests : BaseTest
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<MediatrContext>> _mockLogger;
    private readonly MediatrContext _mediatrContext;

    public MediatrContextTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<MediatrContext>>();
        _mediatrContext = new MediatrContext(_mockMediator.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Send_WhenMediatorSucceeds_ReturnsSuccessResult()
    {
        // Arrange
        var request = new TestMediatrRequest();
        var expectedResponse = new TestMediatrResponse { Data = "Success" };
        var successResult = Result.Ok(expectedResponse);

        _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _mediatrContext.Send<TestMediatrRequest, TestMediatrResponse>(request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(expectedResponse);
        _mockMediator.Verify(m => m.Send(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Send_WhenMediatorThrowsDbException_ReturnsFailResultWithDatabaseError()
    {
        // Arrange
        var request = new TestMediatrRequest();
        var dbException = new Mock<DbException>("Database error").Object; // Moq DbException

        _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbException);

        // Act
        var result = await _mediatrContext.Send<TestMediatrRequest, TestMediatrResponse>(request, TestContext.Current.CancellationToken);

        // Assert
        var databaseError = result.Errors.FirstOrDefault() as DatabaseError;
        
        databaseError.Should().NotBeNull();
        databaseError.Message.Should().Be(dbException.Message);
        _mockMediator.Verify(m => m.Send(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Send_WhenMediatorThrowsHttpRequestException_ReturnsFailResultWithHttpError()
    {
        // Arrange
        var request = new TestMediatrRequest();
        var httpException = new HttpRequestException("HTTP error", null);

        _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(httpException);

        // Act
        var result = await _mediatrContext.Send<TestMediatrRequest, TestMediatrResponse>(request);

        // Assert
        var httpError = result.Errors.OfType<HttpError>().FirstOrDefault();
        httpError.Should().NotBeNull();
        httpError.Message.Should().Be(httpException.Message);
        httpError.ErrorType.Should().Be(CommonErrorTypes.Internal.HttpRequestFailed);
        httpError.Metadata.Should().ContainKey("statusCode").WhoseValue.Should().Be((int)HttpStatusCode.BadGateway);
        _mockMediator.Verify(m => m.Send(request, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Send_WhenMediatorThrowsSocketException_ReturnsFailResultWithNetworkError()
    {
        // Arrange
        var request = new TestMediatrRequest();
        var socketException = new SocketException((int)SocketError.HostNotFound); // Example SocketError

        _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(socketException);

        // Act
        var result = await _mediatrContext.Send<TestMediatrRequest, TestMediatrResponse>(request);

        // Assert
        var networkError = result.Errors.FirstOrDefault() as NetworkError;
        
        networkError.Should().NotBeNull();
        networkError.Message.Should().Be(socketException.Message);
        _mockMediator.Verify(m => m.Send(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Send_WhenMediatorThrowsOperationCanceledException_ReturnsFailResultWithOperationCanceledError()
    {
        // Arrange
        var request = new TestMediatrRequest();
        var cts = new CancellationTokenSource();
        // Important: The exception must be created with the token that matches the one passed to Send
        var operationCanceledException = new OperationCanceledException("User cancelled", cts.Token); 
        
        _mockMediator.Setup(m => m.Send(request, cts.Token)) // Ensure the mocked Send uses this token
            .ThrowsAsync(operationCanceledException);

        // Act
        var result = await _mediatrContext.Send<TestMediatrRequest, TestMediatrResponse>(request, cts.Token);

        // Assert
        var operationCanceledError = result.Errors.FirstOrDefault() as OperationCanceledError;
        
        operationCanceledError.Should().NotBeNull();
        operationCanceledError.Message.Should().Be(operationCanceledException.Message);
        _mockMediator.Verify(m => m.Send(request, cts.Token), Times.Once);
    }
    
    [Fact]
    public async Task Send_WhenMediatorThrowsTaskCanceledException_ReturnsFailResultWithTimeoutError()
    {
        // Arrange
        var request = new TestMediatrRequest();
        // TaskCanceledException often implies a timeout, not necessarily user cancellation by token
        var taskCanceledException = new TaskCanceledException("Operation timed out"); 
        
        _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(taskCanceledException);

        // Act
        var result = await _mediatrContext.Send<TestMediatrRequest, TestMediatrResponse>(request);

        // Assert
        var operationCanceledError = result.Errors.FirstOrDefault() as OperationCanceledError;
        
        operationCanceledError.Should().NotBeNull();
        operationCanceledError.Message.Should().Be(taskCanceledException.Message);
        _mockMediator.Verify(m => m.Send(request, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Send_WhenMediatorThrowsGenericException_ReturnsFailResultWithExceptionalError()
    {
        // Arrange
        var request = new TestMediatrRequest();
        var genericException = new InvalidOperationException("Something went wrong");

        _mockMediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(genericException);

        // Act
        var result = await _mediatrContext.Send<TestMediatrRequest, TestMediatrResponse>(request);

        // Assert
        var exceptionalError = result.Errors.FirstOrDefault() as ExceptionalError;
        
        exceptionalError.Should().NotBeNull();
        exceptionalError.Message.Should().Be(genericException.Message);
        exceptionalError.Exception.Should().Be(genericException);
        _mockMediator.Verify(m => m.Send(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Send_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        TestMediatrRequest? request = null;

        // Act
        Func<Task> act = async () => await _mediatrContext.Send<TestMediatrRequest, TestMediatrResponse>(request!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("*request*");
    }
}