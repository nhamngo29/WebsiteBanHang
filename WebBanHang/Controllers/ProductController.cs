using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAcess.Helpers;
using WebBanHang.DataAcess.Paramets;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models;
using WebBanHang.Models.ViewModel;

namespace WebBanHang.Controllers
{
    public class ProductController(IUnitOfWork _IUnitOfWork) : Controller
    {
        private readonly int pageSize = 12;
        public async Task<IActionResult> Index(int page=1,int? sort=1)
        {
            Product_p a=new Product_p();
            a.SoRecordMoiTrang = pageSize;
            a.SoTrang = page;
            int totalRecord = 0;
            
            List<ProductViewModel> Products;
            (Products, totalRecord) = await _IUnitOfWork.Product.SearchProductAsync(a);

            if (page < 1)
                page = 1;
            int resCount = totalRecord;
            var pager = new Pager(resCount, page, pageSize);
            if(pager.EndPage<page)
                page=pager.EndPage;
            int recSkip = (page - 1) * pageSize;
            switch (sort)
            {
                case 1:
                    Products = Products.OrderBy(x => x.Price).ToList();
                    break;
                case 2:
                    Products = Products.OrderByDescending(x => x.Price).ToList();
                    break;
                case 3:
                    Products = Products.OrderBy(x => x.Name).ToList();
                    break;
                case 4:
                    Products = Products.OrderByDescending(x => x.Name).ToList();
                    break;
                case 5:
                    Products = Products.OrderByDescending(x => x.DateCreate).ToList();
                    break;
                case 6:
                    Products = Products.OrderBy(x => x.DateCreate).ToList();
                    break;
                case 7:
                    Products = Products.OrderByDescending(x => x.TotalSold).ToList();
                    break;
                default:
                    Products = Products;
                    break;
            }
            this.ViewBag.Pager = pager;
            ViewBag.Sort=sort;  
            return View(Products);
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
