using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.Models;

namespace WebBanHang.DataAcess.Repository.IRepository
{
    public interface IOrderRepository : IRepository<Order>
    {
        void Update(Order img);
    }
}
