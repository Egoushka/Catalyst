namespace Catalyst.Common.Errors.Definitions;

public sealed class FtpError : BaseError
{
    public FtpError(string message, ErrorType errorType, string ftpStatusCode = null)
        : base(message, errorType)
    {
        FtpStatusCode = ftpStatusCode;
    }

    public FtpError(string message, string ftpStatusCode = null)
        : this(message, CommonErrorTypes.Internal.FtpConnectionFailed, ftpStatusCode)
    {
    }

    public string FtpStatusCode { get; }

    public override string ToString()
        => $"[FtpError] {Message} (Type: {ErrorType.CustomCode}, FTP Status: {FtpStatusCode ?? "N/A"})";
}