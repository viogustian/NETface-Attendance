using System.Collections.Concurrent;
using NETFace.Attendance.Application.Interfaces;

namespace NETFace.Attendance.Infrastructure.Services.Recognition;

public class SpoofingDetectionService : ISpoofingDetectionService
{
    private readonly ConcurrentDictionary<string, int> _failureCounts = new();
    private readonly int _threshold;

    public SpoofingDetectionService(int threshold = 3)
    {
        _threshold = threshold;
    }

    public int RecordUnknownFace(string key)
    {
        return _failureCounts.AddOrUpdate(key, 1, (_, count) => count + 1);
    }

    public void RecordSuccess(string key)
    {
        _failureCounts.TryRemove(key, out _);
    }

    public bool IsSpoofingSuspected(string key)
    {
        return _failureCounts.TryGetValue(key, out var count) && count >= _threshold;
    }

    public void Reset(string key)
    {
        _failureCounts.TryRemove(key, out _);
    }
}
