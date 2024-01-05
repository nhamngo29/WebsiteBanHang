using Aloper.Models.ViewModel;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.Models;
using WebBanHang.Models.ViewModel;

namespace WebBanHang.DataAcess.Helpers
{
    public class AloperMapper : Profile
    {
        public AloperMapper()
        {
            CreateMap<User, UserViewModel>().ReverseMap();
            CreateMap<Product,CartItem>().ReverseMap();
        }
    }
}
