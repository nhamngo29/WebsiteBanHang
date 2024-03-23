using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DataAcess.Models;
using WebBanHang.DataAcess.Report.Dto;

namespace WebBanHang.DataAcess.Asposes.ReportExporter
{
    public interface IReportExporter 
    {
        Task<MemoryStream> GetReportFile(ReportInfo info);
        MemoryStream GetReportFileFromHtml(ReportHtmlInfo info);
        Task<WorkbookDesigner> CreateExcelFileAndDesign(ReportInfo info);
        Task<MemoryStream> GetReportFileCustomFomart(ReportInfo info);
        Task<MemoryStream> GetReportFile_BCKH_CustomFomart(ReportInfo info);
        Task<MemoryStream> GetReportWordGroupFile(ReportInfo info);
        Task<MemoryStream> GetReportWordOneByOne(ReportInfo info, string byFiled);
        Task<MemoryStream> GetReportFileQR(ReportInfo info);

    }
}
