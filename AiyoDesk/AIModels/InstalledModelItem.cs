
namespace AiyoDesk.AIModels;

public class InstalledModelItem
{
    public string PathName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public ModelType ModelType { get; set; } = ModelType.chat;
    public bool FunctionCall { get; set; }
    public bool Vision { get; set; }
    public string? VisionModel { get; set; }
    public string FunctionCallText
    {
        get { return (FunctionCall ? "是" : "否"); }
    }
    public string VisionText
    {
        get { return (Vision ? "是" : "否"); }
    }
    public string SubDir
    {
        get
        {
            if (FunctionCall && Vision)
            {
                return "llm_tools_vision";
            }
            else if (FunctionCall)
            {
                return "llm_tools";
            }
            else if (Vision)
            {
                return "llm_vision";
            }
            else
            {
                return "llm";
            }
        }
    }
}
