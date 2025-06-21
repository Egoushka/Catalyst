using FluentResults;

namespace Catalyst.Common.Extensions;

public static class ResultExtensions
{
    public static bool HasError<TError>(this Result result) where TError : IError
    {
        return result.Errors.Any(e => e is TError);
    }

    public static bool HasError<TError>(this Result result, Func<TError, bool> predicate) where TError : IError
    {
        return result.Errors.Any(e => e is TError error && predicate(error));
    }

    public static bool HasMetadataKey(this IError error, string key)
    {
        return error.Metadata.ContainsKey(key);
    }
}