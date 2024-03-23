using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using WebBanHang.DataAcess.Models;
using WebBanHang.DataAcess.Procedures.Attributes;

namespace WebBanHang.DataAcess.Procedures.ProcedureHelpers
{
    public interface IStoreProcedureProvider
    {
        string ConnectionString { get; set; }
        string ExternalConnectionString { get; set; }
        Task<List<TModel>> GetDataFromStoredProcedure<TModel>(string storedProcName, object parameters) where TModel : class;
        Task<IDictionary<string, object>> GetResultValueFromStore(string storedProcName, object parameters);
        Task<DataSet> GetMultiDataFromStoredProcedure(string storedProcName, List<ReportParameter> parameters);
        Task<int> ExecuteNonQuery(string storedProcName, object parameters);
        Task<List<dynamic>> GetMultiResultValueFromStore(string storedProcName, object parameters);
        //Task<PagedResultDto<TModel>> GetPagingData<TModel>(string storedProcName, object parameters) where TModel : class;
        string GetProcedureContent(string procedureName);
        Task<GridReader> GetMultiData2(string storedProcName, object parameters = null, Func<GridReader, bool> setValueFunct = null, List<StoreParameterInfoDto> parameterInfos = null);
        Task<List<T>> GetDataQuery<T>(string query);
        //TienLee 5/03/22
        Task<dynamic> GetMultiSelect(string storedProcName, object parameters);
        Task<List<TModel>> GetExternalData<TModel>(string query) where TModel : class;
        Task<DataSet> GetMultiExternalDataFromStoredProcedure(string storedProcName, List<ReportParameter> parameters);
    }
}
