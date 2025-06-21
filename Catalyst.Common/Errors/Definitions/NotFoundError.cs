namespace Catalyst.Common.Errors.Definitions;

public sealed class NotFoundError : BaseError
{
    public NotFoundError(string message, ErrorType errorType) : base(message, errorType)
    {
    }

    public NotFoundError(string message) : this(message, CommonErrorTypes.NotFound)
    {
    }
}