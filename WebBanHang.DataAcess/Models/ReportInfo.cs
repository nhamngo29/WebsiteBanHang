using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.DataAcess.Models
{
    public class ReportInfo
    {
        public ReportInfo()
        {
            TypeExport = "";
            StoreName = "";
            PathName = null;
            Parameters = new List<ReportParameter>();
            Values = new List<ReportParameter>();
        }
        public string StoreName { get; set; }
        public string TypeExport { get; set; }
        public string PathName { get; set; }
        public string FileName { get; set; }
        public bool? ProcessMerge { get; set; }
        public List<ReportParameter> Parameters { get; set; }
        public List<ReportParameter> Values { get; set; }
        // Because nswag gencode fail
        public string pageName { get; set; }
        public string groupId { get; set; }
        public string TypePrint { get; set; }
    }
}
