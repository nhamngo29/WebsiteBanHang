using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DataAcess.Conts;
using WebBanHang.DataAcess.Models;
using WebBanHang.DataAcess.Procedures.Attributes;
using static Dapper.SqlMapper;

namespace WebBanHang.DataAcess.Procedures.ProcedureHelpers
{
    public class ArgumentExceptionEx : ArgumentException
    {
        public int ErrorCode { get; }
        public string PropertyName { get; }

        public ArgumentExceptionEx(string paramName, int errorCode, string propertyName)
            : base(paramName)
        {
            ErrorCode = errorCode;
            PropertyName = propertyName;
        }
    }
    public class NullableDateTimeHandler : SqlMapper.TypeHandler<DateTime?>
    {
        public override void SetValue(IDbDataParameter parameter, DateTime? value)
        {
            if (value.HasValue)
                parameter.Value = value.Value;
            else
                parameter.Value = DBNull.Value;
        }

        public override DateTime? Parse(object value)
        {
            if (value == null || value is DBNull) return null;
            var typeofvalue = value.GetType();
            if (typeofvalue != typeof(DateTime) && typeofvalue != typeof(DateTime?))
            {
                return null;
            }
            return (DateTime)value;
        }
    }
    public class StoreProcedureProvider : IStoreProcedureProvider
    {
        private readonly IConfiguration _configuration;
        private readonly int commandTimeout = 30;

        public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ExternalConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public StoreProcedureProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            ConnectionString = _configuration["ConnectionStrings:DefaultConnection"];
        }

        #region Stored
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

        public async Task<IDictionary<string, object>> GetResultValueFromStore(string storedProcName, object parameters)
        {
            try
            {
                //_ = await ValidateParameter(storedProcName, parameters);

                var model = (IDictionary<string, object>)(await GetDataFromStoredProcedure<dynamic>(storedProcName, parameters)).FirstOrDefault();
                return model;
            }
            catch (ArgumentExceptionEx e)
            {
                if (e.ErrorCode == 101)
                {
                    Dictionary<string, object> error = new Dictionary<string, object>();

                    error.Add("Result", "-2");
                    error.Add("ErrorDesc", e.Message);
                    error.Add("PropertyName", e.PropertyName);

                    return error;
                }
                else
                {
                    Dictionary<string, object> error = new Dictionary<string, object>();

                    error.Add("Result", "-1");
                    error.Add("ErrorDesc", e.Message);

                    return error;
                }

            }
        }

        public async Task<int> ExecuteNonQuery(string storedProcName, object parameters)
        {
            return 0;
        }

        public string GetProcedureContent(string procedureName)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                return (string)((IDictionary<string, object>)conn.Query("SELECT OBJECT_DEFINITION (OBJECT_ID(N'" + procedureName + "')) as CONTENT").First())["CONTENT"];
            }
        }
        public async Task<GridReader> GetMultiData2(string storedProcName, object parameters, Func<GridReader, bool> setValueFunct, List<StoreParameterInfoDto> parameterInfos = null)
        {
            if (parameterInfos == null)
            {
                parameterInfos = await GetParameterInfos(storedProcName);
            }
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


                // add property not include in class parameters
                //foreach (var parameterInfo in parameterInfos.Where(x => x!=null && !procedureInfoInProperties.Any(pi => pi != null &&  x.PARAMETER_NAME.ToLower().Replace("@", "").Replace("p_", "") == pi.PARAMETER_NAME.ToLower().Replace("@", "").Replace("p_", ""))))
                //{
                //    dapperParams.Add(parameterInfo.PARAMETER_NAME);
                //}

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
                using (var conn = new SqlConnection(ConnectionString))
                {
                    //          var rr = await conn.QueryAsync<TModel>(storedProcName, dapperParams, null, null, System.Data.CommandType.StoredProcedure);
                    var rr = await conn.QueryMultipleAsync(storedProcName, dapperParams, null, commandTimeout, System.Data.CommandType.StoredProcedure);
                    foreach (var pair in outputPropertyTable)
                    {
                        pair.Value.SetValue(parameters, dapperParams.Get<object>(pair.Key));
                    }

                    setValueFunct?.Invoke(rr);
                    return rr;
                }
            }
            catch (Exception e)
            {
                throw;
            }



            //var parameterInfos = await GetParameterInfos(storedProcName);
            //var dapperParams = new DynamicParameters();

            //if (parameters != null)
            //{

            //    foreach (var property in parameters)
            //    {

            //        var parameterInfo = GetParameterInfo(parameterInfos, property.Name);

            //        if (parameterInfo == null)
            //        {
            //            continue;
            //        }


            //        dapperParams.Add(parameterInfo.PARAMETER_NAME, property.Value);
            //    }


            //    var names = dapperParams.ParameterNames.ToList();
            //    foreach (var parameterInfo in parameterInfos)
            //    {
            //        if (!names.Any(x => "@" + x == parameterInfo.PARAMETER_NAME))
            //        {
            //            dapperParams.Add(parameterInfo.PARAMETER_NAME, null);
            //        }
            //    }
            //}
            //try
            //{
            //    foreach (var item in dapperParams.ParameterNames)
            //    {
            //        var tmp = item;
            //        var value = dapperParams.Get<object>(tmp);
            //    }
            //    using (var conn = new SqlConnection(ConnectionString))
            //    {
            //        var rr = await conn.QueryMultipleAsync(storedProcName, dapperParams, null, commandTimeout, System.Data.CommandType.StoredProcedure);
            //        return rr;

            //    }
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}
        }

        public async Task<List<T>> GetDataQuery<T>(string query)
        {

            using (var conn = new SqlConnection(ConnectionString))
            {
                var rr = await conn.QueryAsync<T>(query);
                return rr.ToList();

            }
        }

        public async Task<dynamic> GetMultiSelect(string storedProcName, object parameters)
        {
            var parameterInfos = await GetParameterInfos(storedProcName);
            var dapperParams = new DynamicParameters();

            if (parameters != null)
            {
                var convertParameters = JObject.FromObject(parameters).ToObject<Dictionary<string, object>>();
                List<StoreParameterInfoDto> procedureInfoInProperties = new List<StoreParameterInfoDto>();
                foreach (var property in convertParameters)
                {

                    var parameterInfo = GetParameterInfo(parameterInfos, property.Key);

                    if (parameterInfo == null)
                    {
                        continue;
                    }

                    procedureInfoInProperties.Add(parameterInfo);

                    dapperParams.Add(parameterInfo.PARAMETER_NAME, GetParameterValue(property.Value));
                }
            }
            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    var da = new SqlDataAdapter(storedProcName, conn);
                    var ds = new DataSet();

                    da.SelectCommand.CommandType = CommandType.StoredProcedure; // type procedure

                    da.SelectCommand.CommandTimeout = commandTimeout; // set timeout

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

        public async Task<List<TModel>> GetExternalData<TModel>(string query) where TModel : class
        {
            using (var conn = new SqlConnection(ExternalConnectionString))
            {
                //          var rr = await conn.QueryAsync<TModel>(storedProcName, dapperParams, null, null, System.Data.CommandType.StoredProcedure);
                var rr = (List<TModel>)conn.Query<TModel>(query, System.Data.CommandType.Text);
                return rr;
            }
        }

        public async Task<DataSet> GetMultiExternalDataFromStoredProcedure(string storedProcName, List<ReportParameter> parameters)
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
                using (var conn = new SqlConnection(ExternalConnectionString))
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
        #endregion
    }
}
