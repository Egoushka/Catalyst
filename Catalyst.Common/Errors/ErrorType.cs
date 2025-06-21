namespace Catalyst.Common.Errors;

public class ErrorType
{
    public ErrorType(string customCode, string description, int statusCode)
    {
        CustomCode = customCode;
        Description = description;
        StatusCode = statusCode;
    }

    public string CustomCode { get; }
    public string Description { get; }
    public int StatusCode { get; }
}