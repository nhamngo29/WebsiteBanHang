using WebBanHang.DataAcess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.Data;
using Microsoft.AspNetCore.Identity;
using WebBanHang.Models;
using Microsoft.Extensions.Configuration;
using AutoMapper;

namespace WebBanHang.DataAcess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public IProductRepository Product { get; private set; }
        public IUserRepsitory User { get; private set; }
        public IImagesRepository Images { get; private set; }
        public IBrandRepository Brand { get; private set; }
        public ICateogryRepository Cateogry { get; private set; }
        public IProductTypeRepository ProductType { get; private set; }

        public ISlideRepository Slide { get; private set; }

        public IOrderDetailRepository OrderDetail { get; private set; }

        public IOrderRepository Order { get; private set; }

        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _Mapper;

        public UnitOfWork(ApplicationDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, SignInManager<User> signInManager, IMapper mapper)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _signInManager = signInManager;
            _Mapper = mapper;
            Product = new ProductRepository(_db);
            Images = new ImageRepository(_db);
            User = new UserRepository(_userManager, _roleManager, _db, _configuration, _signInManager, _Mapper);
            Brand = new BrandRepository(_db);
            Cateogry=new CategoryRepository(_db);
            ProductType = new ProductTypeRepository(_db);
            Slide=new SlideRepository(_db);
            Order=new OrderRepository(_db); 
            OrderDetail=new OrderDetailRepository(_db); 
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
