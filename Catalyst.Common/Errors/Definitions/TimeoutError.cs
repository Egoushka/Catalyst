namespace Catalyst.Common.Errors.Definitions;

public sealed class TimeoutError : BaseError
{
    public TimeoutError(string message, ErrorType errorType) : base(message, errorType)
    {
    }

    public TimeoutError(string message) : this(message, CommonErrorTypes.RequestTimeout)
    {
    }
}