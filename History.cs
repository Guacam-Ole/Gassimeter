namespace GassiMeter;

public class HistoryEntry
{
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public double Value { get; set; }
}

public class History
{
    private static readonly List<HistoryEntry> HistoryData = [];

    private static void DeleteHistoryOlderThan(int minutes)
    {
        HistoryData.RemoveAll(q => q.Date != DateOnly.FromDateTime(DateTime.Now));
        HistoryData.RemoveAll(q => q.Time < TimeOnly.FromDateTime(DateTime.Now).AddMinutes(-minutes));
    }

    public void AddHistoryData(DateTime time, double value)
    {
        if (HistoryData.Exists(q => q.Time == TimeOnly.FromDateTime(time))) return;
        HistoryData.Add(
            new HistoryEntry
            {
                Date = DateOnly.FromDateTime(time), Time = TimeOnly.FromDateTime(time), Value = value
            });
    }

    public static Dictionary<int, double> GetHistoryData(int minutes)
    {
        DeleteHistoryOlderThan(minutes);
        var now = DateTime.Now;
        Dictionary<int, double> values = [];
        for (var i = -1; i >= -minutes; i--)
        {
            var historyTime = now.AddMinutes(i);
            var entry = HistoryData.FirstOrDefault(q =>
                q.Time.Hour == historyTime.Hour && q.Time.Minute == historyTime.Minute);
            values.Add(i, entry?.Value ?? -1);
        }

        return values;
    }
}