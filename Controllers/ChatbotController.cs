using EcommercePWA.Services;
using EcommercePWA.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcommercePWA.Controllers
{
    /// <summary>
    /// API para chatbot inteligente do e-commerce Kaboom
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        /// <summary>
        /// Envia uma mensagem para o chatbot e recebe resposta inteligente
        /// </summary>
        /// <param name="request">Mensagem do usuário</param>
        /// <returns>Resposta do chatbot com sugestões e ações</returns>
        [HttpPost("message")]
        [ProducesResponseType(typeof(ChatbotResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request)
        {
            try
            {
                var sessionId = request.SessionId ?? HttpContext.Session.Id;
                var userId = HttpContext.Session.GetInt32("UserId")?.ToString();

                var response = await _chatbotService.ProcessMessageAsync(request.Message, sessionId, userId);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Erro ao processar mensagem", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém mensagem de boas-vindas do chatbot
        /// </summary>
        /// <returns>Mensagem de boas-vindas personalizada</returns>
        [HttpGet("welcome")]
        [ProducesResponseType(typeof(ChatbotResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetWelcomeMessage()
        {
            try
            {
                var response = await _chatbotService.GetWelcomeMessageAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Erro ao obter mensagem de boas-vindas", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém sugestões de perguntas frequentes
        /// </summary>
        /// <returns>Lista de perguntas sugeridas</returns>
        [HttpGet("suggestions")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetSuggestions()
        {
            try
            {
                var suggestions = await _chatbotService.GetSuggestedQuestionsAsync();
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Erro ao obter sugestões", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém histórico de conversa por sessão
        /// </summary>
        /// <param name="sessionId">ID da sessão do chat</param>
        /// <returns>Histórico completo da conversa</returns>
        [HttpGet("history/{sessionId}")]
        [ProducesResponseType(typeof(IEnumerable<ChatMessage>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetChatHistory(string sessionId)
        {
            try
            {
                var history = await _chatbotService.GetChatHistoryAsync(sessionId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Erro ao obter histórico", details = ex.Message });
            }
        }
    }

    /// <summary>
    /// Modelo para requisição de mensagem do chatbot
    /// </summary>
    public class ChatMessageRequest
    {
        /// <summary>Mensagem do usuário</summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>ID da sessão (opcional)</summary>
        public string? SessionId { get; set; }
    }
}
