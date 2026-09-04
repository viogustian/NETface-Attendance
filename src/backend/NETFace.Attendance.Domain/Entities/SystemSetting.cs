namespace NETFace.Attendance.Domain.Entities;

public class SystemSetting
{
    public string Key { get; private set; }
    public string Value { get; private set; }

    private SystemSetting()
    {
        Key = string.Empty;
        Value = string.Empty;
    }

    public SystemSetting(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        Key = key;
        Value = value;
    }

    public void UpdateValue(string value)
    {
        Value = value;
    }
}
