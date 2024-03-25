using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.Data;
using WebBanHang.DataAcess.Paramets;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models;
using WebBanHang.Models.ViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebBanHang.DataAcess.Repository
{
    internal class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
            //_db.products.Include(u => u.Brand);
        }
        public void Update(Product product)
        {
            _db.products.Update(product);
        }
        public int GetCountProductByIDProductType(int type)
        {
            return _db.products.Where(t=>t.ProductTypeID == type).Count();
        }
        public async Task<(List<ProductViewModel>, int)> SearchProductAsync(Product_p a)
        {
            int @TotalRecord = 0;
            var ddataa = await _db.GetDataFromStoredProcedure<ProductViewModel>("Search_Product", new
            {
                Search = a.Search,
                SoTrang = a.SoTrang,
                SoRecordMoiTrang = a.SoRecordMoiTrang
            });
            //var ddataa = await _db.GetDataFromStoredProcedure<ProductViewModel>("Search_Product", a);
            TotalRecord = a.TotalRecord;
            return (ddataa, TotalRecord);
        }
    }
}
