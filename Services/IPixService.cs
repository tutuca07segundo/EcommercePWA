using EcommercePWA.Models;

namespace EcommercePWA.Services
{

    public interface IPixService
    {

        Task<PixPaymentData> GeneratePixAsync(decimal valor, string descricao, string identificador);

        string GenerateQRCodeImage(string pixCode);

        bool ValidatePixKey(string chavePix);
    }

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
