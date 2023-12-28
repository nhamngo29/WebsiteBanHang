using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.Models
{
    public class User:IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? Identification { get; set; } = null!;
        //noi cap
        public string? IssuedBy { get; set; } = null!;
        // Ngay het han
        public DateTime? DateRange { get; set; }
        //Khóa tài khoản
        public bool? IsLocked { get; set; } = false;
        //Họ tên
        public string? FullName { get; set; } = null!;
        //Ngày sinh
        public DateTime? BOD { get; set; } = DateTime.UtcNow;
        //Địa chỉ trường chú
        public string? PermanentAddress { get; set; }
        [Required]
        public string Role { get; set; } = null!;
    }
}
