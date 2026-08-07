namespace PocketFlow.Helpers;

public static class DateHelper
{
    public static string ToRelativeLocalString(this DateTime utcDate, TimeZoneInfo localZone)
    {
        var localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, localZone);
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localZone);
        
        var datePart = localDate.Date;
        var today = now.Date;
        
        if (datePart == today)
        {
            return $"Hoy, {localDate:HH:mm}";
        }
        
        if (datePart == today.AddDays(-1))
        {
            return $"Ayer, {localDate:HH:mm}";
        }
        
        return localDate.ToString("d MMM, HH:mm", new System.Globalization.CultureInfo("es-ES"));
    }
}
