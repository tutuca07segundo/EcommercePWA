using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public interface ICouponService
    {
        Coupon? GetCouponByCode(string code);
        List<Coupon> GetAllCoupons();
        Coupon CreateCoupon(Coupon coupon);
        void UpdateCoupon(Coupon coupon);
        void DeleteCoupon(int id);
        bool ValidateCoupon(string code, decimal orderAmount);
        Task InitializeDatabaseAsync();
    }
}
