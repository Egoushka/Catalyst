namespace Catalyst.Common.Errors.Definitions;

public sealed class ExceptionalError : BaseError
{
    public ExceptionalError(string message, Exception ex) : base(message, CommonErrorTypes.Internal.UnhandledException)
    {
        Exception = ex;
        Metadata["ExceptionType"] = ex.GetType().FullName;
    }

    public Exception Exception { get; }

    public override string ToString()
        => $"[ExceptionalError] {Message} (Type: {ErrorType.CustomCode}, Exception: {Exception.GetType().Name})";
}