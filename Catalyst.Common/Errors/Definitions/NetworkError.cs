namespace Catalyst.Common.Errors.Definitions;

public sealed class NetworkError : BaseError
{
    public NetworkError(string message, ErrorType errorType) : base(message, errorType)
    {
    }

    public NetworkError(string message) : this(message, CommonErrorTypes.NetworkUnavailable)
    {
    }
}