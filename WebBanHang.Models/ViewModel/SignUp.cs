using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.Models.ViewModel
{
    public class SignUp
    {
        [Required(ErrorMessage = "User is required")]
        public string UserName { get; set; } = null!;
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = null!;
        [Phone]
        [Required(ErrorMessage = "PhoneNumber is required")]
        public string PhoneNumber { get; set; } = null!;
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = null!;
        [Required(ErrorMessage = "ID telegram is required")]
        public string TelegramID { get; set; } = null!;
        [Required(ErrorMessage = "Identification is required")]
        public string Identification { get; set; } = null!;
        public byte[]? Signature { get; set; } = null!;
        [Required(ErrorMessage = "Issued By is required")]
        public string IssuedBy { get; set; } = null!;
        [Required(ErrorMessage = "Date Range is required")]
        public string Role { get; set; } = null!;
        [Required(ErrorMessage = "Account Number is required")]
        public string FullName { get; set; } = null!;
        //tên chủ tài khoản
        public string? AccountName { get; set; } = null!;
        public string? Position { get; set; } = null!;
        public DateTime? bOD { get; set; } = DateTime.UtcNow;
        public string AccountNumber { get; set; } = null!;
        public string? PermanentAddress { get; set; } = null!;
        [Required(ErrorMessage = "Bank Code is required")]
        public string BankCode { get; set; } = null!;
        [ForeignKey("OwnerID")]
        public DateTime? DateRange { get; set; }
        public string? OwnerID { get; set; } = null!;
    }
}
