using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Metadata;
using WebBanHang.DataAcess.Conts;
using WebBanHang.DataAcess.Models;
using WebBanHang.DataAcess.Procedures.Attributes;
using WebBanHang.Models;

namespace WebBanHang.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        private readonly IConfiguration _configuration;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration) : base(options)
        {
            _configuration=configuration;
        }
        public DbSet<Brand> brands { get; set; }
        public DbSet<Category> categories { get; set; }
        public DbSet<Product> products { get; set; }
        public DbSet<ImageProduct> images { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<Slide> Slides { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        private readonly int commandTimeout = 30;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ImageProduct>()
                  .HasKey(m => new { m.ProductId, m.Id });
            modelBuilder.Entity<IdentityRole>().Property(x => x.Id).HasMaxLength(50).IsRequired(true);
            modelBuilder.Entity<User>().Property(x => x.Id).HasMaxLength(50).IsRequired(true);
            modelBuilder.Entity<OrderDetail>().HasKey(m => new { m.IdOrder, m.IdProductt });
            SeedRoles(modelBuilder);
        }
        private static void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
                new IdentityRole() { Name = "User", ConcurrencyStamp = "2", NormalizedName = "User" },
                new IdentityRole() { Name = "HR", ConcurrencyStamp = "3", NormalizedName = "HR" },
                new IdentityRole() { Name = "Customer", ConcurrencyStamp = "4", NormalizedName = "Customer" }
                );
        }
        public async Task<List<StoreParameterInfoDto>> GetParameterInfos(string storeProcName)
        {
            using (var conn = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
            {
                var rr = await conn.QueryAsync<StoreParameterInfoDto>($"select PARAMETER_NAME, PARAMETER_MODE, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH from information_schema.parameters where specific_name = @storeProcName", new
                {
                    storeProcName
                });
                return rr.ToList();
            }
        }
        public async Task<List<dynamic>> GetMultiResultValueFromStore(string storedProcName, object parameters)
        {
            var list = await GetDataFromStoredProcedure<dynamic>(storedProcName, parameters);
            return list;
        }
        public async Task<DataSet> GetMultiDataFromStoredProcedure(string storedProcName, List<ReportParameter> parameters)
        {
           
            var parameterInfos = await GetParameterInfos(storedProcName);
            var dapperParams = new DynamicParameters();

            if (parameters != null)
            {

                List<StoreParameterInfoDto> procedureInfoInProperties = new List<StoreParameterInfoDto>();
                foreach (var property in parameters)
                {

                    var parameterInfo = GetParameterInfo(parameterInfos, property.Name);

                    if (parameterInfo == null)
                    {
                        continue;
                    }

                    procedureInfoInProperties.Add(parameterInfo);

                    dapperParams.Add(parameterInfo.PARAMETER_NAME, GetParameterValue(property.Value));
                }

                // add property not include in class parameters
                //foreach (var parameterInfo in parameterInfos.Where(x => !parameters.Any(pi => x.PARAMETER_NAME.ToLower().Replace("@", "").Replace("p_", "") == pi.Name.ToLower())))
                //{
                //    dapperParams.Add(parameterInfo.PARAMETER_NAME);
                //}
            }

            try
            {
                using (var conn = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
                {
                    var da = new SqlDataAdapter(storedProcName, conn);
                    var ds = new DataSet();

                    da.SelectCommand.CommandType = CommandType.StoredProcedure;

                    da.SelectCommand.CommandTimeout = commandTimeout;

                    foreach (var item in dapperParams.ParameterNames)
                    {
                        da.SelectCommand.Parameters.Add(new SqlParameter(item, dapperParams.Get<object>(item)));
                    }
                    //da.SelectCommand.CommandTimeout
                    da.Fill(ds);
                    return ds;

                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private string GetParameterName(PropertyInfo property)
        {
            var paramName = "";

            var storeParameterAttribute = (StoreParamAttribute)property.GetCustomAttributes(typeof(StoreParamAttribute), false).FirstOrDefault();

            if (storeParameterAttribute == null)
            {
                paramName = property.Name;
            }
            else
            {
                paramName = storeParameterAttribute.Name;
            }
            return "@" + paramName;
        }
        private StoreParameterInfoDto GetParameterInfo(List<StoreParameterInfoDto> parameterInfos, string paramName)
        {
            var result = parameterInfos
                .Where(x => x.PARAMETER_NAME.Replace("@", "").ToLower().Equals(paramName.Replace("@", "").ToLower())
                || x.PARAMETER_NAME.Replace("@", "").ToLower().Equals("p_" + paramName.Replace("@", "").ToLower())
                || x.PARAMETER_NAME.Replace("@", "").ToLower().Equals("l_" + paramName.Replace("@", "").ToLower()))
                .SingleOrDefault();
            return result;
        }
        private object GetParameterValue(object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value.GetType() == typeof(DateTime?))
            {
                return ((DateTime?)value).Value.ToString(WebBanHangCoreConst.DateTimeFormat);
            }
            if (value.GetType() == typeof(DateTime))
            {
                return ((DateTime)value).ToString(WebBanHangCoreConst.DateTimeFormat);
            }
            return value;
        }
        private ParameterDirection GetParameterDirection(StoreParameterInfoDto parameterInfo)
        {
            switch (parameterInfo.PARAMETER_MODE)
            {
                case ParameterSqlDirection.Input:
                    return ParameterDirection.Input;
                case ParameterSqlDirection.InputOutput:
                    return ParameterDirection.InputOutput;
                case ParameterSqlDirection.Output:
                    return ParameterDirection.Output;
            }
            return ParameterDirection.Input;
        }
        private object GetParameterValue(PropertyInfo property, object obj)
        {
            var value = property.GetValue(obj);
            return GetParameterValue(value);
        }
        public async Task<List<TModel>> GetDataFromStoredProcedure<TModel>(string storedProcName, object parameters) where TModel : class
        {
            var parameterInfos = await GetParameterInfos(storedProcName);
            var dapperParams = new DynamicParameters();
            var outputPropertyTable = new Dictionary<string, PropertyInfo>();

            if (parameters != null)
            {
                var properties = parameters.GetType().GetProperties().Where(x => x != null);

                List<StoreParameterInfoDto> procedureInfoInProperties = new List<StoreParameterInfoDto>();

                foreach (var property in properties)
                {
                    var paramName = GetParameterName(property);

                    var parameterInfo = GetParameterInfo(parameterInfos, paramName);

                    procedureInfoInProperties.Add(parameterInfo);

                    if (parameterInfo == null)
                    {
                        continue;
                    }

                    var direction = GetParameterDirection(parameterInfo);

                    if (direction == ParameterDirection.InputOutput || direction == ParameterDirection.Output)
                    {
                        outputPropertyTable.Add(parameterInfo.PARAMETER_NAME, property);
                    }

                    var parameterValue = GetParameterValue(property, parameters);

                    dapperParams.Add(parameterInfo.PARAMETER_NAME, parameterValue, null, direction);
                }

                var names = dapperParams.ParameterNames.ToList();
                foreach (var parameterInfo in parameterInfos)
                {
                    if (!names.Any(x => "@" + x == parameterInfo.PARAMETER_NAME))
                    {
                        dapperParams.Add(parameterInfo.PARAMETER_NAME, null, null, GetParameterDirection(parameterInfo));
                    }
                }

            }
            try
            {
                foreach (var item in dapperParams.ParameterNames)
                {
                    var tmp = item;
                    var value = dapperParams.Get<object>(tmp);
                }
                using (var conn = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
                {
                    //          var rr = await conn.QueryAsync<TModel>(storedProcName, dapperParams, null, null, System.Data.CommandType.StoredProcedure);
                    var rr = (List<TModel>)conn.Query<TModel>(storedProcName, dapperParams, null, true, commandTimeout, System.Data.CommandType.StoredProcedure);
                    foreach (var pair in outputPropertyTable)
                    {
                        pair.Value.SetValue(parameters, dapperParams.Get<object>(pair.Key));
                    }
                    return rr;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
