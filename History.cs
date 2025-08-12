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

    private void DeleteHistoryOlderThan(int minutes)
    {
        HistoryData.RemoveAll(q => q.Date != DateOnly.FromDateTime(DateTime.Now));
        HistoryData.RemoveAll(q => q.Time < TimeOnly.FromDateTime(DateTime.Now).AddMinutes(-minutes));
    }

    public void AddHistoryData(DateTime time, double value)
    {
        if (HistoryData.Exists(q => q.Time == TimeOnly.FromDateTime(time))) return;
        HistoryData.Add(new HistoryEntry
            { Date = DateOnly.FromDateTime(time), Time = TimeOnly.FromDateTime(time), Value = value });
    }

    public Dictionary<int, double> GetHistoryData(int minutes)
    {
        DeleteHistoryOlderThan(minutes);
        var currentTime = TimeOnly.FromDateTime(DateTime.Now);
        Dictionary<int, double> values = [];
        for (var i = -1; i >= -minutes; i--)
        {
            var entry = HistoryData.FirstOrDefault(q => q.Time == currentTime.AddMinutes(minutes));
            values.Add(i, entry?.Value ?? -1);
        }

        return values;
    }
}