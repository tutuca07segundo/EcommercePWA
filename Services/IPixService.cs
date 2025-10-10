using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    /// <summary>
    /// Interface para serviços de PIX
    /// </summary>
    public interface IPixService
    {
        /// <summary>
        /// Gera um PIX com QR Code para pagamento
        /// </summary>
        /// <param name="valor">Valor do PIX em reais</param>
        /// <param name="descricao">Descrição do pagamento</param>
        /// <param name="identificador">Identificador único da transação</param>
        /// <returns>Dados do PIX gerado</returns>
        Task<PixPaymentData> GeneratePixAsync(decimal valor, string descricao, string identificador);

        /// <summary>
        /// Gera QR Code como imagem base64
        /// </summary>
        /// <param name="pixCode">Código PIX (EMV)</param>
        /// <returns>Imagem QR Code em base64</returns>
        string GenerateQRCodeImage(string pixCode);

        /// <summary>
        /// Valida se uma chave PIX é válida
        /// </summary>
        /// <param name="chavePix">Chave PIX para validar</param>
        /// <returns>True se válida</returns>
        bool ValidatePixKey(string chavePix);
    }

    /// <summary>
    /// Dados do pagamento PIX gerado
    /// </summary>
    public class PixPaymentData
    {
        public string PixCode { get; set; } = string.Empty;
        public string QRCodeBase64 { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Identificador { get; set; } = string.Empty;
        public string ChavePix { get; set; } = string.Empty;
        public DateTime DataVencimento { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
