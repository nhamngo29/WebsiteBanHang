using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAcess.Helpers;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models.ViewModel;

namespace WebBanHang.Views.ViewComponents
{
    public class CartComponent : ViewComponent
    {
        private readonly IUnitOfWork _IUnitOfWork;
        public CartComponent(IUnitOfWork unitOfWork) => _IUnitOfWork = unitOfWork;
        public IViewComponentResult Invoke()
        {
            int cartItemCount = GetCartItemCount();
            return View("CartComponent", cartItemCount);
        }
        const string CART_KEY = "MYCART";
        private int GetCartItemCount()
        {
            // Thực hiện logic để lấy số lượng sản phẩm trong giỏ hàng từ Session hoặc nơi lưu trữ khác
            // Điều này có thể tương tự như phương thức CartController.Cart được hiện thị trong câu hỏi trước đó
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            return cart.Sum(item => item.Quantity);
        }
    }
}
