using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.DataAcess.Models
{
    public class ReportParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public ReportParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
