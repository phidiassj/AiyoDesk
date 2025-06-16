
namespace AiyoDesk.AIModels;

public class InstalledModelItem
{
    public string PathName { get; set; } = string.Empty;
    public ModelType ModelType { get; set; } = ModelType.chat;
    public bool FunctionCall { get; set; }
    public bool Vision { get; set; }
    public string? VisionModel { get; set; }
}
