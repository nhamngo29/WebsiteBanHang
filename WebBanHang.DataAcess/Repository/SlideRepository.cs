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
    public class SlideRepository : Repository<Slide>,ISlideRepository
    {
        private readonly ApplicationDbContext _db;
        public SlideRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
            //_db.products.Include(u => u.Brand);
        }

        public void Update(Slide s)
        {
            _db.Slides.Update(s);
        }
    }
}
