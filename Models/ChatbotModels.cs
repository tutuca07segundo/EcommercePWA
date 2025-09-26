using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsFromUser { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string? UserId { get; set; }
    }

    public class ChatbotResponse
    {
        public string Message { get; set; } = string.Empty;
        public List<string> QuickReplies { get; set; } = new();
        public bool ShowWhatsAppButton { get; set; } = false;
        public string? WhatsAppMessage { get; set; }
        public bool ShowHumanSupport { get; set; } = false;
    }

    public class FAQ
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class ChatbotConfig
    {
        public string WelcomeMessage { get; set; } = "Olá! 👋 Sou o assistente virtual da Kaboom. Como posso ajudar você hoje?";
        public string WhatsAppNumber { get; set; } = "5511999999999";
        public string FallbackMessage { get; set; } = "Desculpe, não entendi sua pergunta. Gostaria de falar com um atendente humano?";
        public List<string> WelcomeOptions { get; set; } = new()
        {
            "📦 Rastrear pedido",
            "🛍️ Informações sobre produtos",
            "💳 Formas de pagamento",
            "🚚 Prazos de entrega",
            "🔄 Trocas e devoluções",
            "📞 Falar com atendente"
        };
    }
}
