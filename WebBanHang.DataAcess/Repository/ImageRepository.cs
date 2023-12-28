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
    public class ImageRepository : Repository<ImageProduct>, IImagesRepository
    {

        private readonly ApplicationDbContext _db;
        public ImageRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
            //_db.products.Include(u => u.Brand);

        }
        public void Update(ImageProduct imng)
        {
            _db.images.Update(imng);
        }
    }
}
