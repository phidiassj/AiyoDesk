namespace AiyoCoveX.Shared;

public class CoreService : ICoreService
{
    public EventHandler<string>? SpeechText { get; set; }

    public Func<bool> IsRecording { get; set; } = null!;
    public Func<bool> ToogleRecording { get; set; } = null!;
    public Func<List<DeviceInfo>> GetRecordingDevices { get; set; } = null!;
}


public interface ICoreService
{
    EventHandler<string>? SpeechText { get; set; }
    Func<bool> IsRecording { get; set; }
    Func<bool> ToogleRecording { get; set; }
    Func<List<DeviceInfo>> GetRecordingDevices { get; set; }
}
