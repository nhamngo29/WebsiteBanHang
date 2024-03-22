using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.DataAcess.Procedures.Attributes
{
    public class StoreParamAttribute : Attribute
    {
        public StoreParamAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
