namespace Catalyst.Common.Errors.Definitions;

public sealed class OperationCanceledError : BaseError
{
    public OperationCanceledError(string message, ErrorType errorType) : base(message, errorType)
    {
    }

    public OperationCanceledError(string message) : this(message, CommonErrorTypes.UserCancelled)
    {
    }
}