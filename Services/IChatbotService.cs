using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public interface IChatbotService
    {
        Task<ChatbotResponse> ProcessMessageAsync(string message, string sessionId, string? userId = null);
        Task<List<FAQ>> GetFAQsByCategoryAsync(string category);
        Task<List<ChatMessage>> GetChatHistoryAsync(string sessionId);
        Task SaveMessageAsync(ChatMessage message);
        Task<ChatbotResponse> GetWelcomeMessageAsync();
        Task<List<string>> GetSuggestedQuestionsAsync();
    }
}
