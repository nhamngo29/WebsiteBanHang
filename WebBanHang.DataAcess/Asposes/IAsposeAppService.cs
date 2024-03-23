using Abp.Application.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.DataAcess.Models;

namespace WebBanHang.DataAcess.Asposes
{
    public interface IAsposeAppService : IApplicationService
    {
        public Task<FileDto> GetReport(ReportInfo info);
       

    }
}
