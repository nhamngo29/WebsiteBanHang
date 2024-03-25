
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DataAcess.Paramets;
using WebBanHang.Models;
using WebBanHang.Models.ViewModel;

namespace WebBanHang.DataAcess.Repository.IRepository
{
    public interface IProductRepository:IRepository<Product>
    {
        void Update(Product product);
        int GetCountProductByIDProductType(int type);
        public Task<(List<ProductViewModel>, int)> SearchProductAsync(Product_p a);
    }
}
