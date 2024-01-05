using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAcess.Helpers;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models;
using WebBanHang.Models.ViewModel;

namespace WebBanHang.Controllers
{
    public class PayController(IUnitOfWork _IUnitOfWork,IMapper mapper) : Controller
    {
      
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
            if(Cart!=null||Cart.Count>0)
                return View("Index", Cart);
            return BadRequest();
        }
    }
}
