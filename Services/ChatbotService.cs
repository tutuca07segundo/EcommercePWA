using EcommercePWA.Data;
using EcommercePWA.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace EcommercePWA.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ChatbotConfig _config;

        public ChatbotService(ApplicationDbContext context, IProductService productService, IOrderService orderService)
        {
            _context = context;
            _productService = productService;
            _orderService = orderService;
            _config = new ChatbotConfig
        {
            WelcomeMessage = "Olá! 👋 Sou o assistente virtual da Kaboom. Como posso ajudar você hoje?",
            WelcomeOptions = new List<string>
            {
                "💳 Formas de pagamento",
                "📦 Rastrear pedido",
                "📞 Falar com atendente"
            }
        };
        }

        public async Task<ChatbotResponse> ProcessMessageAsync(string message, string sessionId, string? userId = null)
        {

            await SaveMessageAsync(new ChatMessage
            {
                SessionId = sessionId,
                Message = message,
                IsFromUser = true,
                UserId = userId
            });

            var response = await GenerateResponseAsync(message, userId);

            await SaveMessageAsync(new ChatMessage
            {
                SessionId = sessionId,
                Message = response.Message,
                IsFromUser = false
            });

            return response;
        }

        private async Task<ChatbotResponse> GenerateResponseAsync(string message, string? userId)
        {
            var lowerMessage = message.ToLower().Trim();

            if (IsGreeting(lowerMessage))
            {
                return new ChatbotResponse
                {
                    Message = _config.WelcomeMessage,
                    QuickReplies = _config.WelcomeOptions
                };
            }

            if (ContainsKeywords(lowerMessage, new[] { "rastrear", "pedido", "entrega", "onde está", "📦" }))
            {
                return await HandleOrderTrackingAsync(lowerMessage, userId);
            }

            if (ContainsKeywords(lowerMessage, new[] { "pagamento", "pix", "cartão", "boleto", "parcelamento", "💳" }))
            {
                return HandlePaymentInquiry();
            }

            if (ContainsKeywords(lowerMessage, new[] { "contato", "telefone", "whatsapp", "atendente", "humano", "suporte", "📞" }))
            {
                return HandleContactInquiry();
            }

            return new ChatbotResponse
            {
                Message = "Desculpe, não entendi sua pergunta. 😅\n\nPosso te ajudar com:",
                QuickReplies = new List<string>
                {
                    "💳 Formas de pagamento",
                    "📦 Rastrear pedido", 
                    "📞 Falar com atendente"
                },
                ShowWhatsAppButton = true,
                WhatsAppMessage = $"Olá! Vim do site Kaboom e preciso de ajuda com: {message}"
            };
        }

        private async Task<ChatbotResponse> HandleOrderTrackingAsync(string message, string? userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new ChatbotResponse
                {
                    Message = "Para rastrear seu pedido, você precisa estar logado. Gostaria que eu te ajude com o login?",
                    QuickReplies = new List<string> { "Como fazer login?", "Esqueci minha senha", "Falar com atendente" }
                };
            }

            var recentOrders = await _orderService.GetOrdersByUserId(int.Parse(userId));
            
            if (!recentOrders.Any())
            {
                return new ChatbotResponse
                {
                    Message = "Não encontrei pedidos em sua conta. Se você fez um pedido recentemente, pode levar alguns minutos para aparecer no sistema.",
                    QuickReplies = new List<string> { "Fazer um pedido", "Falar com atendente" }
                };
            }

            var lastOrder = recentOrders.First();
            return new ChatbotResponse
            {
                Message = $"Seu último pedido #{lastOrder.Id} está com status: {GetOrderStatusDescription(lastOrder.Status)}. " +
                         $"Valor: R$ {lastOrder.Total:F2}. Gostaria de mais detalhes?",
                QuickReplies = new List<string> { "Ver detalhes do pedido", "Outros pedidos", "Falar com atendente" }
            };
        }

        private ChatbotResponse HandlePaymentInquiry()
        {
            return new ChatbotResponse
            {
                Message = "Aceitamos as seguintes formas de pagamento:\n\n" +
                         "💳 **Cartão de Crédito**\n- Visa, Mastercard, Elo\n- Parcelamento em até 12x\n\n" +
                         "📱 **PIX**\n- Aprovação instantânea\n- Desconto de 5%\n\n" +
                         "🎫 **Boleto Bancário**\n- Vencimento em 3 dias úteis\n- Desconto de 3%\n\n" +
                         "Todas as transações são processadas pelo Mercado Pago com total segurança! 🔒",
                QuickReplies = new List<string> { "Como pagar com PIX?", "Parcelamento", "Segurança", "Fazer pedido" }
            };
        }

        private ChatbotResponse HandleContactInquiry()
        {
            return new ChatbotResponse
            {
                Message = "📞 **Entre em Contato Conosco:**\n\n" +
                         "💬 **WhatsApp**: Clique no botão abaixo para falar conosco\n" +
                         "📧 **Email**: contato@kaboom.com.br\n" +
                         "🕒 **Horário**: Segunda a Sexta, 8h às 18h\n" +
                         "📍 **Endereço**: São Paulo - SP\n\n" +
                         "Nossa equipe está pronta para te ajudar! 😊",
                QuickReplies = new List<string> { "Enviar email", "Horário de funcionamento", "Endereço completo" },
                ShowWhatsAppButton = true,
                WhatsAppMessage = "Olá! Vim do site Kaboom e gostaria de falar com um atendente."
            };
        }

        private bool IsGreeting(string message)
        {
            var greetings = new[] { "oi", "olá", "ola", "hello", "hi", "bom dia", "boa tarde", "boa noite" };
            return greetings.Any(g => message.Contains(g));
        }

        private bool ContainsKeywords(string message, string[] keywords)
        {
            return keywords.Any(keyword => message.Contains(keyword));
        }

        private string GetOrderStatusDescription(string status)
        {
            return status.ToLower() switch
            {
                "pending" => "Aguardando Pagamento",
                "paid" => "Pagamento Confirmado",
                "processing" => "Em Processamento",
                "shipped" => "Enviado",
                "delivered" => "Entregue",
                "cancelled" => "Cancelado",
                _ => "Status Desconhecido"
            };
        }

        public async Task<ChatbotResponse> GetWelcomeMessageAsync()
        {
            return new ChatbotResponse
            {
                Message = _config.WelcomeMessage,
                QuickReplies = _config.WelcomeOptions
            };
        }

        public async Task<List<string>> GetSuggestedQuestionsAsync()
        {
            return new List<string>
            {
                "💳 Quais formas de pagamento vocês aceitam?",
                "📦 Como rastrear meu pedido?",
                "📞 Como falar com um atendente?"
            };
        }

        public async Task<List<FAQ>> GetFAQsByCategoryAsync(string category)
        {

            return new List<FAQ>();
        }

        public async Task<List<ChatMessage>> GetChatHistoryAsync(string sessionId)
        {

            return new List<ChatMessage>();
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {

            await Task.CompletedTask;
        }
    }
}
