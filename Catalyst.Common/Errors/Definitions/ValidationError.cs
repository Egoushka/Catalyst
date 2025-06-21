namespace Catalyst.Common.Errors.Definitions;

public sealed class ValidationError : BaseError
{
    public ValidationError(string message, ErrorType errorType) : base(message, errorType)
    {
    }

    public ValidationError(string message) : this(message, CommonErrorTypes.InvalidInput)
    {
    }
}