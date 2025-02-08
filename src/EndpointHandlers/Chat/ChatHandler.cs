using GenAIService.Infrastructure.Persistence.Repositories;
using GenAIService.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace GenAIService.EndpointHandlers;

public class ChatHandler : IEndpointRouteHandler
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/novo-chat", HandleCreateNovoChat)
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Cria um novo chat e retorna o id do chat para ser utilizado nas interações."
            });
        
        app.MapPost("/api/chats/{chatId}/messages", HandleChatRequest)
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Envia uma mensagem para o assistente IA e retorna a resposta.",
                Description = "Utiliza o Semantic Kernel para processar entradas e gerar respostas.",
            })
            .Produces<ChatResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleCreateNovoChat(
        ChatHistoryRedisRepository chatHistoryRedisRepository)
    {
        var chatId = Guid.CreateVersion7();
        var chatHistory = new ChatHistory();

        // Salva o novo ChatHistory com as últimas mensagens acrescentadas.
        await chatHistoryRedisRepository.SaveChatHistoryAsync(chatId, chatHistory, TimeSpan.FromMinutes(30));

        return Results.Created($"/api/chats/{chatId}", new { ChatId = chatId });
    }

    internal static async Task<IResult> HandleChatRequest(
        ChatHistoryRedisRepository chatHistoryRedisRepository,
        Kernel kernel,  
        ChatRequest request,
        Guid chatId)
    {
        // kernel.Plugins.AddFromType<TimePlugin>("CurrentTime");
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        kernel.Plugins.AddFromType<TimePlugin>("TempoAtual");

        // Enable planning
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new() 
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        // Obtém o histórico da conversa (ou cria um novo).
        ChatHistory chatHistory;
        try
        {
            chatHistory = await chatHistoryRedisRepository.GetChatHistoryAsync(chatId);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound($"Chat id {chatId} não encontrado.");
        }

        chatHistory.AddUserMessage(request.Message);

        // var chatFunction = kernel.CreateFunctionFromPrompt(@"Pergunta do usuário: {{$input}} .");
        // var context = new KernelArguments { ["input"] = request.Message };
        // var result = await kernel.InvokeAsync(chatFunction, context);

        var result = await chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings: openAIPromptExecutionSettings,
            kernel: kernel);

        chatHistory.AddAssistantMessage(result.ToString());

        // Salva o novo ChatHistory com as últimas mensagens acrescentadas.
        await chatHistoryRedisRepository.SaveChatHistoryAsync(chatId, chatHistory, TimeSpan.FromMinutes(30));

        return Results.Ok(new ChatResponse { Response = result.ToString() });
    }
}

public class ChatRequest
{
    public required string Message { get; set; }
}

public class ChatResponse
{
    public required string Response { get; set; }
}
