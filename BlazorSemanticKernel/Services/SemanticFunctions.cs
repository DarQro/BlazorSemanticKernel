using BlazorSemanticKernel.Models;
using BlazorSemanticKernel.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BlazorSemanticKernel.Services;

public class SemanticFunctions
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;

    public SemanticFunctions(Kernel kernel, IChatCompletionService chatCompletionService)
    {
        _kernel = kernel;
        _chatCompletionService = chatCompletionService;

        //just showing that you can add plugins from the constructor or anywhere else
        _kernel.Plugins.AddFromType<ApplicantManagementPlugin>();
    }

    // This is the basic function calling demo endpoint
    public async Task<string> GetChatResponseAsync(ChatHistory chat)
    {
        var openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0.2,
            ModelId="llama3.1:8b",
            ChatSystemPrompt = @"Assistant is a large language model. 
                    This assistant uses plugins from the CustomPlugin class to interact with the software.
                    You can tell the time using the kernel function GetCurrentTime().
                    The assistant is very brief and succinct with words and does not talk much.
                    The assistant always includes the answer to the user's question in its response.
                    The assistant begins the conversation with a greeting and asks the user which function he or she would like to call based on the CustomPlugin functions you know about.
                    The assistant is brief and only replies with the least amount of words necessary.
                    If the prompt was simply a command, the assistant will ask for clarification ONLY if it is confused, otherwise the assisstant replies with 'Done.'"
        };


        ChatMessageContent result = await _chatCompletionService.GetChatMessageContentAsync(
            chatHistory: chat,
            executionSettings: openAIPromptExecutionSettings,
            kernel: _kernel);

        return result.Content ?? "Whoopsie, something went horribly wrong.";

    }

    // This is the endpoint for the document analysis demo
    public async Task<string> AnalyzeDocument(ChatHistory chat)
    {
        var openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0.2,
            ChatSystemPrompt = @"
                You are an AI assistant specialized in analyzing documents and routing them to appropriate departments.
                Analyze the document content carefully and determine the most appropriate department.
                Valid departments are: Accounting, Legal, Human Resources, IT Support, Sales, Customer Service, and Executive Office.
                
                Provide your response in this format:
                1. State the determined department name
                2. Provide a brief explanation of why you chose this department
                
                Be decisive and choose the single most appropriate department."
        };

        ChatMessageContent result = await _kernel.GetRequiredService<IChatCompletionService>().GetChatMessageContentAsync(
            chat,
            executionSettings: openAIPromptExecutionSettings,
            kernel: _kernel);

        return result.Content ?? "An error occurred while analyzing the document.";
    }

    public async Task<string> CheckOnApplicantsAsync(ChatHistory chat){
        var openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0.2,
            ChatSystemPrompt = @"You are an AI assistant specialized in managing job applicants.
                You have access to the ApplicantManagement plugin with the following capabilities:
                - Retrieving information about all applicants
                - Finding specific applicants by name
                - Getting applicants for specific positions
                
                Be concise in your responses and use the plugin functions when appropriate.
                When analyzing salary data or experience, provide insights about market competitiveness.
                Consider location and experience when discussing candidates.
                Provide information in a clear, organized way when displaying applicant data."
        };

        ChatMessageContent result = await _kernel.GetRequiredService<IChatCompletionService>()
            .GetChatMessageContentAsync(
                chat,
                executionSettings: openAIPromptExecutionSettings,
                kernel: _kernel);

        return result.Content ?? "An error occurred while processing your request.";
    }

    //This is the endpoint for the customer service chat bot demo
    public async Task<string> ProcessEnhancedCustomerServiceQueryAsync(ChatHistory chatHistory)
    {
        var openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0.3,
            ChatSystemPrompt = @"You are an AI customer service assistant for a life insurance company called Southern Farm Bureau Life Insurance (SFBLI). 
                Your task is to help customers with their queries, including address changes, policy information, claim status, and policy reviews. 
                Use the CustomerServicePlugin to fetch or update information when necessary. 
                Always confirm the customer's identity by asking for their policy number before providing or updating any information.
                Be proactive in suggesting relevant services or information based on the customer's situation.
                Always maintain a professional, empathetic tone, and ensure all interactions are compliant with financial services regulations.
                If you're unsure about any information, ask for clarification rather than making assumptions."
        };

        ChatMessageContent result = await _kernel.GetRequiredService<IChatCompletionService>().GetChatMessageContentAsync(
            chatHistory,
            executionSettings: openAIPromptExecutionSettings,
            kernel: _kernel);

        return result.Content ?? "I apologize, but I'm having trouble processing your request at the moment.";
    }
}

//             var executionSettings = new OllamaPromptExecutionSettings
//             {
//                 Temperature = 0.3f,
//                 TopP = 0.9f,
//                 TopK = 40,
//                 };
// #pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

//             chat.AddSystemMessage(@"Assistant is a large language model. 
//                     This assistant uses plugins from the CustomPlugin class to interact with the software.
//                     The assistant is very brief and succinct with words and does not talk much.
//                     The assistant responds to commands with 'Done. Anything else?'.
//                     The assistant begins the conversation with a greeting and asks the user which function he or she would like to call based on the CustomPlugin functions you know about.
//                     The assistant is brief and only replies with the least amount of words necessary.
//                     If the prompt was simply a command, the assistant will ask for clarification ONLY if it is confused, otherwise the assisstant replies with 'Done.'"
//             );
// #pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//             var settings = new OllamaPromptExecutionSettings
//             {
//                 TopK = 10,
//                 TopP = 8,
//                 Temperature = 0.8f,
//                 ModelId="llama3.1:latest"
//                 };
// #pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

//             chat.AddSystemMessage(@"Assistant is a large language model. 
//                      This assistant uses plugins from the CustomPlugin class to interact with the software.
//                      The assistant is very brief and succinct with words and does not talk much.
//                      The assistant responds to commands with 'Done. Anything else?'.
//                      The assistant begins the conversation with a greeting and asks the user which function he or she would like to call based on the CustomPlugin functions you know about.
//                      The assistant is brief and only replies with the least amount of words necessary.
//                      If the prompt was simply a command, the assistant will ask for clarification ONLY if it is confused, otherwise the assisstant replies with 'Done.'");