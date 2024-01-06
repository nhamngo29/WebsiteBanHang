using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.Models.ViewModel
{
    public class DiaChi
    {
        [Display(Name = "Số nhà")]
        public string? Sonha { get; set; }
        [Display(Name = "Tên đường")]
        public string? Duong { get; set; }
        [Display(Name = "Tên phường")]
        public string? Phuong { get; set; }
        [Display(Name = "Tên quận")]
        public string? Quan { get; set; }
        [Display(Name = "Tên thành phố")]
        public string? Tp { get; set; }
        public DiaChi()
        {
        }
        public DiaChi(string sonha, string duong, string phuong, string quan, string tp)
        {
            Sonha = sonha;
            Duong = duong;
            Phuong = phuong;
            Quan = quan;
            Tp = tp;
        }
    }
}
