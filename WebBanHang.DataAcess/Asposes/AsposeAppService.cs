using Abp.AspNetZeroCore.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DataAcess.Asposes.ReportExporter;
using WebBanHang.DataAcess.Models;
using WebBanHang.DataAcess.Report.Dto;
using WebBanHang.DataAcess.Storage;

namespace WebBanHang.DataAcess.Asposes
{
    public class AsposeAppService
    {
        private readonly IReportExporter _customReportFile;
        private readonly ITempFileCacheManager _tempFileCacheManager;
        public async Task<FileDto> GetReport(ReportInfo info)
        {
            try
            {
                // TIENLEE 28-06-2022 Vá lỗi Directory Traversal
                if (info.PathName != "")
                {
                    info.PathName = info.PathName.Replace(@"/..", @"").Replace(@"..", @"/").Replace(@"//", @"/");
                }
                var reportByteArray = await _customReportFile.GetReportFile(info);
                FileDto file = new FileDto();

                var fileName = info.PathName.Substring(info.PathName.LastIndexOf("/") + 1);

                if (info.FileName != null)
                {
                    fileName = info.FileName;
                }
                fileName = fileName.Substring(0, fileName.LastIndexOf(".")) + "-" + Guid.NewGuid().ToString();

                switch (info.TypeExport.ToLower())
                {
                    case FileTypeConst.Excel:
                        file = new FileDto(fileName + ".xlsx", MimeTypeNames.ApplicationVndMsExcel);
                        break;
                    case FileTypeConst.Pdf:
                        file = new FileDto(fileName + ".pdf", MimeTypeNames.ApplicationPdf);
                        break;
                    case FileTypeConst.Word:
                        file = new FileDto(fileName + ".docx", MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentWordprocessingmlDocument);
                        break;
                }

                _tempFileCacheManager.SetFile(file.FileToken, reportByteArray.ToArray());
                return file;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
