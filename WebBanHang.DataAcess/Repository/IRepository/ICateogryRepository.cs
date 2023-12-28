using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.Models;

namespace WebBanHang.DataAcess.Repository.IRepository
{
    public interface ICateogryRepository : IRepository<Category>
    {
        void Update(Category product);
    }
}
