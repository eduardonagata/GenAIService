using Microsoft.SemanticKernel.ChatCompletion;
using StackExchange.Redis.Extensions.Core.Abstractions;
// Certifique-se de referenciar o namespace correto onde está definida a classe ChatHistory do Semantic Kernel.
// Exemplo:
// using Microsoft.SemanticKernel.Orchestration; 

namespace GenAIService.Infrastructure.Persistence.Repositories
{
    public class ChatHistoryRedisRepository
    {
        private readonly IRedisDatabase _redisDatabase;

        public ChatHistoryRedisRepository(IRedisDatabase redisDatabase)
        {
            _redisDatabase = redisDatabase;
        }

        /// <summary>
        /// Persiste o objeto ChatHistory no Redis com expiração.
        /// </summary>
        /// <param name="chatId">Identificador único do chat.</param>
        /// <param name="chatHistory">Objeto ChatHistory do Semantic Kernel a ser persistido.</param>
        /// <param name="expiresIn">Tempo de expiração para o item armazenado.</param>
        public async Task SaveChatHistoryAsync(Guid chatId, ChatHistory chatHistory, TimeSpan expiresIn)
        {
            await _redisDatabase.AddAsync(chatId.ToString(), chatHistory, expiresIn);
        }

        /// <summary>
        /// Recupera o objeto ChatHistory armazenado no Redis.
        /// </summary>
        /// <param name="chatId">Identificador único do chat.</param>
        /// <returns>Objeto ChatHistory ou null, se não existir.</returns>
        public async Task<ChatHistory> GetChatHistoryAsync(Guid chatId)
        {
            var chatHistory = await _redisDatabase.GetAsync<ChatHistory>(chatId.ToString());
            if (chatHistory is null)
            {
                throw new KeyNotFoundException($"Chat id {chatId} não encontrado.");
            }
            return chatHistory;
        }
    }
}
