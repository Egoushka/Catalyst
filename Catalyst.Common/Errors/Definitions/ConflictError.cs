namespace Catalyst.Common.Errors.Definitions;

public sealed class ConflictError : BaseError
{
    public ConflictError(string message, ErrorType errorType) : base(message, errorType)
    {
    }

    public ConflictError(string message) : this(message, CommonErrorTypes.ResourceConflict)
    {
    }
}