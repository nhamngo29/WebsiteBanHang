using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.Models.ViewModel
{
    public class CheckOutViewModel
    {
        public string? FullNameRecipient { get; set; }
        public int City { get; set; }
        public int District { get; set; }
        public int Ward { get; set; }
        public int PhoneNumber { get; set; }
        public string Adress { get; set; }
        public string Note { get; set; }
    }
}
