
namespace AiyoDesk.AIModels;

public class RecommandModelItem
{
    public string ModelId { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Serial { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string LicenseUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ModelType ModelType { get; set; } = ModelType.chat;
    public HardwareRequiredType HardwareRequired { get; set; } = HardwareRequiredType.medium;
    public bool FunctionCall { get; set; }
    public bool Vision { get; set; }
}

public enum ModelType
{
    chat,
    embedding,
    speech
}

public enum HardwareRequiredType
{
    low,
    medium,
    high,
}
