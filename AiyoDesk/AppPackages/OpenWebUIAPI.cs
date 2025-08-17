using AiyoDesk.Data;
using AiyoDesk.LocalHost;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AiyoDesk.AppPackages;

/// <summary>
/// 封裝對 Open-WebUI API 的操作
/// </summary>
public class OpenWebUiClient : IDisposable
{
    private readonly HttpClient _http;

    public OpenWebUiClient(string baseUrl, HttpMessageHandler? handler = null)
    {
        _http = handler is null
            ? new HttpClient()
            : new HttpClient(handler, disposeHandler: true);
        _http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    }

    public void AddTokenHeader()
    {
        SystemSetting systemSetting = ServiceCenter.databaseManager.GetSystemSetting();
        if (systemSetting.OpenWebUIToken != null)
        {
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", systemSetting.OpenWebUIToken);
        }
    }
    public void AddTokenHeader(string spToken)
    {
        _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", spToken);
    }

    /// <summary>
    /// 取得管理員詳細資訊（GET /api/v1/auths/admin/details）
    /// </summary>
    public async Task<AdminDetails> GetAdminDetailsAsync()
    {
        var resp = await _http.GetAsync("api/v1/auths/admin/details");
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<AdminDetails>()
               ?? throw new InvalidOperationException("無法解析 AdminDetails");
    }

    /// <summary>
    /// 新增登入身分（POST /api/v1/auths/add）
    /// </summary>
    public async Task<AuthSignUpResult> AddAuthAsync(AddAuthBody req)
    {
        var resp = await _http.PostAsJsonAsync("/api/v1/auths/signup", req);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<AuthSignUpResult>()
               ?? throw new InvalidOperationException("無法解析 AuthResponse");
    }

    public async Task<AuthSignUpResult> SignInAsync(SignInRequest req)
    {
        var resp = await _http.PostAsJsonAsync("/api/v1/auths/signin", req);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<AuthSignUpResult>()
               ?? throw new InvalidOperationException("無法解析 AuthResponse");
    }

    /// <summary>
    /// 取得 OpenAI 全局設定（GET /openai/config）
    /// </summary>
    public async Task<OpenAiGlobalConfig> GetOpenAiConfigAsync()
    {
        var resp = await _http.GetAsync("openai/config");
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<OpenAiGlobalConfig>()
               ?? throw new InvalidOperationException("無法解析 OpenAiGlobalConfig");
    }

    /// <summary>
    /// 更新 OpenAI 全局設定（POST /openai/config/update）
    /// </summary>
    public async Task<OpenAiGlobalConfig> UpdateOpenAiConfigAsync(OpenAiGlobalConfig cfg)
    {
        var resp = await _http.PostAsJsonAsync("openai/config/update", cfg);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<OpenAiGlobalConfig>()
               ?? throw new InvalidOperationException("無法解析 OpenAiGlobalConfig");
    }

    public async Task<GenerativeSetting> GetImagesConfigAsync()
    {
        var resp = await _http.GetAsync("/api/v1/images/config");
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<GenerativeSetting>()
               ?? throw new InvalidOperationException("無法解析 GenerativeSetting");
    }

    public async Task<GenerativeSetting> UpdateImagesConfigAsync(GenerativeSetting cfg)
    {
        //var resp = await _http.PostAsJsonAsync("/api/v1/images/config/update", cfg);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };
        var json = System.Text.Json.JsonSerializer.Serialize(cfg, options);
        var resp = await _http.PostAsync("/api/v1/images/config/update", new StringContent(json, Encoding.UTF8, "application/json"));
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<GenerativeSetting>()
               ?? throw new InvalidOperationException("無法解析 GenerativeSetting");
    }

    public async Task<ImageSetting> GetImageOutputConfigAsync()
    {
        var resp = await _http.GetAsync("/api/v1/images/image/config");
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ImageSetting>()
               ?? throw new InvalidOperationException("無法解析 ImageSetting");
    }

    public async Task<ImageSetting> UpdateImageOutputConfigAsync(ImageSetting cfg)
    {
        //var resp = await _http.PostAsJsonAsync("/api/v1/images/image/config/update", cfg);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };
        var json = System.Text.Json.JsonSerializer.Serialize(cfg, options);
        var resp = await _http.PostAsync("/api/v1/images/image/config/update", new StringContent(json, Encoding.UTF8, "application/json"));
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ImageSetting>()
               ?? throw new InvalidOperationException("無法解析 ImageSetting");
    }

    public static string NitroSD_Workflow_Template = "{\n  \"1\": {\n    \"inputs\": {\n      \"ckpt_name\": \"nitrosd-vibrant_comfyui.safetensors\"\n    },\n    \"class_type\": \"CheckpointLoaderSimple\",\n    \"_meta\": {\n      \"title\": \"Load Checkpoint\"\n    }\n  },\n  \"2\": {\n    \"inputs\": {\n      \"shifted_timestep\": 500,\n      \"model\": [\n        \"1\",\n        0\n      ]\n    },\n    \"class_type\": \"Timestep Shift Model\",\n    \"_meta\": {\n      \"title\": \"Timestep Shift Model\"\n    }\n  },\n  \"4\": {\n    \"inputs\": {\n      \"text\": \"A photograph of a tiger in winter wonderland. 8k resolution.\",\n      \"clip\": [\n        \"1\",\n        1\n      ]\n    },\n    \"class_type\": \"CLIPTextEncode\",\n    \"_meta\": {\n      \"title\": \"CLIP Text Encode (Prompt)\"\n    }\n  },\n  \"5\": {\n    \"inputs\": {\n      \"text\": \"low resolution.\",\n      \"clip\": [\n        \"1\",\n        1\n      ]\n    },\n    \"class_type\": \"CLIPTextEncode\",\n    \"_meta\": {\n      \"title\": \"CLIP Text Encode (Prompt)\"\n    }\n  },\n  \"6\": {\n    \"inputs\": {\n      \"width\": 512,\n      \"height\": 512,\n      \"batch_size\": 1\n    },\n    \"class_type\": \"EmptyLatentImage\",\n    \"_meta\": {\n      \"title\": \"Empty Latent Image\"\n    }\n  },\n  \"7\": {\n    \"inputs\": {\n      \"seed\": 918931249181095,\n      \"steps\": 2,\n      \"cfg\": 1,\n      \"sampler_name\": \"lcm\",\n      \"scheduler\": \"simple\",\n      \"denoise\": 1,\n      \"model\": [\n        \"13\",\n        0\n      ],\n      \"positive\": [\n        \"11\",\n        0\n      ],\n      \"negative\": [\n        \"12\",\n        0\n      ],\n      \"latent_image\": [\n        \"6\",\n        0\n      ]\n    },\n    \"class_type\": \"KSampler\",\n    \"_meta\": {\n      \"title\": \"KSampler\"\n    }\n  },\n  \"8\": {\n    \"inputs\": {\n      \"samples\": [\n        \"7\",\n        0\n      ],\n      \"vae\": [\n        \"1\",\n        2\n      ]\n    },\n    \"class_type\": \"VAEDecode\",\n    \"_meta\": {\n      \"title\": \"VAE Decode\"\n    }\n  },\n  \"9\": {\n    \"inputs\": {\n      \"filename_prefix\": \"ComfyUI\",\n      \"images\": [\n        \"8\",\n        0\n      ]\n    },\n    \"class_type\": \"SaveImage\",\n    \"_meta\": {\n      \"title\": \"Save Image\"\n    }\n  },\n  \"11\": {\n    \"inputs\": {\n      \"value\": [\n        \"4\",\n        0\n      ],\n      \"model\": [\n        \"2\",\n        0\n      ]\n    },\n    \"class_type\": \"UnloadModel\",\n    \"_meta\": {\n      \"title\": \"UnloadModel\"\n    }\n  },\n  \"12\": {\n    \"inputs\": {\n      \"value\": [\n        \"5\",\n        0\n      ],\n      \"model\": [\n        \"2\",\n        0\n      ]\n    },\n    \"class_type\": \"UnloadModel\",\n    \"_meta\": {\n      \"title\": \"UnloadModel\"\n    }\n  },\n  \"13\": {\n    \"inputs\": {\n      \"value\": [\n        \"2\",\n        0\n      ]\n    },\n    \"class_type\": \"UnloadAllModels\",\n    \"_meta\": {\n      \"title\": \"UnloadAllModels\"\n    }\n  }\n}";

    public void Dispose() => _http.Dispose();
}

#region DTO 定義

public class AdminDetails
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("roles")]
    public string[] Roles { get; set; } = Array.Empty<string>();
}

public class AddAuthBody
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("profile_image_url")]
    public string ProfileImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}

public class SignInRequest
{
    public string email { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
}

public class SignInResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("profile_image_url")]
    public string ProfileImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    public long? expires_at { get; set; }

}

public class OpenAiGlobalConfig
{
    [JsonPropertyName("ENABLE_OPENAI_API")]
    public bool EnableOpenAiApi { get; set; }

    [JsonPropertyName("OPENAI_API_BASE_URLS")]
    public List<string> BaseUrls { get; set; } = new();

    [JsonPropertyName("OPENAI_API_KEYS")]
    public List<string> ApiKeys { get; set; } = new();

    [JsonPropertyName("OPENAI_API_CONFIGS")]
    public Dictionary<string, OpenAiConnectionConfig> ConnectionConfigs { get; set; } = new();
}

public class OpenAiConnectionConfig
{
    [JsonPropertyName("enable")]
    public bool? Enable { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("prefix_id")]
    public string? PrefixId { get; set; }

    [JsonPropertyName("model_ids")]
    public List<string>? ModelIds { get; set; }

    [JsonPropertyName("connection_type")]
    public string? ConnectionType { get; set; }
}




public class AuthSignUpResult
{
    public string id { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string role { get; set; } = string.Empty;
    public string profile_image_url { get; set; } = string.Empty;
    public string token { get; set; } = string.Empty;
    public string token_type { get; set; } = string.Empty;
    public string? expires_at { get; set; }
    public Permissions permissions { get; set; } = new();
}

public class Permissions
{
    public Workspace workspace { get; set; } = new();
    public Sharing sharing { get; set; } = new();
    public Chat chat { get; set; } = new();
    public Features features { get; set; } = new();
}

public class Workspace
{
    public bool models { get; set; }
    public bool knowledge { get; set; }
    public bool prompts { get; set; }
    public bool tools { get; set; }
}

public class Sharing
{
    public bool public_models { get; set; }
    public bool public_knowledge { get; set; }
    public bool public_prompts { get; set; }
    public bool public_tools { get; set; }
}

public class Chat
{
    public bool controls { get; set; }
    public bool system_prompt { get; set; }
    public bool file_upload { get; set; }
    public bool delete { get; set; }
    public bool edit { get; set; }
    public bool share { get; set; }
    public bool export { get; set; }
    public bool stt { get; set; }
    public bool tts { get; set; }
    public bool call { get; set; }
    public bool multiple_models { get; set; }
    public bool temporary { get; set; }
    public bool temporary_enforced { get; set; }
}

public class Features
{
    public bool direct_tool_servers { get; set; }
    public bool web_search { get; set; }
    public bool image_generation { get; set; }
    public bool code_interpreter { get; set; }
    public bool notes { get; set; }
}




public class GenerativeSetting
{
    public bool enabled { get; set; }
    public string engine { get; set; } = string.Empty;
    public bool prompt_generation { get; set; }
    public Openai openai { get; set; } = new();
    public Automatic1111 automatic1111 { get; set; } = new();
    public Comfyui comfyui { get; set; } = new();
    public Gemini gemini { get; set; } = new();
}

public class Openai
{
    public string OPENAI_API_BASE_URL { get; set; } = string.Empty;
    public string OPENAI_API_KEY { get; set; } = string.Empty;
}

public class Automatic1111
{
    public string AUTOMATIC1111_BASE_URL { get; set; } = string.Empty;
    public string AUTOMATIC1111_API_AUTH { get; set; } = string.Empty;
    public object? AUTOMATIC1111_CFG_SCALE { get; set; } = null;
    public object? AUTOMATIC1111_SAMPLER { get; set; } = null;
    public object? AUTOMATIC1111_SCHEDULER { get; set; } = null;
}

public class Comfyui
{
    public string COMFYUI_BASE_URL { get; set; } = string.Empty;
    public string COMFYUI_API_KEY { get; set; } = string.Empty;
    public string COMFYUI_WORKFLOW { get; set; } = string.Empty;
    public List<COMFYUI_WORKFLOW_NODE> COMFYUI_WORKFLOW_NODES { get; set; } = new();
}

public class COMFYUI_WORKFLOW_NODE
{
    public string type { get; set; } = string.Empty;
    public string key { get; set; } = string.Empty;
    public List<string> node_ids { get; set; } = new();
}

public class Gemini
{
    public string GEMINI_API_BASE_URL { get; set; } = string.Empty;
    public string GEMINI_API_KEY { get; set; } = string.Empty;
}



public class ImageSetting
{
    public string MODEL { get; set; } = string.Empty;
    public string IMAGE_SIZE { get; set; } = string.Empty;
    public int IMAGE_STEPS { get; set; }
}


#endregion