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
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
            //_db.products.Include(u => u.Brand);

        }
        public void Update(Order imng)
        {
            _db.Orders.Update(imng);
        }
    }
}
