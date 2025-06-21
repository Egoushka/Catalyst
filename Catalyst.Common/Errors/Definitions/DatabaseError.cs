namespace Catalyst.Common.Errors.Definitions;

public sealed class DatabaseError : BaseError
{
    public DatabaseError(string message, ErrorType errorType) : base(message, errorType)
    {
    }

    public DatabaseError(string message) : this(message, CommonErrorTypes.Internal.DatabaseQueryFailed)
    {
    }
}