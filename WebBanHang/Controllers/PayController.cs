using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VnPayLibrary.Servirces;
using WebBanHang.DataAcess.Helpers;
using WebBanHang.DataAcess.Repository;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models;
using WebBanHang.Models.ViewModel;

namespace WebBanHang.Controllers
{
    public class PayController(IUnitOfWork _IUnitOfWork, IMapper mapper, PaypalClient _paypalClient, IVnPayServirces vnPayServirces) : Controller
    {
        private readonly PaypalClient paypalClient = _paypalClient;
        private readonly IVnPayServirces _vnPayservice = vnPayServirces;
        public IActionResult Index(string ID, int Quantity = 1)
        {
            ViewBag.PaypalClientdId = paypalClient.ClientId;
            Product product = _IUnitOfWork.Product.GetFirstOrDefault(t => t.ProductId == ID);
            if (product == null)
            {
                return Redirect("/404");
            }
            CartItem cart = mapper.Map<CartItem>(product);
            cart.Quantity = Quantity;
            List<CartItem> items = new List<CartItem>();
            items.Add(cart);
            return View(items);
        }
        const string CART_KEY = "MYCART";
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
        public IActionResult PayInSecction()
        {
            ViewBag.PaypalClientdId = paypalClient.ClientId;
            if (Cart != null || Cart.Count > 0)
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
            double totalPriceOrder = Cart.Sum(t => t.TotalPrice);
            double priceShip = (Cart.Sum(t => t.TotalPrice) > 500000 ? 0 : 25000);
            double totalPrice = totalPriceOrder + priceShip;
            Order order = new Order();
            order.Total = totalPriceOrder;
            order.Id = Guid.NewGuid().ToString();
            order.Ship = priceShip;
            order.TotalPrice = totalPrice;
            order.DateBooking = DateTime.Now;
            order.IdUser = null;
            order.Status = 1;
            _IUnitOfWork.Order.Add(order);
            foreach (var item in Cart)
            {
                OrderDetail a = new OrderDetail();
                a.IdProductt = item.ProductId;
                a.IdOrder = order.Id;
                a.Count = item.Quantity;
                a.Price = item.Price;
                _IUnitOfWork.OrderDetail.Add(a);
            }
            _IUnitOfWork.Save();
            HttpContext.Session.Remove("MYCART");
            return View();
        }

        #endregion
        #region VnPay
        public IActionResult PaymentFail()
        {
            return View();
        }
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayservice.PaymentExecute(Request.Query);
            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }
            return RedirectToAction("PaymentSuccess");
        }
        [HttpPost]
        public IActionResult CheckOut(string MethodPayment)
        {
            if (MethodPayment == "COD")
            {
                // Xử lý thanh toán COD
            }
            else if (MethodPayment == "VnPay")
            {
                // Xử lý thanh toán VnPay
                TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
                var vnPayModel = new VnPayMentRequestModel
                {
                    Amount = Cart.Sum(p => p.TotalPrice),
                    CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
                    Description = $"{"Nguyeenx Nham Ngo"} {"0779442612"}",
                    FullName = "Nguyeenx Nham Ngo",
                    OrderId = new Random().Next(1000, 100000)
                };
                return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
            }
            return null;
            // Rest of your code
        }
        #endregion
    }
}