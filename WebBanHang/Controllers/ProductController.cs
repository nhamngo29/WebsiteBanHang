using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    public class ProductController(IUnitOfWork _IUnitOfWork) : Controller
    {
        public IActionResult Index()
        {
            var a = _IUnitOfWork.Product.GetAll(includeProperties: "ProductType");
            return View(a);
        }
        public ActionResult Detail(string id)
        {
            Product product =_IUnitOfWork.Product.GetFirstOrDefault(t=>t.ProductId == id, "ProductType,Brand");
            ViewBag.AllImg = _IUnitOfWork.Images.GetFilter(t=>t.ProductId == id).ToList();
            if (product != null)
            {
                if (product.Promotion > 0)
                {
                    ViewBag.Price = product.Price * (1 - product.Promotion * 0.01);
                }
                else
                {
                    ViewBag.Price = product.Price;
                }
            }
            ViewBag.ProductsSame = _IUnitOfWork.Product.GetFilter(t => t.ProductType.Id == product.ProductTypeID).ToList();
            return View(product);
        }
    }
}
