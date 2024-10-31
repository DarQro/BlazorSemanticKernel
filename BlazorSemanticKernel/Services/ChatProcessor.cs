using System.Linq;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BlazorSemanticKernel.Services;

public static class ChatProcessor
{
    public static string ProcessChatHistory(ChatHistory chatHistory)
    {
        // Get only the last assistant message that isn't a function call or result
        var lastAssistantMessage = chatHistory
            .Where(m => m.Role == AuthorRole.Assistant && !IsFunctionRelatedMessage(m.Content))
            .LastOrDefault();

        return lastAssistantMessage?.Content ?? "I apologize, but I couldn't generate a proper response.";
    }

    private static bool IsFunctionRelatedMessage(string content)
    {
        // Check if the message looks like a function call or result
        return content.Contains("function call") || content.Contains("function result") || 
               content.StartsWith("{") || content.StartsWith("[");
    }
}