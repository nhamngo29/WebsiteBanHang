using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebBanHang.DataAcess.Helpers;
using VnPayLibrary.Servirces;
using WebBanHang.DataAcess.Procedures.ProcedureHelpers;
using WebBanHang.DataAcess.Asposes.ReportExporter;
using WebBanHang.DataAcess.Asposes;
using WebBanHang.Installers;
using Aspose.Cells.Charts;
using Microsoft.AspNetCore.Identity;
using WebBanHang.Data;
using WebBanHang.DataAcess.Repository;
using WebBanHang.Models;
using Microsoft.EntityFrameworkCore;
using WebBanHang.DataAcess.Repository.IRepository;


var builder = WebApplication.CreateBuilder(args);


var configuration = builder.Configuration;
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(3000);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
//Aspose.Cells.License cellLicense = new Aspose.Cells.License();
//cellLicense.SetLicense(Directory.GetCurrentDirectory() + "/aspose-lic/Aspose.lic");
//Aspose.Words.License wordLicense = new Aspose.Words.License();
//wordLicense.SetLicense(Directory.GetCurrentDirectory() + "/aspose-lic/Aspose.Total.lic");
// Add services to the container.
builder.Services.InstallerServiceInAssembly(configuration);
builder.Services.AddAutoMapper(typeof(AloperMapper));
builder.Services.AddScoped<IStoreProcedureProvider,StoreProcedureProvider>();

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = configuration["JWT:ValidAudience"],
        ValidIssuer = configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
    };
});
builder.Services.AddSingleton(x => new PaypalClient(configuration["PaypalOptions:AppID"], configuration["PaypalOptions:AppSecret"], configuration["PaypalOptions:Mode"]));
builder.Services.AddSingleton<IVnPayServirces, VnPayServirces>();
builder.Services.AddMemoryCache();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Configure CORS policy
app.UseCors(options =>
{
    options.AllowAnyOrigin();
    options.AllowAnyMethod();
    options.AllowAnyHeader();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "DownloadPdf",
        pattern: "pdf/download/{fileToken}", // Định dạng URL bạn muốn sử dụng
        defaults: new { controller = "Pdf", action = "DownloadPdf" }
    );
});