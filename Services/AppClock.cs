namespace PocketFlow.Services;

public interface IAppClock
{
    DateTime UtcNow { get; }
    DateTime LocalNow { get; }
    TimeZoneInfo LocalTimeZone { get; }
    (DateTime StartUtc, DateTime EndUtc) GetCurrentWeekLimitsUtc();
}

public class AppClock : IAppClock
{
    private readonly TimeZoneInfo _timeZone;

    public AppClock(IConfiguration configuration)
    {
        try
        {
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
        }
        catch
        {
            try
            {
                _timeZone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
            }
            catch
            {
                _timeZone = TimeZoneInfo.Local;
            }
        }
    }

    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime LocalNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);
    public TimeZoneInfo LocalTimeZone => _timeZone;

    public (DateTime StartUtc, DateTime EndUtc) GetCurrentWeekLimitsUtc()
    {
        var localNow = LocalNow;
        int diff = (7 + (localNow.DayOfWeek - DayOfWeek.Monday)) % 7;
        var startLocal = localNow.Date.AddDays(-diff);
        var endLocal = startLocal.AddDays(7).AddTicks(-1);

        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, _timeZone);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, _timeZone);

        return (startUtc, endUtc);
    }
}
