namespace NETFace.Attendance.Application.Interfaces;

public interface ISpoofingDetectionService
{
    int RecordUnknownFace(string key);
    void RecordSuccess(string key);
    bool IsSpoofingSuspected(string key);
    void Reset(string key);
}
