using System.Reflection;
using Catalyst.Common.Errors.Definitions;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Catalyst.Core.MediatR.Behaviours;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IContext
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;

        if (!_validators.Any())
        {
            _logger.LogDebug("No validators found for request type: {RequestType}", requestType);
            return await next();
        }

        _logger.LogDebug("Starting validation for request type: {RequestType}", requestType);

        var context = new ValidationContext<TRequest>(request);
        var validationResults =
            await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            _logger.LogWarning("Validation failed for request type: {RequestType} - with {FailureCount} errors.",
                requestType, failures.Count);
            foreach (var failure in failures)
            {
                _logger.LogDebug(
                    "Validation Failure: PropertyName={PropertyName}, ErrorMessage={ErrorMessage}, ErrorCode={ErrorCode}"
                    , failure.PropertyName, failure.ErrorMessage, failure.ErrorCode);
            }

            return CreateValidationFailureResponse(failures);
        }

        _logger.LogDebug("Validation successful for request type: {RequestType}", requestType);
        return await next();
    }


    private TResponse CreateValidationFailureResponse(List<ValidationFailure> failures)
    {
        var notFoundError = failures.FirstOrDefault(f => f.ErrorCode == "NotFound");
        if (notFoundError != null)
        {
            return CreateNotFoundErrorResponse(notFoundError);
        }

        var errors = failures.Select(f => new ValidationError($"{f.PropertyName}: {f.ErrorMessage}")
        {
            Metadata = { ["errorCode"] = f.ErrorCode }
        }).ToList();

        // Handle non-generic Result
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Fail(errors);
        }

        // Handle generic Result<T>
        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            // Get the type argument (T) of Result<T>
            Type resultValueType = typeof(TResponse).GetGenericArguments()[0];

            // Create an instance of Result<T> using its parameterless constructor
            var resultType = typeof(Result<>).MakeGenericType(resultValueType);

            //Use of parameterless constructor.
            var resultInstance = Activator.CreateInstance(resultType);

            //Crucial Step: Find and invoke the .WithError() method
            MethodInfo withErrorMethod =
                resultType.GetMethod("WithError", new[] { typeof(IError) }); // Get WithError method
            if (withErrorMethod == null)
            {
                throw new InvalidOperationException("The 'WithError' method was not found on the Result<> type.");
            }

            foreach (var error in errors)
            {
                //Calling method on resultInstance for each error
                resultInstance =
                    withErrorMethod.Invoke(resultInstance,
                        new object[] { error }); // Invoke the method on resultInstance
            }

            return (TResponse)resultInstance;
        }

        throw new ValidationException("Validation failed", failures);
    }

    private TResponse CreateNotFoundErrorResponse(ValidationFailure failure)
    {
        var notFoundError = new NotFoundError($"{failure.PropertyName}: {failure.ErrorMessage}");

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Fail(notFoundError);
        }

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            Type resultValueType = typeof(TResponse).GetGenericArguments()[0];
            var resultType = typeof(Result<>).MakeGenericType(resultValueType);
            var resultInstance = Activator.CreateInstance(resultType);
            MethodInfo withErrorMethod = resultType.GetMethod("WithError", new[] { typeof(IError) });
            if (withErrorMethod == null)
            {
                throw new InvalidOperationException("The 'WithError' method was not found on the Result<> type.");
            }

            // Invoke .WithError() with the NotFoundError
            resultInstance = withErrorMethod.Invoke(resultInstance, new object[] { notFoundError });
            return (TResponse)resultInstance;
        }

        throw new ValidationException("Validation failed (NotFound)",
            new[] { failure }); // Or a custom NotFoundException
    }
}