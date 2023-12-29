using Microsoft.AspNetCore.Mvc;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models.ViewModel;

namespace WebBanHang.Views.ViewComponents
{
    public class ProductTypeComponent:ViewComponent
    {
        private readonly IUnitOfWork _IUnitOfWork;
        public ProductTypeComponent(IUnitOfWork unitOfWork) =>_IUnitOfWork = unitOfWork;
        public IViewComponentResult Invoke()
        {
            var data = _IUnitOfWork.ProductType.GetAll().Select(t => new ProductTypeComponentViewModel (t.Id,t.Name,_IUnitOfWork.Product.GetCountProductByIDProductType(t.Id)));
            return View("ProductTypeComponent", data);
        }
        
    }
}
