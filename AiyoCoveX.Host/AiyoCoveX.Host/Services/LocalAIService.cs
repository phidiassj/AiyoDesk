using AiyoLibraryV2.AiService.Extensions;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using System.ClientModel;
using System.Text.Json;

namespace AiyoCoveX.Host.Services;

public class LocalAIService : ILocalAIService
{
    public string AIRootEndpoint { get; set; } = @"http://localhost:16888/v1";
    public string ContentEndpoint { get; set; } = @"http://localhost:16888/";
    public string ContentLocalPath { get; set; } = @"D:\AI\llama-cpp-ipex-llm-2.2.0b20250313-win\public";

    public async Task<ChatCompletion> ChatAsync(string ModelId, List<ChatMessage> messages)
    {
        OpenAIClientOptions openAIClientOptions = new();
        openAIClientOptions.Endpoint = new Uri(AIRootEndpoint);
        ApiKeyCredential apiKey = new("123");
        OpenAIClient openAIClient = new(apiKey, openAIClientOptions);
        
        ChatCompletionOptions chatCompletionOptions = new();
        chatCompletionOptions.Temperature = 0.2f;
        chatCompletionOptions.Tools.Add(ToolsManager.getCurrentTimeTool);

        ChatClient chatClient = openAIClient.GetChatClient(ModelId);
        ClientResult<ChatCompletion> result = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);

        bool needRevoke = false;
        if (result.Value.FinishReason == ChatFinishReason.ToolCalls && result.Value.ToolCalls.Any())
        {
            needRevoke = true;
            messages.Add(new AssistantChatMessage(result.Value));
            foreach (ChatToolCall toolCall in result.Value.ToolCalls)
            {
                switch (toolCall.FunctionName)
                {
                    case nameof(ToolsManager.getCurrentTime):
                        {
                            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                            bool hasLocation = argumentsJson.RootElement.TryGetProperty("location", out JsonElement location);
                            messages.Add(new ToolChatMessage(toolCall.Id, ToolsManager.getCurrentTime()));
                            break;
                        }
                    default:
                        {
                            // Handle other unexpected calls.
                            throw new NotImplementedException();
                        }
                }
            }
        }

        if (needRevoke)
        {
            result = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);
        }

        return result.Value;
    }

    public async Task<OpenAIModelCollection?> GetModels()
    {
        OpenAIClientOptions openAIClientOptions = new();
        openAIClientOptions.Endpoint = new Uri(AIRootEndpoint);
        ApiKeyCredential apiKey = new("123");
        OpenAIClient openAIClient = new(apiKey, openAIClientOptions);

        await Task.Delay(1);

        OpenAIModelClient modelClient = openAIClient.GetOpenAIModelClient();
        OpenAIModelCollection models = modelClient.GetModels();
        return models;

        //var modelRet = await Task.Run<OpenAIModelCollection?>(() =>
        //{
        //    try
        //    {
        //        OpenAIModelClient modelClient = openAIClient.GetOpenAIModelClient();
        //        OpenAIModelCollection models = modelClient.GetModels();
        //        return models;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //});

        //return modelRet;
    }


}

public interface ILocalAIService
{
    string AIRootEndpoint { get; set; }
    string ContentLocalPath { get; set; }
    string ContentEndpoint { get; set; }

    Task<ChatCompletion> ChatAsync(string ModelId, List<ChatMessage> messages);
    Task<OpenAIModelCollection?> GetModels();
}

public class ToolsManager
{
    public static string getCurrentTime()
    {
        return StaticFunctions.GetGmt8DateTime().ToString("yyyy年MM月dd日HH時mm分ss秒");
    }

    public static readonly ChatTool getCurrentTimeTool = ChatTool.CreateFunctionTool(
    functionName: nameof(getCurrentTime),
    functionDescription: "Get current time",
    functionParameters: BinaryData.FromBytes("""
        {
            "type": "object",
            "properties": {
                "location": {
                    "type": "string",
                    "description": "The city and state, e.g. Boston, MA"
                }
            },
            "required": [ "location" ]
        }
        """u8.ToArray())
    );
}