using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AiyoCoveX.Host.Services;

/// <summary>
/// ComfyUI API Client
/// 可用來調用 ComfyUI 各項 API：下發工作流、查詢隊列、取得歷史結果、組合圖片預覽 URL，以及連接 WebSocket 監聽進度通知。
/// </summary>
public class ComfyUIClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    /// <summary>
    /// 建構子，需傳入 ComfyUI 的基本網址 (例如：http://127.0.0.1:8188)
    /// </summary>
    /// <param name="baseUrl">ComfyUI 服務 URL</param>
    public ComfyUIClient(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// 下發繪圖工作流 (POST /prompt)
    /// </summary>
    /// <param name="clientId">客戶端識別字串，由使用者自訂</param>
    /// <param name="prompt">包含工作流參數的物件（通常由前端產生的 JSON）</param>
    /// <returns>回傳 PromptResponse 物件，包含 prompt_id、任務編號與錯誤資訊</returns>
    public async Task<PromptResponse?> SubmitPromptAsync(string clientId, object prompt)
    {
        var payload = new
        {
            client_id = clientId,
            prompt = prompt
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/prompt", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PromptResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result;
    }

    /// <summary>
    /// 查詢工作隊列 (GET /queue)
    /// </summary>
    /// <returns>回傳原始 JSON 字串，使用者可自行解析</returns>
    public async Task<string> GetQueueAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/queue");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// 取得指定 prompt_id 的歷史結果 (GET /history/{prompt_id})
    /// </summary>
    /// <param name="promptId">工作流的 prompt_id</param>
    /// <returns>回傳原始 JSON 字串，可解析最終輸出圖片資訊</returns>
    public async Task<string> GetHistoryAsync(string promptId)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/history/{promptId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// 組合圖片預覽 URL (GET /view)
    /// </summary>
    /// <param name="filename">圖片檔名</param>
    /// <param name="type">圖片類型，預設 "output"，也可為 "input" 等</param>
    /// <param name="subfolder">子目錄 (若有)</param>
    /// <param name="preview">是否為預覽 (可選)</param>
    /// <param name="channel">頻道參數 (可選)</param>
    /// <returns>組合後可直接存取圖片的 URL</returns>
    public string GetViewUrl(string filename, string type = "output", string? subfolder = null, string? preview = null, string? channel = null)
    {
        var queryParams = new List<string> { $"filename={Uri.EscapeDataString(filename)}", $"type={Uri.EscapeDataString(type)}" };

        if (!string.IsNullOrEmpty(subfolder))
            queryParams.Add($"subfolder={Uri.EscapeDataString(subfolder)}");
        if (!string.IsNullOrEmpty(preview))
            queryParams.Add($"preview={Uri.EscapeDataString(preview)}");
        if (!string.IsNullOrEmpty(channel))
            queryParams.Add($"channel={Uri.EscapeDataString(channel)}");

        var query = string.Join("&", queryParams);
        return $"{_baseUrl}/view?{query}";
    }

    /// <summary>
    /// 建立 WebSocket 連線以監聽 ComfyUI 的進度與狀態推播 (/ws?clientId=...)
    /// </summary>
    /// <param name="clientId">與 /prompt 呼叫時所使用的相同 client_id</param>
    /// <param name="cancellationToken">取消代碼</param>
    /// <returns>回傳已連線的 ClientWebSocket 物件</returns>
    public async Task<ClientWebSocket> ConnectWebSocketAsync(string clientId, CancellationToken cancellationToken = default)
    {
        // 將 HTTP(s) URL 轉為 WebSocket URL (http:// => ws://，https:// => wss://)
        var wsScheme = _baseUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws";
        // 假設 WebSocket 端點為 /ws?clientId=xxx
        var wsUrl = $"{wsScheme}://{new Uri(_baseUrl).Authority}/ws?clientId={Uri.EscapeDataString(clientId)}";

        var clientWebSocket = new ClientWebSocket();
        await clientWebSocket.ConnectAsync(new Uri(wsUrl), cancellationToken);
        return clientWebSocket;
    }

    /// <summary>
    /// 釋放 HttpClient 資源
    /// </summary>
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// 下發繪圖工作流回應結果
/// </summary>
public class PromptResponse
{
    public string Prompt_Id { get; set; } = null!; // 對應 JSON 中的 "prompt_id"
    public int Number { get; set; }
    public Dictionary<string, object>? Node_Errors { get; set; }
}