namespace Catalyst.Common.Errors.Definitions;

public sealed class UnauthorizedError : BaseError
{
    public UnauthorizedError(string message, ErrorType errorType) : base(message, errorType)
    {
    }

    public UnauthorizedError(string message) : this(message, CommonErrorTypes.InvalidCredentials)
    {
    }
}