using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Text.RegularExpressions;

namespace EcommercePWA.Services
{

    public class PixService : IPixService
    {
        private readonly ILogger<PixService> _logger;
        private readonly IConfiguration _configuration;

        public PixService(ILogger<PixService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<PixPaymentData> GeneratePixAsync(decimal valor, string descricao, string identificador)
        {
            try
            {
                var chavePix = _configuration["Pix:ChavePix"] ?? "sua.chave@pix.com";
                var nomeRecebedor = _configuration["Pix:NomeRecebedor"] ?? "KABOOM ECOMMERCE";
                var cidadeRecebedor = _configuration["Pix:CidadeRecebedor"] ?? "SAO PAULO";

                var pixCode = GeneratePixEMVCode(chavePix, nomeRecebedor, cidadeRecebedor, valor, descricao, identificador);

                var qrCodeBase64 = GenerateQRCodeImage(pixCode);

                return new PixPaymentData
                {
                    PixCode = pixCode,
                    QRCodeBase64 = qrCodeBase64,
                    Valor = valor,
                    Descricao = descricao,
                    Identificador = identificador,
                    ChavePix = chavePix,
                    DataVencimento = DateTime.Now.AddMinutes(30), // PIX expira em 30 minutos
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar PIX");
                return new PixPaymentData
                {
                    Success = false,
                    ErrorMessage = "Erro interno ao gerar PIX"
                };
            }
        }

        public string GenerateQRCodeImage(string pixCode)
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(pixCode, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new QRCode(qrCodeData);
                using var qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, true);
                
                using var ms = new MemoryStream();
                qrCodeImage.Save(ms, ImageFormat.Png);
                var imageBytes = ms.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar QR Code");
                return string.Empty;
            }
        }

        public bool ValidatePixKey(string chavePix)
        {
            if (string.IsNullOrWhiteSpace(chavePix))
                return false;

            if (Regex.IsMatch(chavePix, @"^\d{11}$"))
                return true;

            if (Regex.IsMatch(chavePix, @"^\d{14}$"))
                return true;

            if (Regex.IsMatch(chavePix, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return true;

            if (Regex.IsMatch(chavePix, @"^\+55\d{10,11}$"))
                return true;

            if (Regex.IsMatch(chavePix, @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", RegexOptions.IgnoreCase))
                return true;

            return false;
        }

        private string GeneratePixEMVCode(string chavePix, string nomeRecebedor, string cidadeRecebedor, decimal valor, string descricao, string identificador)
        {
            var emv = new StringBuilder();

            emv.Append("000201");

            emv.Append("010212");

            var merchantInfo = $"0014br.gov.bcb.pix01{chavePix.Length:D2}{chavePix}";
            emv.Append($"26{merchantInfo.Length:D2}{merchantInfo}");

            emv.Append("52040000");

            emv.Append("5303986");

            if (valor > 0)
            {
                var valorStr = valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                emv.Append($"54{valorStr.Length:D2}{valorStr}");
            }

            emv.Append("5802BR");

            var nomeFormatado = nomeRecebedor.ToUpper().Substring(0, Math.Min(25, nomeRecebedor.Length));
            emv.Append($"59{nomeFormatado.Length:D2}{nomeFormatado}");

            var cidadeFormatada = cidadeRecebedor.ToUpper().Substring(0, Math.Min(15, cidadeRecebedor.Length));
            emv.Append($"60{cidadeFormatada.Length:D2}{cidadeFormatada}");

            var additionalData = new StringBuilder();

            if (!string.IsNullOrEmpty(identificador))
            {
                var txId = identificador.Substring(0, Math.Min(25, identificador.Length));
                additionalData.Append($"05{txId.Length:D2}{txId}");
            }

            if (!string.IsNullOrEmpty(descricao))
            {
                var desc = descricao.Substring(0, Math.Min(72, descricao.Length));
                additionalData.Append($"02{desc.Length:D2}{desc}");
            }

            if (additionalData.Length > 0)
            {
                emv.Append($"62{additionalData.Length:D2}{additionalData}");
            }

            emv.Append("6304");

            var crc = CalculateCRC16(emv.ToString());
            emv.Append(crc.ToString("X4"));

            return emv.ToString();
        }

        private ushort CalculateCRC16(string data)
        {
            const ushort polynomial = 0x1021;
            var bytes = Encoding.UTF8.GetBytes(data);
            ushort crc = 0xFFFF;

            foreach (var b in bytes)
            {
                crc ^= (ushort)(b << 8);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (ushort)((crc << 1) ^ polynomial);
                    else
                        crc <<= 1;
                }
            }

            return crc;
        }
    }
}
