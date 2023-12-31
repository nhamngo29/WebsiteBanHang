using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.Models;

namespace WebBanHang.DataAcess.Repository.IRepository
{
    public interface ISlideRepository : IRepository<Slide>
    {
        void Update(Slide s);
    }
}
