using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.DataAcess.Repository;
using WebBanHang.DataAcess.Repository.IRepository;
using WebBanHang.Models;

namespace WebBanHang.Installers
{
    public class SystemInstrller : IInstaller
    {
        public void InstrallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllersWithViews();
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddDbContext<ApplicationDbContext>(options =>
              options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
            );
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;   // Không yêu cầu số
                options.Password.RequireLowercase = false;   // Không yêu cầu chữ thường
                options.Password.RequireUppercase = false;   // Không yêu cầu chữ hoa
                options.Password.RequireNonAlphanumeric = false;   // Không yêu cầu ký tự đặc biệt
                options.Password.RequiredLength = 6;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
