using Microsoft.AspNetCore.Http;

namespace Catalyst.Common.Errors;

public static class CommonErrorTypes
{
    // General
    public static readonly ErrorType UnknownError =
        new("0.0", "An unknown error occurred.", StatusCodes.Status500InternalServerError);

    // Validation
    public static readonly ErrorType InvalidInput =
        new("3.1", "Invalid input.", StatusCodes.Status422UnprocessableEntity);

    // Not Found
    public static readonly ErrorType NotFound = new("4.1", "Resource not found.", StatusCodes.Status404NotFound);

    // Unauthorized
    public static readonly ErrorType
        InvalidCredentials = new("5.1", "Invalid credentials.", StatusCodes.Status401Unauthorized);

    public static readonly ErrorType AccessDenied = new("5.2", "Access Denied.", StatusCodes.Status403Forbidden);

    // Conflict
    public static readonly ErrorType ResourceConflict = new("6.1", "Resource conflict.", StatusCodes.Status409Conflict);

    // Operation Cancelled
    public static readonly ErrorType UserCancelled =
        new("7.1", "Operation cancelled by user.", StatusCodes.Status400BadRequest);

    // Timeout Errors
    public static readonly ErrorType RequestTimeout =
        new("9.1", "Request timed out", StatusCodes.Status408RequestTimeout);

    // Network Errors
    public static readonly ErrorType NetworkUnavailable =
        new("10.1", "Network unavailable", StatusCodes.Status502BadGateway);

    public static class Internal
    {
        public static readonly ErrorType DatabaseQueryFailed =
            new("8.2", "Database query failed.", StatusCodes.Status502BadGateway);

        public static readonly ErrorType FtpConnectionFailed =
            new("8.3", "FTP connection failed.", StatusCodes.Status502BadGateway);

        public static readonly ErrorType UnhandledException =
            new("8.5", "An unhandled exception occurred.", StatusCodes.Status500InternalServerError);

        public static readonly ErrorType HttpRequestFailed =
            new("8.6", "HTTP request failed.", StatusCodes.Status502BadGateway);
    }
}