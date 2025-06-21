using Catalyst.Common.Errors;
using Catalyst.Common.Models;
using Catalyst.Common.Services;
using Catalyst.Core.MediatR;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Catalyst.Core;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase // Make abstract
{
    protected readonly ICorrelationIdProvider CorrelationIdProvider;
    protected readonly IMediatrContext MediatrContext;

    // Use protected constructor for base class
    protected BaseController(IMediatrContext mediatrContext, ICorrelationIdProvider correlationIdProvider)
    {
        MediatrContext = mediatrContext ?? throw new ArgumentNullException(nameof(mediatrContext));
        CorrelationIdProvider = correlationIdProvider ?? throw new ArgumentNullException(nameof(correlationIdProvider));
    }

    // Keep helper methods protected
    protected async Task<IActionResult> HandleRequestAsync<TRequest, TResponse>(TRequest request)
        where TRequest : IBaseRequest<Result<TResponse>>
    {
        return await HandleAsync<TRequest, TResponse>(request);
    }

    protected async Task<IActionResult> HandleCommandAsync<TCommand, TResponse>(TCommand command)
        where TCommand : IBaseCommand<Result<TResponse>>
    {
        return await HandleAsync<TCommand, TResponse>(command);
    }

    protected async Task<IActionResult> HandleAsync<TRequest, TResponse>(TRequest request)
        where TRequest : IRequest<Result<TResponse>>
    {
        if (request == null)
        {
            // Handle null request argument if necessary, though MediatR might handle this
            return BadRequest(CreateProblemDetailsForError("Invalid request.", StatusCodes.Status400BadRequest));
        }

        var result = await MediatrContext.Send<TRequest, TResponse>(request);
        return HandleResult(result);
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            // Check if the successful value is null (e.g., for commands with no return value)
            if (result.Value == null)
            {
                // Return NoContent for successful commands without a specific return value
                return NoContent();
            }

            // Handle specific response wrappers (Consider simplifying this)
            var responseValue = result.Value; // Cast should be safe if TResponse matches
            object apiResponseValue = responseValue switch
            {
                IPaginationResponse paginated => new PaginatedApiResponse<object> // Use object or T
                {
                    Value = (paginated as dynamic).Data, // Need dynamic or reflection if T isn't correct
                    TotalCount = paginated.TotalCount,
                    TotalPages = paginated.TotalPages,
                    PageNumber = paginated.PageNumber,
                    PageSize = paginated.PageSize,
                    IsSuccess = true,
                    CorrelationId = CorrelationIdProvider.CorrelationId ?? Guid.Empty // Add CorrelationId
                },
                BaseResponse br => new BaseApiResponse<object> // Use object or T
                {
                    Value = (br as dynamic).Data, // Need dynamic or reflection
                    IsSuccess = true,
                    CorrelationId = CorrelationIdProvider.CorrelationId ?? Guid.Empty // Add CorrelationId
                },
                _ => responseValue // Return the raw value if not a known wrapper
            };


            // If apiResponseValue is still the raw value, wrap it in a standard success response
            if (ReferenceEquals(apiResponseValue, responseValue) && !(responseValue is BaseApiResponse<T>))
            {
                apiResponseValue = new BaseApiResponse<T>
                {
                    IsSuccess = true,
                    Value = responseValue,
                    CorrelationId = CorrelationIdProvider.CorrelationId ?? Guid.Empty
                };
            }

            // Determine appropriate success status code (200 OK or 201 Created etc.)
            // This might require more context from the request/response. Defaulting to OK.
            return Ok(apiResponseValue);
        }

        // Failure case
        var primaryError = result.Errors.FirstOrDefault();
        var statusCode = StatusCodes.Status500InternalServerError;
        var errorType = CommonErrorTypes.UnknownError;

        if (primaryError is BaseError baseError) // Check if it's our custom error type
        {
            statusCode = baseError.ErrorType.StatusCode;
            errorType = baseError.ErrorType;
        }
        else if (primaryError != null && primaryError.Metadata.TryGetValue("statusCode", out var codeObj) &&
                 codeObj is int code)
        {
            // Fallback if status code was added to metadata of a standard FluentResults.Error
            statusCode = code;
        }
        // Further logic to map specific FluentResults error types (e.g., Validation) if needed

        return StatusCode(statusCode, CreateProblemDetails(result, statusCode, errorType));
    }

    private ProblemDetails CreateProblemDetails<T>(Result<T> result, int statusCode, ErrorType? primaryErrorType = null)
    {
        var title = primaryErrorType?.Description ?? GetTitleForStatusCode(statusCode);

        return new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}", // Link to HTTP status code definition
            Title = title,
            Status = statusCode,
            Detail = result.Errors.FirstOrDefault()?.Message, // Provide primary error message as detail
            Instance = HttpContext.Request.Path, // Indicate which request path caused this
            Extensions =
            {
                ["traceId"] = CorrelationIdProvider.CorrelationId?.ToString(), // Consistent traceId
                ["isSuccess"] = false,
                ["errors"] = result.Errors.Select(e => new // Simplified error structure
                {
                    // Use ErrorType properties if available, otherwise fallback
                    Code = (e as BaseError)?.ErrorType.CustomCode ?? statusCode.ToString(),
                    Message = e.Message,
                    // Optionally add Metadata if needed for client debugging
                    // Metadata = e.Metadata.Count > 0 ? e.Metadata : null
                }).ToList()
            }
        };
    }

    // Helper for single error scenarios
    private ProblemDetails CreateProblemDetailsForError(string message, int statusCode)
    {
        return new ProblemDetails
        {
            /* ... fill similarly ... */
        };
    }


    // Static helper for title mapping
    private static string GetTitleForStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
        StatusCodes.Status500InternalServerError => "Internal Server Error",
        StatusCodes.Status502BadGateway => "Bad Gateway",
        StatusCodes.Status408RequestTimeout => "Request Timeout",
        StatusCodes.Status503ServiceUnavailable => "Service Unavailable",
        StatusCodes.Status429TooManyRequests => "Too Many Requests", // Added
        _ => "An error occurred"
    };
}