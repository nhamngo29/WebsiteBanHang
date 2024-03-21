using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAcess.Helpers;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    public class ProductController(IUnitOfWork _IUnitOfWork) : Controller
    {
        public IActionResult Index(int page=1,int? sort=1)
        {
            var Products = _IUnitOfWork.Product.GetAll(includeProperties: "ProductType").ToList();
            const int pageSize = 12;
            if (page < 1)
                page = 1;
            int resCount = Products.Count();
            var pager = new Pager(resCount, page, pageSize);
            if(pager.EndPage<page)
                page=pager.EndPage;
            int recSkip = (page - 1) * pageSize;
            var data = Products.Skip(recSkip).Take(pager.PageSize).ToList();
            switch (sort)
            {
                case 1:
                    data=data.OrderBy(x => x.Price).ToList();
                    break;
                case 2:
                    data = data.OrderByDescending(x => x.Price).ToList();
                    break;
                case 3:
                    data = data.OrderBy(x => x.Name).ToList();
                    break;
                case 4:
                    data = data.OrderByDescending(x => x.Name).ToList();
                    break;
                case 5:
                    data = data.OrderByDescending(x => x.DateCreate).ToList();
                    break;
                case 6:
                    data = data.OrderBy(x => x.DateCreate).ToList();
                    break;
                case 7:
                    data = data.OrderByDescending(x => x.TotalSold).ToList();
                    break;
                default:
                    data = data;
                    break;
            }
            this.ViewBag.Pager = pager;
            ViewBag.Sort=sort;  
            return View(data);
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
