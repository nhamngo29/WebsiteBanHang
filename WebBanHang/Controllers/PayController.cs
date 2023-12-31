using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAcess.Repository.IRepository;

namespace WebBanHang.Controllers
{
    public class PayController : Controller
    {
        private readonly IUnitOfWork _IUnitOfWork;
        public PayController(IUnitOfWork IUnitOfWork)
        {
            _IUnitOfWork = IUnitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
