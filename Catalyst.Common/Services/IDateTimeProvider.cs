namespace Catalyst.Common.Services;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
    DateTimeOffset LocalNow { get; }

    DateOnly Today { get; }

    public DateTimeOffset GetTimeInZone(TimeZoneInfo timeZone);
}

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    public DateTimeOffset LocalNow => DateTimeOffset.Now;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow.Date);


    public DateTimeOffset GetTimeInZone(TimeZoneInfo timeZone)
    {
        return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
    }
}