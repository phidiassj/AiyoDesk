
using AiyoDesk.CommanandTools;
using System.Diagnostics;
using System;
using System.IO;

namespace AiyoDesk.AIModels;

public class RecommandModelItem
{
    public string ModelId { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Serial { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string VisionUrl { get; set; } = string.Empty;
    public string LicenseUrl { get; set; } = string.Empty;
    public string OfficialUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ModelType ModelType { get; set; } = ModelType.chat;
    public HardwareRequiredType HardwareRequired { get; set; } = HardwareRequiredType.medium;
    public bool FunctionCall { get; set; }
    public bool Vision { get; set; }

    public bool IsModelInstalled()
    {
        string modelPath = string.Empty;
        string mmprojPath = string.Empty;

        modelPath = Path.Combine(CommandLineExecutor.GetAIModelsPath(), getModelTypePathname(), Name, "model.gguf");
        if (Vision)
        {
            mmprojPath = Path.Combine(CommandLineExecutor.GetAIModelsPath(), getModelTypePathname(), Name, "mmproj.gguf");
        }

        if (string.IsNullOrWhiteSpace(modelPath)) return false;
        if (!File.Exists(modelPath)) return false;
        if (Vision && (string.IsNullOrWhiteSpace(mmprojPath) || !File.Exists(mmprojPath))) return false;
        return true;
    }

    public void ModelInstall()
    {
        string scriptPath = Path.Combine(CommandLineExecutor.GetScriptRootPath(), "hf_download.ps1");
        if (!File.Exists(scriptPath)) throw new FileNotFoundException(scriptPath);

        string targetPath = Path.Combine(CommandLineExecutor.GetAIModelsPath(), getModelTypePathname(), Name, "model.gguf");
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -ModelUrl \"{DownloadUrl}\" -TargetPath \"{targetPath}\"",
            UseShellExecute = false,
            CreateNoWindow = false
        };
        var proc = Process.Start(psi);
        if (proc == null) throw new Exception("執行安裝 script 發生錯誤");
        proc.WaitForExit();

        if (!Vision) return;

        targetPath = Path.Combine(CommandLineExecutor.GetAIModelsPath(), getModelTypePathname(), Name, "mmproj.gguf");
        ProcessStartInfo psi2 = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -ModelUrl \"{VisionUrl}\" -OutputDir \"{targetPath}\"",
            UseShellExecute = false,
            CreateNoWindow = false
        };
        var proc2 = Process.Start(psi2);
        if (proc2 == null) throw new Exception("執行安裝 script 發生錯誤");
        proc2.WaitForExit();

    }

    public void ModelUninstall()
    {
        if (!IsModelInstalled()) return;

        string targetPath = Path.Combine(CommandLineExecutor.GetAIModelsPath(), getModelTypePathname(), Name);
        if (!Directory.Exists(targetPath)) return;

        Directory.Delete(targetPath, true);


    }

    private string getModelTypePathname()
    {
        if (!Vision && !FunctionCall)
        {
            return "llm";
        }
        else if (!Vision && FunctionCall)
        {
            return "llm_tools";
        }
        else if (Vision && !FunctionCall)
        {
            return "llm_vision";
        }
        else if (Vision && FunctionCall)
        {
            return "llm_tools_vision";
        }
        else
        {
            return string.Empty;
        }
    }

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
