using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAcess.Helpers;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models;
using WebBanHang.Models.ViewModel;

namespace WebBanHang.Controllers
{
    public class PayController(IUnitOfWork _IUnitOfWork,IMapper mapper,PaypalClient _paypalClient) : Controller
    {
        private readonly PaypalClient paypalClient = _paypalClient;
        public IActionResult Index(string ID,int Quantity=0)
        {
            Product product=_IUnitOfWork.Product.GetFirstOrDefault(t=>t.ProductId== ID);
            CartItem cart=mapper.Map<CartItem>(product);
            cart.Quantity=Quantity;
            List<CartItem> items=new List<CartItem>();
            items.Add(cart);
            if (product==null)
            {
                return Redirect("/404");
            }    
            return View(items);
        }
        const string CART_KEY = "MYCART";
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
        public IActionResult PayInSecction()
        {
            ViewBag.PaypalClientdId = paypalClient.ClientId;
            if(Cart!=null||Cart.Count>0)
                return View("Index", Cart);
            return BadRequest();
        }
        #region Paypal payment
        [HttpPost("/Pay/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            // Thông tin đơn hàng gửi qua Paypal
            var tongTien = Cart.Sum(p => p.TotalPrice).ToString();
            var donViTienTe = "USD";
            var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();
            try
            {
                var response = await _paypalClient.CreateOrder(tongTien, donViTienTe, maDonHangThamChieu);

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }
        [HttpPost("/Pay/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderId);

                // Lưu database đơn hàng của mình

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }
        public IActionResult PaymentSuccess()
        {
            HttpContext.Session.Remove("MYCART");
            return View();
        }    
        #endregion
    }
}
