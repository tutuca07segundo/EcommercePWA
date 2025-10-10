using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Text.RegularExpressions;

namespace EcommercePWA.Services
{
    /// <summary>
    /// Serviço para geração de PIX com QR Code
    /// </summary>
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

                // Gerar código PIX (EMV)
                var pixCode = GeneratePixEMVCode(chavePix, nomeRecebedor, cidadeRecebedor, valor, descricao, identificador);
                
                // Gerar QR Code
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

            // CPF (11 dígitos)
            if (Regex.IsMatch(chavePix, @"^\d{11}$"))
                return true;

            // CNPJ (14 dígitos)
            if (Regex.IsMatch(chavePix, @"^\d{14}$"))
                return true;

            // Email
            if (Regex.IsMatch(chavePix, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return true;

            // Telefone (+5511999999999)
            if (Regex.IsMatch(chavePix, @"^\+55\d{10,11}$"))
                return true;

            // Chave aleatória (UUID)
            if (Regex.IsMatch(chavePix, @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", RegexOptions.IgnoreCase))
                return true;

            return false;
        }

        /// <summary>
        /// Gera código PIX no formato EMV (padrão brasileiro)
        /// </summary>
        private string GeneratePixEMVCode(string chavePix, string nomeRecebedor, string cidadeRecebedor, decimal valor, string descricao, string identificador)
        {
            var emv = new StringBuilder();

            // Payload Format Indicator
            emv.Append("000201");

            // Point of Initiation Method (12 = QR Code dinâmico)
            emv.Append("010212");

            // Merchant Account Information
            var merchantInfo = $"0014br.gov.bcb.pix01{chavePix.Length:D2}{chavePix}";
            emv.Append($"26{merchantInfo.Length:D2}{merchantInfo}");

            // Merchant Category Code (0000 = não especificado)
            emv.Append("52040000");

            // Transaction Currency (986 = Real brasileiro)
            emv.Append("5303986");

            // Transaction Amount
            if (valor > 0)
            {
                var valorStr = valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                emv.Append($"54{valorStr.Length:D2}{valorStr}");
            }

            // Country Code
            emv.Append("5802BR");

            // Merchant Name
            var nomeFormatado = nomeRecebedor.ToUpper().Substring(0, Math.Min(25, nomeRecebedor.Length));
            emv.Append($"59{nomeFormatado.Length:D2}{nomeFormatado}");

            // Merchant City
            var cidadeFormatada = cidadeRecebedor.ToUpper().Substring(0, Math.Min(15, cidadeRecebedor.Length));
            emv.Append($"60{cidadeFormatada.Length:D2}{cidadeFormatada}");

            // Additional Data Field Template
            var additionalData = new StringBuilder();
            
            // Transaction ID
            if (!string.IsNullOrEmpty(identificador))
            {
                var txId = identificador.Substring(0, Math.Min(25, identificador.Length));
                additionalData.Append($"05{txId.Length:D2}{txId}");
            }

            // Description
            if (!string.IsNullOrEmpty(descricao))
            {
                var desc = descricao.Substring(0, Math.Min(72, descricao.Length));
                additionalData.Append($"02{desc.Length:D2}{desc}");
            }

            if (additionalData.Length > 0)
            {
                emv.Append($"62{additionalData.Length:D2}{additionalData}");
            }

            // CRC16 (será calculado)
            emv.Append("6304");

            // Calcular CRC16
            var crc = CalculateCRC16(emv.ToString());
            emv.Append(crc.ToString("X4"));

            return emv.ToString();
        }

        /// <summary>
        /// Calcula CRC16 para o código PIX
        /// </summary>
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
