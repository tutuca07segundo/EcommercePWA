using EcommercePWA.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommercePWA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("message")]
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

        [HttpGet("welcome")]
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

        [HttpGet("suggestions")]
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

        [HttpGet("history/{sessionId}")]
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

    public class ChatMessageRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? SessionId { get; set; }
    }
}
