using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAcess.Repository.IRepository;

namespace WebBanHang.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageProductsController : ControllerBase
    {
        private readonly IUnitOfWork _IUnitOfWork;
        public ImageProductsController(IUnitOfWork IUnitOfWork)
        {
            _IUnitOfWork = IUnitOfWork;
        }
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            return Ok(_IUnitOfWork.Images.GetAll());
        }
        [HttpGet("GetByID/{id}")]
        public IActionResult GetByID(string id)
        {
            return Ok(_IUnitOfWork.Images.GetFilter(t => t.ProductId == id));
        }
    }
}
