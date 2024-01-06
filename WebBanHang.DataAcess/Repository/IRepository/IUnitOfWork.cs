using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.DataAcess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IProductRepository Product { get; }
        IUserRepsitory User { get; }
        IImagesRepository Images { get; }
        ICateogryRepository Cateogry { get; }
        IBrandRepository Brand { get; }
        IProductTypeRepository ProductType { get; }
        ISlideRepository Slide { get; }
        IOrderDetailRepository OrderDetail { get; }
        IOrderRepository Order {  get; }
        void Save();
    }
}
