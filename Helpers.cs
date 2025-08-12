using System.ComponentModel;

namespace GassiMeter;

public static class Helpers
{
    public static DateTime ToDateTime(this long timestamp) {
    {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return  dateTime.AddSeconds( timestamp ).ToLocalTime();
    }}
}