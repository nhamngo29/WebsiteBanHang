using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebBanHang.DataAcess.Repository;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _IUnitOfWork;
        public HomeController(IUnitOfWork IUnitOfWork)
        {
            _IUnitOfWork = IUnitOfWork;
        }

        public IActionResult Index()
        {
            var a= _IUnitOfWork.Product.GetAll(includeProperties: "ProductType");
            return View(a);
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
