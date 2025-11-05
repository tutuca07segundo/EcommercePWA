using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers
{
    public class CouponController : Controller
    {
        private readonly ICouponService _couponService;
        private readonly ILogger<CouponController> _logger;

        public CouponController(ICouponService couponService, ILogger<CouponController> logger)
        {
            _couponService = couponService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var coupons = _couponService.GetAllCoupons();
            return View(coupons);
        }

        public IActionResult Details(int id)
        {
            var coupon = _couponService.GetAllCoupons().FirstOrDefault(c => c.Id == id);
            if (coupon == null)
                return NotFound();
            return View(coupon);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Coupon coupon)
        {
            try
            {
                _logger.LogInformation("Tentando criar cupom: {Code}", coupon.Code);

                if (string.IsNullOrWhiteSpace(coupon.Code))
                {
                    ModelState.AddModelError("Code", "O código do cupom é obrigatório");
                }

                if (string.IsNullOrWhiteSpace(coupon.Description))
                {
                    ModelState.AddModelError("Description", "A descrição do cupom é obrigatória");
                }

                var existingCoupon = _couponService.GetCouponByCode(coupon.Code);
                if (existingCoupon != null)
                {
                    ModelState.AddModelError("Code", "Já existe um cupom com este código");
                }

                if (coupon.ValidUntil <= coupon.ValidFrom)
                {
                    ModelState.AddModelError("ValidUntil", "A data de fim deve ser posterior à data de início");
                }

                if (ModelState.IsValid)
                {

                    if (coupon.DiscountAmount == 0 && (!coupon.DiscountPercentage.HasValue || coupon.DiscountPercentage == 0))
                    {
                        ModelState.AddModelError("", "É necessário informar um valor de desconto ou percentual");
                        return View(coupon);
                    }

                    coupon.CreatedAt = DateTime.Now;

                    var createdCoupon = _couponService.CreateCoupon(coupon);
                    
                    _logger.LogInformation("Cupom criado com sucesso: {Code}, ID: {Id}", createdCoupon.Code, createdCoupon.Id);
                    
                    TempData["SuccessMessage"] = $"Cupom '{coupon.Code}' criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogWarning("Falha na validação do cupom: {Code}. Erros: {Errors}", 
                        coupon.Code, string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar cupom: {Code}", coupon.Code);
                ModelState.AddModelError("", "Ocorreu um erro ao criar o cupom. Tente novamente.");
            }

            return View(coupon);
        }

        public IActionResult Edit(int id)
        {
            var coupon = _couponService.GetAllCoupons().FirstOrDefault(c => c.Id == id);
            if (coupon == null)
                return NotFound();
            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Coupon coupon)
        {
            if (id != coupon.Id)
                return NotFound();

            try
            {
                _logger.LogInformation("Tentando editar cupom: {Code}, ID: {Id}", coupon.Code, coupon.Id);

                if (string.IsNullOrWhiteSpace(coupon.Code))
                {
                    ModelState.AddModelError("Code", "O código do cupom é obrigatório");
                }

                if (string.IsNullOrWhiteSpace(coupon.Description))
                {
                    ModelState.AddModelError("Description", "A descrição do cupom é obrigatória");
                }

                var existingCoupon = _couponService.GetCouponByCode(coupon.Code);
                if (existingCoupon != null && existingCoupon.Id != coupon.Id)
                {
                    ModelState.AddModelError("Code", "Já existe um cupom com este código");
                }

                if (coupon.ValidUntil <= coupon.ValidFrom)
                {
                    ModelState.AddModelError("ValidUntil", "A data de fim deve ser posterior à data de início");
                }

                if (ModelState.IsValid)
                {

                    if (coupon.DiscountAmount == 0 && (!coupon.DiscountPercentage.HasValue || coupon.DiscountPercentage == 0))
                    {
                        ModelState.AddModelError("", "É necessário informar um valor de desconto ou percentual");
                        return View(coupon);
                    }

                    _couponService.UpdateCoupon(coupon);
                    
                    _logger.LogInformation("Cupom editado com sucesso: {Code}, ID: {Id}", coupon.Code, coupon.Id);
                    
                    TempData["SuccessMessage"] = $"Cupom '{coupon.Code}' editado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogWarning("Falha na validação do cupom: {Code}. Erros: {Errors}", 
                        coupon.Code, string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erro de validação ao editar cupom: {Code}. Erro: {Error}", coupon.Code, ex.Message);
                ModelState.AddModelError("", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operação inválida ao editar cupom: {Code}. Erro: {Error}", coupon.Code, ex.Message);
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao editar cupom: {Code}, ID: {Id}", coupon.Code, coupon.Id);
                ModelState.AddModelError("", "Ocorreu um erro inesperado ao editar o cupom. Tente novamente.");
            }

            return View(coupon);
        }

        public IActionResult Delete(int id)
        {
            var coupon = _couponService.GetAllCoupons().FirstOrDefault(c => c.Id == id);
            if (coupon == null)
                return NotFound();
            return View(coupon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var coupon = _couponService.GetAllCoupons().FirstOrDefault(c => c.Id == id);
                if (coupon == null)
                {
                    return NotFound();
                }

                _couponService.DeleteCoupon(id);
                
                _logger.LogInformation("Cupom excluído com sucesso: {Code}, ID: {Id}", coupon.Code, coupon.Id);
                
                TempData["SuccessMessage"] = $"Cupom '{coupon.Code}' excluído com sucesso!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir cupom ID: {Id}", id);
                TempData["ErrorMessage"] = "Ocorreu um erro ao excluir o cupom.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
