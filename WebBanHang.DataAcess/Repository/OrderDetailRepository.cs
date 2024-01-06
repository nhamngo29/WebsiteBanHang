using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.Data;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models;

namespace WebBanHang.DataAcess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {

        private readonly ApplicationDbContext _db;
        public OrderDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
            //_db.products.Include(u => u.Brand);

        }
        public void Update(OrderDetail imng)
        {
            _db.OrderDetails.Update(imng);
        }
    }
}
