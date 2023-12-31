﻿using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAcess.Helpers;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models.ViewModel;

namespace WebBanHang.Controllers
{
    public class CartController : Controller
    {
        private readonly IUnitOfWork _IUnitOfWork;
        public CartController(IUnitOfWork IUnitOfWork)
        {
            _IUnitOfWork = IUnitOfWork;
        }
        public IActionResult Index()
        {
            return View(Cart);
        }
        const string CART_KEY = "MYCART";
        public List<CartItem> Cart=>HttpContext.Session.Get<List<CartItem>>(CART_KEY)??new List<CartItem>();
        [HttpPost]
        public IActionResult AddToCart(string ID, int Quantity = 1)
        {
            var code = new { Success = false, msg = "", code = -1, count = 0 };
            try
            {
                var gioHang = Cart;
                var item = gioHang.SingleOrDefault(t => t.IDProduct == ID);
                if (item == null)
                {
                    var hangHoa = _IUnitOfWork.Product.GetFirstOrDefault(t => t.ProductId == ID);
                    if (hangHoa == null)
                    {
                        TempData["Message"] = "Not fount product";
                        return Redirect("/404");
                    }
                    item = new CartItem
                    {
                        IDProduct = hangHoa.ProductId,
                        Name = hangHoa.Name,
                        Price = hangHoa.Price ?? 0,
                        Image = hangHoa.ImgeMain ?? string.Empty,
                        Quantity = Quantity,
                    };
                    gioHang.Add(item);
                }
                else
                {
                    item.Quantity += Quantity;
                }
                HttpContext.Session.Set(CART_KEY, gioHang);
                
                code = new { Success = true, msg = "Them san pham vao gio hang thanh cong", code = 1, count = Cart.Sum(t=>t.Quantity)};
                return Json(code);
            }
            catch (Exception)
            {
                code = new { Success = false, msg = "Them san pham vao gio hang khong thanh cong", code = 1, count = Quantity };
                return Json(code);
                throw;
            }
        }
    }
}
