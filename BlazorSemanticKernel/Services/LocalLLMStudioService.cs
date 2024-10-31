using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextGeneration;

namespace BlazorSemanticKernel.Services;


public class ChatCompletionResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("object")]
            public string Object { get; set; }

            [JsonPropertyName("created")]
            public long Created { get; set; }

            [JsonPropertyName("model")]
            public string Model { get; set; }

            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; }

            [JsonPropertyName("usage")]
            public Usage TokenUsage { get; set; }

            public class Choice
            {
                [JsonPropertyName("index")]
                public int Index { get; set; }

                [JsonPropertyName("message")]
                public Message Message { get; set; }

                [JsonPropertyName("finish_reason")]
                public string? FinishReason { get; set; }
            }

            public class Message
            {
                [JsonPropertyName("role")]
                public string Role { get; set; }

                [JsonPropertyName("content")]
                public string Content { get; set; }
            }

            public class Usage
            {
                [JsonPropertyName("prompt_tokens")]
                public int PromptTokens { get; set; }

                [JsonPropertyName("completion_tokens")]
                public int CompletionTokens { get; set; }

                [JsonPropertyName("total_tokens")]
                public int TotalTokens { get; set; }
            }
        }
        



public class LocalLMStudioService : ITextGenerationService, IChatCompletionService
{
    public string ModelUrl { get; init; } = "http://localhost:1234/v1/chat/completions";
    public string ModelId { get; init; } = "NousResearch/Nous-Hermes-2-Mistral-7B-DPO-GGUF";
    private readonly HttpClient _httpClient;
    private readonly string _modelName;

    public LocalLMStudioService(HttpClient httpClient, string modelName)
    {
        _httpClient = httpClient;
        _modelName = modelName;
    }

    // just here to satisfy the interface
    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {

        var openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings 
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        executionSettings = openAIPromptExecutionSettings;

        var messages = chatHistory.Select(m => new { role = m.Role.ToString().ToLower(), content = m.Content }).ToList();
        var requestBody = JsonSerializer.Serialize(new
        {
            model = _modelName,
            messages,
            temperature = 0.7
        });

        var response = await _httpClient.PostAsync("v1/chat/completions",
            new StringContent(requestBody, Encoding.UTF8, "application/json"), cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseBody, options);
        Console.WriteLine($"Deserialized result: Choices count = {result?.Choices[0].Message.Content}, First choice content = {result?.Choices?.FirstOrDefault()?.Message?.Content}");

        if (result?.Choices == null || result.Choices.Count == 0)
        {
            // Handle the case where there are no choices in the response
            return new List<ChatMessageContent>();
        }

        return new List<ChatMessageContent>
        {
            new ChatMessageContent(
                AuthorRole.Assistant,
                result.Choices[0].Message.Content,
                metadata: new Dictionary<string, object?>
                {
                    { "FinishReason", result.Choices[0].FinishReason ?? "unknown" }
                })
        };
    }

    
    // just here to satisfy the interface again
    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);
        foreach (var message in response)
        {
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, message.Content ?? string.Empty);
        }
    }



public async Task<IReadOnlyList<TextContent>> GetTextContentsAsync(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<object> { new { role = "user", content = prompt } };
        var requestBody = JsonSerializer.Serialize(new
        {
            model = _modelName,
            messages,
            temperature =  0.7
        });

        var response = await _httpClient.PostAsync(ModelUrl,
            new StringContent(requestBody, Encoding.UTF8, "application/json"), cancellationToken);

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseBody, options);

        if (result?.Choices == null || result.Choices.Count == 0)
        {
            return new List<TextContent>();
        }

        return new List<TextContent>
        {
            new TextContent(result.Choices[0].Message.Content)
        };
    }

    public async IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = new List<object> { new { role = "user", content = prompt } };
        var requestBody = JsonSerializer.Serialize(new
        {
            model = _modelName,
            messages,
            temperature =  0.7,
            stream = true
        });

        var request = new HttpRequestMessage(HttpMethod.Post, ModelUrl)
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("data: "))
            {
                var json = line.Substring(6);
                if (json == "[DONE]")
                    break;

                var streamResponse = JsonSerializer.Deserialize<StreamResponse>(json);
                if (streamResponse?.Choices != null && streamResponse.Choices.Count > 0)
                {
                    yield return new StreamingTextContent(streamResponse.Choices[0].Delta.Content ?? string.Empty);
                }
            }
        }
    }






    private class StreamResponse
    {
        public List<StreamChoice> Choices { get; set; } = new List<StreamChoice>();
    }

    private class StreamChoice
    {
        public StreamDelta Delta { get; set; } = new StreamDelta();
    }

    private class StreamDelta
    {
        public string? Content { get; set; }
    }

    private class ChatCompletionResponse
    {
        public List<Choice> Choices { get; set; } = new List<Choice>();

        public class Choice
        {
            public Message Message { get; set; } = new Message();
            public string? FinishReason { get; set; }
        }

        public class Message
        {
            public string Content { get; set; } = string.Empty;
        }
    }
}
