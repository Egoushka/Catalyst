using System.Net;

namespace Catalyst.Common.Errors.Definitions;

public sealed class HttpError : BaseError
{
    public HttpError(string message, ErrorType errorType, HttpStatusCode? statusCode = null)
        : base(message, errorType)
    {
        if (statusCode.HasValue)
            Metadata["statusCode"] = (int)statusCode.Value;
    }

    public HttpError(string message, HttpStatusCode? statusCode = null)
        : this(message, CommonErrorTypes.Internal.HttpRequestFailed, statusCode)
    {
    }

    public override string ToString()
    {
        var statusCode = Metadata.GetValueOrDefault("statusCode", "N/A");
        return $"[HttpError] {Message} (Type: {ErrorType.CustomCode}, HTTP Status: {statusCode})";
    }
}