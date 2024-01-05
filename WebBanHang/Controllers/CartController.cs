using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAcess.Helpers;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models.ViewModel;

namespace WebBanHang.Controllers
{
    public class CartController(IUnitOfWork _IUnitOfWork) : Controller
    {
        public IActionResult Index()
        {
            return View(Cart);
        }
        const string CART_KEY = "MYCART";
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
        [HttpPost]
        public IActionResult AddToCart(string ID, int Quantity = 1)
        {
            var code = new { Success = false, msg = "", code = -1, count = 0, name="",image="" ,price=0.1};
            try
            {
                var gioHang = Cart;
                var item = gioHang.SingleOrDefault(t => t.ProductId == ID);
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
                        ProductId = hangHoa.ProductId,
                        Name = hangHoa.Name,
                        Price = hangHoa.Price ?? 0,
                        ImgeMain = hangHoa.ImgeMain ?? string.Empty,
                        Quantity = Quantity,
                    };
                    gioHang.Add(item);
                }
                else
                {
                    item.Quantity += Quantity;
                }
                HttpContext.Session.Set(CART_KEY, gioHang);

                code = new { Success = true, msg = "Them san pham vao gio hang thanh cong", code = 1, count = Cart.Sum(t => t.Quantity), name = item.Name, image = item.ImgeMain, price=item.Price};
                return Json(code);
            }
            catch (Exception)
            {
                code = new { Success = false, msg = "Them san pham vao gio hang khong thanh cong", code = 1, count = Quantity ,name="", image = "", price=0.0 };
                return Json(code);
                throw;
            }
        }
    }
}
