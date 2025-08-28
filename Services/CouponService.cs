using EcommercePWA.Models;
using EcommercePWA.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommercePWA.Services
{
    public class CouponService : ICouponService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CouponService> _logger;

        public CouponService(ApplicationDbContext context, ILogger<CouponService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Coupon? GetCouponByCode(string code)
        {
            // Buscar todos os cupons e fazer a comparação no cliente
            return _context.Coupons
                .ToList()
                .FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        }

        public List<Coupon> GetAllCoupons()
        {
            return _context.Coupons
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        public Coupon CreateCoupon(Coupon coupon)
        {
            try
            {
                _logger.LogInformation("Tentando criar cupom: {Code}", coupon.Code);

                // Validações adicionais
                if (string.IsNullOrWhiteSpace(coupon.Code))
                {
                    throw new ArgumentException("O código do cupom é obrigatório");
                }

                if (string.IsNullOrWhiteSpace(coupon.Description))
                {
                    throw new ArgumentException("A descrição do cupom é obrigatória");
                }

                _logger.LogInformation("Validações básicas passaram para cupom: {Code}", coupon.Code);

                // Verificar se já existe um cupom com o mesmo código
                var existingCoupon = GetCouponByCode(coupon.Code);
                if (existingCoupon != null)
                {
                    throw new InvalidOperationException($"Já existe um cupom com o código '{coupon.Code}'");
                }

                _logger.LogInformation("Verificação de duplicata passou para cupom: {Code}", coupon.Code);

                // Validar datas
                if (coupon.ValidUntil <= coupon.ValidFrom)
                {
                    throw new ArgumentException("A data de fim deve ser posterior à data de início");
                }

                _logger.LogInformation("Validação de datas passou para cupom: {Code}", coupon.Code);

                // Validar que pelo menos um tipo de desconto foi informado
                if (coupon.DiscountAmount == 0 && (!coupon.DiscountPercentage.HasValue || coupon.DiscountPercentage == 0))
                {
                    throw new ArgumentException("É necessário informar um valor de desconto ou percentual");
                }

                _logger.LogInformation("Validação de desconto passou para cupom: {Code}", coupon.Code);

                // Definir valores padrão
                coupon.CreatedAt = DateTime.Now;
                coupon.CurrentUses = 0;

                // Se MaximumUses for 0, definir como null (ilimitado)
                if (coupon.MaximumUses == 0)
                {
                    coupon.MaximumUses = null;
                }

                _logger.LogInformation("Valores padrão definidos para cupom: {Code}", coupon.Code);

                _context.Coupons.Add(coupon);
                _logger.LogInformation("Cupom adicionado ao contexto: {Code}", coupon.Code);
                
                _context.SaveChanges();
                _logger.LogInformation("Cupom salvo no banco de dados: {Code}, ID: {Id}", coupon.Code, coupon.Id);
                
                return coupon;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar cupom: {Code}. Stack trace: {StackTrace}", coupon.Code, ex.StackTrace);
                throw;
            }
        }

        public void UpdateCoupon(Coupon coupon)
        {
            try
            {
                _logger.LogInformation("Tentando editar cupom: {Code}, ID: {Id}", coupon.Code, coupon.Id);

                var existingCoupon = _context.Coupons.Find(coupon.Id);
                if (existingCoupon == null)
                {
                    throw new ArgumentException($"Cupom com ID {coupon.Id} não encontrado");
                }

                // Validações
                if (string.IsNullOrWhiteSpace(coupon.Code))
                {
                    throw new ArgumentException("O código do cupom é obrigatório");
                }

                if (string.IsNullOrWhiteSpace(coupon.Description))
                {
                    throw new ArgumentException("A descrição do cupom é obrigatória");
                }

                // Verificar se já existe outro cupom com o mesmo código
                var duplicateCoupon = GetCouponByCode(coupon.Code);
                if (duplicateCoupon != null && duplicateCoupon.Id != coupon.Id)
                {
                    throw new InvalidOperationException($"Já existe um cupom com o código '{coupon.Code}'");
                }

                // Validar datas
                if (coupon.ValidUntil <= coupon.ValidFrom)
                {
                    throw new ArgumentException("A data de fim deve ser posterior à data de início");
                }

                // Validar que pelo menos um tipo de desconto foi informado
                if (coupon.DiscountAmount == 0 && (!coupon.DiscountPercentage.HasValue || coupon.DiscountPercentage == 0))
                {
                    throw new ArgumentException("É necessário informar um valor de desconto ou percentual");
                }

                // Se MaximumUses for 0, definir como null (ilimitado)
                if (coupon.MaximumUses == 0)
                {
                    coupon.MaximumUses = null;
                }

                // Atualizar propriedades
                existingCoupon.Code = coupon.Code;
                existingCoupon.Description = coupon.Description;
                existingCoupon.DiscountAmount = coupon.DiscountAmount;
                existingCoupon.DiscountPercentage = coupon.DiscountPercentage;
                existingCoupon.MinimumOrderAmount = coupon.MinimumOrderAmount;
                existingCoupon.MaximumUses = coupon.MaximumUses;
                existingCoupon.ValidFrom = coupon.ValidFrom;
                existingCoupon.ValidUntil = coupon.ValidUntil;
                existingCoupon.IsActive = coupon.IsActive;

                _context.SaveChanges();

                _logger.LogInformation("Cupom editado com sucesso: {Code}, ID: {Id}", coupon.Code, coupon.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao editar cupom: {Code}, ID: {Id}", coupon.Code, coupon.Id);
                throw;
            }
        }

        public void DeleteCoupon(int id)
        {
            try
            {
                _logger.LogInformation("Tentando excluir cupom ID: {Id}", id);

                var coupon = _context.Coupons.Find(id);
                if (coupon == null)
                {
                    throw new ArgumentException($"Cupom com ID {id} não encontrado");
                }

                _context.Coupons.Remove(coupon);
                _context.SaveChanges();

                _logger.LogInformation("Cupom excluído com sucesso: {Code}, ID: {Id}", coupon.Code, coupon.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir cupom ID: {Id}", id);
                throw;
            }
        }

        public bool ValidateCoupon(string code, decimal orderAmount)
        {
            var coupon = GetCouponByCode(code);
            if (coupon == null || !coupon.IsActive)
                return false;

            if (DateTime.Now < coupon.ValidFrom || DateTime.Now > coupon.ValidUntil)
                return false;

            if (coupon.MaximumUses.HasValue && coupon.CurrentUses >= coupon.MaximumUses.Value)
                return false;

            if (coupon.MinimumOrderAmount.HasValue && orderAmount < coupon.MinimumOrderAmount.Value)
                return false;

            return true;
        }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                // Verificar se já existem cupons
                if (!_context.Coupons.Any())
                {
                    var initialCoupons = new List<Coupon>
                    {
                        new Coupon
                        {
                            Code = "WELCOME10",
                            Description = "10% de desconto para novos clientes",
                            DiscountPercentage = 10,
                            MinimumOrderAmount = 50,
                            ValidFrom = DateTime.Now.AddDays(-30),
                            ValidUntil = DateTime.Now.AddDays(30),
                            IsActive = true,
                            MaximumUses = 1000,
                            CurrentUses = 150,
                            CreatedAt = DateTime.Now
                        },
                        new Coupon
                        {
                            Code = "FREESHIP",
                            Description = "Frete grátis para pedidos acima de R$ 200",
                            DiscountAmount = 25, // Valor do frete
                            MinimumOrderAmount = 200,
                            ValidFrom = DateTime.Now.AddDays(-15),
                            ValidUntil = DateTime.Now.AddDays(45),
                            IsActive = true,
                            MaximumUses = 500,
                            CurrentUses = 89,
                            CreatedAt = DateTime.Now
                        },
                        new Coupon
                        {
                            Code = "GAMING20",
                            Description = "20% de desconto em produtos gaming",
                            DiscountPercentage = 20,
                            MinimumOrderAmount = 100,
                            ValidFrom = DateTime.Now.AddDays(-7),
                            ValidUntil = DateTime.Now.AddDays(23),
                            IsActive = true,
                            MaximumUses = 200,
                            CurrentUses = 67,
                            CreatedAt = DateTime.Now
                        }
                    };

                    _context.Coupons.AddRange(initialCoupons);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Cupons iniciais criados com sucesso");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar cupons");
                throw;
            }
        }
    }
}
