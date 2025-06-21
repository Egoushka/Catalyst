using System.Runtime.InteropServices.JavaScript;
using FluentResults;

namespace Catalyst.Common.Errors;

public abstract class BaseError : Error
{
    protected BaseError(string message, ErrorType errorType) : base(message)
    {
        ErrorType = errorType;
        Metadata.Add("errorType", errorType.CustomCode);
        Metadata.Add("errorDescription", errorType.Description);
        Metadata.Add("statusCode", errorType.StatusCode);
    }

    public ErrorType ErrorType { get; }
}