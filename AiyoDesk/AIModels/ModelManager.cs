
using AiyoDesk.CommanandTools;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace AiyoDesk.AIModels;

public class ModelManager
{
    public EventHandler? InstalledStateChanged { get; set; }

    public List<RecommandModelItem> RecommandModels = null!;
    public List<InstalledModelItem> ChatModels = null!;

    public InstalledModelItem? UsingLlmModel { get; set; }

    public ModelManager()
    {
        loadRecommandModels();
        LoadInstalledModels();
    }

    private void loadRecommandModels()
    {
        RecommandModels = new();
        string jsonFilename = Path.Combine(CommandLineExecutor.GetAIModelsPath(), "RecommandModels.json");
        if (!File.Exists(jsonFilename)) return;

        try
        {
            string jsonContent = File.ReadAllText(jsonFilename);
            var jsonObj = JsonConvert.DeserializeObject<List<RecommandModelItem>>(jsonContent);
            if (jsonObj == null) return;
            RecommandModels = jsonObj.ToList();
        } 
        catch { }
    }

    public void LoadInstalledModels()
    {
        ChatModels = new();
        loadInstalledModels("llm", ModelType.chat, false, false, ChatModels);
        loadInstalledModels("llm_tools", ModelType.chat, true, false, ChatModels);
        loadInstalledModels("llm_vision", ModelType.chat, false, true, ChatModels);
        loadInstalledModels("llm_tools_vision", ModelType.chat, true, true, ChatModels);
        if (InstalledStateChanged != null) InstalledStateChanged.Invoke(this, EventArgs.Empty);
    }

    private void loadInstalledModels(string subPath, ModelType modelType, bool functionCall, bool vision, List<InstalledModelItem> typeGroup)
    {
        string modelPathRoot = CommandLineExecutor.GetAIModelsPath();
        if (!Directory.Exists(modelPathRoot)) Directory.CreateDirectory(modelPathRoot);

        string tgPath = Path.Combine(modelPathRoot, subPath);
        if (!Directory.Exists(tgPath)) Directory.CreateDirectory(tgPath);
        DirectoryInfo tgDir = new DirectoryInfo(tgPath);
        foreach(DirectoryInfo subDir in tgDir.GetDirectories())
        {
            string modelName = Path.Combine(subDir.FullName, "model.gguf");
            if (!File.Exists(modelName)) continue;
            if (vision && !File.Exists(Path.Combine(subDir.FullName, "mmproj.gguf"))) continue;
            InstalledModelItem newItem = new() { PathName = modelName, ModelName = subDir.Name, ModelType = modelType, FunctionCall = functionCall };
            if (vision)
            {
                newItem.Vision = true;
                newItem.VisionModel = Path.Combine(subDir.FullName, "mmproj.gguf");
            }
            typeGroup.Add(newItem);

            if (newItem.PathName.Contains("gemma-3-4b-it")) UsingLlmModel = newItem;
        }

    }

}
