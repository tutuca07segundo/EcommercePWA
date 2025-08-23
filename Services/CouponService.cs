using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public class CouponService : ICouponService
    {
        private readonly List<Coupon> _coupons;
        private int _nextId = 1;

        public CouponService()
        {
            _coupons = InitializeCoupons();
        }

        private List<Coupon> InitializeCoupons()
        {
            return new List<Coupon>
            {
                new Coupon
                {
                    Id = _nextId++,
                    Code = "WELCOME10",
                    Description = "10% de desconto para novos clientes",
                    DiscountPercentage = 10,
                    MinimumOrderAmount = 50,
                    ValidFrom = DateTime.Now.AddDays(-30),
                    ValidUntil = DateTime.Now.AddDays(30),
                    IsActive = true,
                    MaximumUses = 1000,
                    CurrentUses = 150
                },
                new Coupon
                {
                    Id = _nextId++,
                    Code = "FREESHIP",
                    Description = "Frete grátis para pedidos acima de R$ 200",
                    DiscountAmount = 25, // Valor do frete
                    MinimumOrderAmount = 200,
                    ValidFrom = DateTime.Now.AddDays(-15),
                    ValidUntil = DateTime.Now.AddDays(45),
                    IsActive = true,
                    MaximumUses = 500,
                    CurrentUses = 89
                },
                new Coupon
                {
                    Id = _nextId++,
                    Code = "GAMING20",
                    Description = "20% de desconto em produtos gaming",
                    DiscountPercentage = 20,
                    MinimumOrderAmount = 100,
                    ValidFrom = DateTime.Now.AddDays(-7),
                    ValidUntil = DateTime.Now.AddDays(23),
                    IsActive = true,
                    MaximumUses = 200,
                    CurrentUses = 67
                }
            };
        }

        public Coupon? GetCouponByCode(string code)
        {
            return _coupons.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        }

        public List<Coupon> GetAllCoupons()
        {
            return _coupons.ToList();
        }

        public Coupon CreateCoupon(Coupon coupon)
        {
            coupon.Id = _nextId++;
            coupon.CreatedAt = DateTime.Now;
            _coupons.Add(coupon);
            return coupon;
        }

        public void UpdateCoupon(Coupon coupon)
        {
            var existingCoupon = _coupons.FirstOrDefault(c => c.Id == coupon.Id);
            if (existingCoupon != null)
            {
                var index = _coupons.IndexOf(existingCoupon);
                _coupons[index] = coupon;
            }
        }

        public void DeleteCoupon(int id)
        {
            var coupon = _coupons.FirstOrDefault(c => c.Id == id);
            if (coupon != null)
            {
                _coupons.Remove(coupon);
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
    }
}
