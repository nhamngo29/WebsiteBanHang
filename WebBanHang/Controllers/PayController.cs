using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    public class PayController : Controller
    {
        private readonly IUnitOfWork _IUnitOfWork;
        public PayController(IUnitOfWork IUnitOfWork)
        {
            _IUnitOfWork = IUnitOfWork;
        }
        public IActionResult Index(string ID,int Quantity)
        {
            List<Product> a=new List<Product>();
            Product product=_IUnitOfWork.Product.GetFirstOrDefault(t=>t.ProductId== ID);
            a.Add(product);
            if (product==null)
            {
                return Redirect("/404");
            }    
            return View(a);
        }
    }
}
