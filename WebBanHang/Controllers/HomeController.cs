using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebBanHang.DataAcess.Repository;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    public class HomeController(IUnitOfWork _IUnitOfWork) : Controller
    {
        public async Task<IActionResult> Index()
        {
            Category brand = new Category(1,"Hi");
            List<Category> test =await _IUnitOfWork.Cateogry.GetA(brand);
            List<Slide> Slides = _IUnitOfWork.Slide.GetFilter(t=>t.Active).ToList();
            ViewBag.Slides = Slides;
            ViewBag.Cout = Slides.Count();
            List<Product> Products = _IUnitOfWork.Product.GetAll(includeProperties: "ProductType").ToList();

            return View(Products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
