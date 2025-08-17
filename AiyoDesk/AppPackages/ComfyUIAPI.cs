using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AiyoDesk.AppPackages;

public class ComfyUIAPI
{
    /// <summary>
    /// 在扁平化的 comfy.settings.json 中，設定或更新 ComfyUI 介面語言。
    /// </summary>
    /// <param name="comfyUiRoot">ComfyUI 根目錄。</param>
    /// <param name="locale">語系代碼，例如 "zh"、"en"。</param>
    public static void SetLocale(string settingsPath, string locale)
    {
        JsonObject root;

        // 讀取既有檔案或建立新物件
        if (File.Exists(settingsPath))
        {
            var text = File.ReadAllText(settingsPath);
            root = JsonNode.Parse(text)?.AsObject() ?? new JsonObject();
        }
        else
        {
            root = new JsonObject();
        }

        // 直接以扁平化 key 指定語系
        root["Comfy.Locale"] = locale;

        // 保留原本其他設定
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(settingsPath, root.ToJsonString(options));

    }
}

